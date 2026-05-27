# CrossWaves — Pattern-Based Crossword Generation

**Implementation plan for Claude Code.**

This document describes how to replace the current greedy seed-and-grow crossword generator with a database-driven pattern + CSP fill approach. Read fully before starting; the algorithm changes touch four existing scripts and add four new ones.

---

## Error handling directive (read first)

When implementing this plan, **never silently swallow errors or return `null` without logging the cause**. Every failure path must produce a clear, actionable message in the Editor console, or throw an exception with descriptive context.

**Rules:**

- **Missing or malformed input** (file not found, JSON parse failure, empty database, wrong grid dimensions): log with `Debug.LogError`, including the file path / class / method, then either throw or return null — but never return null without logging first.
- **Recoverable degraded state** (one of two databases missing but the other loaded, low word availability): log with `Debug.LogWarning` and continue.
- **Broken invariants** (null passed where required, grid with non-13×13 cells, word slot of length < 3, unknown difficulty enum): throw `ArgumentNullException` / `ArgumentException` / `InvalidOperationException` with a message describing what was expected vs. received.
- **Caught exceptions**: never catch just to suppress. If you catch, either rethrow or log with the full exception (`Debug.LogException(ex)`).
- **API boundaries**: public methods accepting external input must validate and throw on bad input, not silently early-return.
- **Legitimate empty results** (selector finds no available grids, CSP can't fit a particular grid) are not errors — but the caller decision based on that result must be logged at least at warning level so the operator knows why generation slowed or stopped.

**Examples**

❌ Don't do this:
```csharp
if (template == null) return null;
var json = File.ReadAllText(path);  // throws unhelpful IOException if path is wrong
```

✓ Do this:
```csharp
if (template == null) {
    Debug.LogError("[WordFitter.Fit] called with null GridTemplate.");
    throw new ArgumentNullException(nameof(template));
}

if (!File.Exists(path)) {
    Debug.LogError($"[GridDatabase] Grid file not found: {path}. " +
                   $"Place the JSON in {Application.persistentDataPath}/Grids/.");
    return new List<GridTemplate>();  // returning empty is OK here — caller can handle
}
```

✓ Legitimate "no result" case:
```csharp
var grid = selector.PickRandom(transient);
if (grid == null) {
    Debug.LogWarning($"[CrosswordGenerator] No grids available on attempt {attempt}. " +
                     $"Used: {tracker.UsedCount}/{db.TotalCount}, transient excluded: {transient.Count}. " +
                     $"Resetting tracker.");
    tracker.Reset();
    // ... retry
}
```

Apply this consistently to every script touched by this plan.

---

## 1. Why we're changing

`CrosswordGenerator.cs` currently uses greedy seed-and-grow: pick a seed cell, place a word, then for each next word find an intersection with placed words and squeeze it in subject to no-butt-adjacency / no-parallel-neighbour / require-intersection rules. This **biases every puzzle toward a dense central blob with stub tendrils** — visually every output looks the same regardless of seed or word choices. Preferential attachment is inherent to the algorithm; randomising seed/direction can't fix it.

The new approach **decouples grid topology from word placement**:

1. **Stage 1 — Grid selection.** Pick a 13×13 pattern (which cells are letter cells) from one of two pre-built databases.
2. **Stage 2 — Word fitting.** Treat slot-filling as a CSP: variables = word slots, domains = words matching length, constraints = intersecting letters must agree. Backtracking search fills the grid.

Visual variety comes from the grid database. Word selection is just constraint satisfaction. The two are independent concerns.

---

## 2. Scope

**Editor-time only.** Per the architecture, `CrosswordGenerator.cs` runs in the Unity Editor and serialises completed puzzles to `Application.persistentDataPath`. Runtime (`CrosswordManager.cs`) reads finished puzzle JSONs. Therefore:

- All new pattern/CSP code is **editor-time** (gated by `#if UNITY_EDITOR` where it pulls in Editor-only APIs, or kept plain C# and called from an Editor menu).
- The runtime puzzle JSON schema **does not change**. Existing puzzles keep loading. New puzzles use the same schema, just with cell layouts that come from the grid database instead of seed-and-grow.
- `CrosswordManager.cs` likely needs no logic changes, but may need cosmetic tweaks (Section 8) since the new grids are visually different — verify how it renders inactive cells.

---

## 3. Files to add

### Data files

User will place these in `Application.persistentDataPath/Grids/`:

- `grid_templates_normal.json` — 200 standard rectangular crosswords (180° rotational symmetry, varied block densities; 20–40 blocks each, 37–59 words each).
- `grid_templates_fun.json` — fun-shape silhouettes (bilateral symmetry; everyday objects, letters, geometric shapes, etc.). Currently ~50, will grow over time.

Both share this schema:

```json
{
  "version": 1,
  "kind": "normal" | "fun",
  "size": 13,
  "count": <N>,
  "templates": [
    {
      "name": "string id, e.g. 'normal_000' or 'heart'",
      "size": 13,
      "cells": [[0,1,...],[...]],   // 13 rows × 13 cols, row-major, 1 = active
      "stats": {
        "active_cells": <int>,
        "h_words": <int>,
        "v_words": <int>,
        "total_words": <int>
      }
    }
  ]
}
```

Generator must handle either or both files being missing (warn, skip that database).

### New C# scripts

Place in `Assets/Scripts/Generation/`:

- `GridTemplate.cs` — data class for one grid + slot extraction
- `GridDatabase.cs` — loads & holds both JSONs
- `UsedGridsTracker.cs` — persists which grids have been used
- `GridSelector.cs` — random selection w/ used-tracking
- `WordFitter.cs` — CSP backtracking to fill slots
- `WordAvailabilityMonitor.cs` — warnings when word library is thin

### Scripts to modify

- `Assets/Scenes/CrosswordGenerator.cs` — replace generation loop
- `Assets/Scripts/CrosswordDatabase.cs` — add lookup helpers (see 5.f)
- `Assets/Scripts/CrosswordUtils.cs` — minor: slot↔cell helpers if not present
- `Assets/Scripts/CrosswordManager.cs` — verify rendering of sparse grids (likely no logic change; see Section 8)

---

## 4. New flow at a glance

```
GenerateOne(difficulty):
    transientExcluded = {}                  # grids that failed THIS call only
    for attempt in 1..MAX_GRID_ATTEMPTS:    # MAX_GRID_ATTEMPTS = 20
        grid = selector.PickRandom(transientExcluded)
        if grid == null:
            tracker.Reset()                 # exhausted both DBs
            grid = selector.PickRandom(transientExcluded)
            if grid == null: return ERROR_NO_GRIDS

        if grid.HCount < MIN_H or grid.VCount < MIN_V:
            transientExcluded.add(grid.compositeKey)
            continue                        # not even worth trying

        puzzle = fitter.Fit(grid, difficulty, maxBacktracks=10000)
        if puzzle == null:
            transientExcluded.add(grid.compositeKey)
            continue                        # CSP couldn't fill it

        tracker.Mark(grid.compositeKey)
        tracker.Save()
        return puzzle

    return ERROR_RAN_OUT_OF_ATTEMPTS
```

Constants (tunable):
- `MIN_H = 12`, `MIN_V = 12` — minimum across and down words required.
- `MAX_GRID_ATTEMPTS = 20` — how many grids we'll try before giving up on one generation call.
- `MAX_BACKTRACKS = 10000` — CSP search budget per grid.
- `COOLDOWN = 5` — most recent N grids preserved across a tracker reset (so the same grid never appears twice in a row).

These should live in a `GenerationConfig` struct/ScriptableObject so they're tunable without recompiling logic.

---

## 5. Component details

### 5.a `GridTemplate.cs`

```csharp
[Serializable]
public class GridTemplate {
    public string name;
    public int size;          // always 13
    public int[][] cells;     // [row][col]; 1 = active letter cell, 0 = blank
    public GridStats stats;

    [NonSerialized] public string databaseKind;   // "normal" or "fun"
    public string CompositeKey => $"{databaseKind}:{name}";

    public bool IsActive(int r, int c) =>
        r >= 0 && r < size && c >= 0 && c < size && cells[r][c] == 1;

    /// Extracts contiguous runs of active cells of length >= minLen.
    public List<WordSlot> ExtractSlots(int minLen = 3) {
        var slots = new List<WordSlot>();
        // Horizontal
        for (int r = 0; r < size; r++) {
            int c = 0;
            while (c < size) {
                if (cells[r][c] == 1) {
                    int s = c;
                    while (c < size && cells[r][c] == 1) c++;
                    int len = c - s;
                    if (len >= minLen)
                        slots.Add(new WordSlot(r, s, len, true));
                } else c++;
            }
        }
        // Vertical
        for (int c = 0; c < size; c++) {
            int r = 0;
            while (r < size) {
                if (cells[r][c] == 1) {
                    int s = r;
                    while (r < size && cells[r][c] == 1) r++;
                    int len = r - s;
                    if (len >= minLen)
                        slots.Add(new WordSlot(s, c, len, false));
                } else r++;
            }
        }
        return slots;
    }
}

[Serializable]
public class GridStats {
    public int active_cells, h_words, v_words, total_words;
}

public class WordSlot {
    public int row, col, length;
    public bool isHorizontal;
    public WordSlot(int r, int c, int len, bool isH) {
        row = r; col = c; length = len; isHorizontal = isH;
    }
    public (int r, int c) CellAt(int i) =>
        isHorizontal ? (row, col + i) : (row + i, col);
}
```

### 5.b `GridDatabase.cs`

```csharp
public class GridDatabase {
    public List<GridTemplate> Normals { get; private set; } = new();
    public List<GridTemplate> Fun { get; private set; } = new();

    public void LoadFromPersistentPath() {
        var dir = Path.Combine(Application.persistentDataPath, "Grids");
        Normals = LoadFile(Path.Combine(dir, "grid_templates_normal.json"), "normal");
        Fun     = LoadFile(Path.Combine(dir, "grid_templates_fun.json"),    "fun");
        Debug.Log($"GridDatabase: loaded {Normals.Count} normals, {Fun.Count} fun");
    }

    static List<GridTemplate> LoadFile(string path, string kind) {
        if (!File.Exists(path)) {
            Debug.LogWarning($"Missing grid DB: {path}");
            return new List<GridTemplate>();
        }
        var json = File.ReadAllText(path);
        var wrapped = JsonUtility.FromJson<GridDbFile>(json);
        foreach (var t in wrapped.templates) t.databaseKind = kind;
        return wrapped.templates.ToList();
    }

    public int TotalCount => Normals.Count + Fun.Count;
}

[Serializable] class GridDbFile {
    public int version, size, count;
    public string kind;
    public GridTemplate[] templates;
}
```

Note: Unity's `JsonUtility` can't parse `int[][]` directly. **Do not pick a JSON library at this point** — see Section 12 ("Checkpoint: JSON library"). Claude Code should pause here and report findings to the human before writing any deserialisation code.

### 5.c `UsedGridsTracker.cs`

```csharp
public class UsedGridsTracker {
    const string KEY = "CrossWaves_UsedGrids_v1";
    HashSet<string> used = new();
    Queue<string> cooldown = new();   // last N marked, preserved across reset
    public int CooldownSize = 5;

    public void Load() {
        used.Clear();
        var raw = PlayerPrefs.GetString(KEY, "");
        if (!string.IsNullOrEmpty(raw))
            foreach (var k in raw.Split('|')) used.Add(k);
    }

    public void Save() =>
        PlayerPrefs.SetString(KEY, string.Join("|", used));

    public bool IsUsed(string compositeKey) => used.Contains(compositeKey);

    public void Mark(string compositeKey) {
        used.Add(compositeKey);
        cooldown.Enqueue(compositeKey);
        while (cooldown.Count > CooldownSize) cooldown.Dequeue();
    }

    /// Called when both databases are exhausted. Clears `used` but re-adds
    /// the most-recently-used N so they don't immediately repeat.
    public void Reset() {
        used.Clear();
        foreach (var k in cooldown) used.Add(k);
        Save();
    }

    public int UsedCount => used.Count;
}
```

### 5.d `GridSelector.cs`

```csharp
public class GridSelector {
    public GridDatabase db;
    public UsedGridsTracker tracker;
    static System.Random rng = new();

    /// Returns null if every grid in both DBs is either used or transient-excluded.
    public GridTemplate PickRandom(HashSet<string> transientExclude = null) {
        bool funAvail   = HasAvailable(db.Fun,     transientExclude);
        bool normAvail  = HasAvailable(db.Normals, transientExclude);
        if (!funAvail && !normAvail) return null;

        // 50/50 weighting when both available; fall back to the non-empty one
        bool pickFun = funAvail && (!normAvail || rng.NextDouble() < 0.5);
        var pool = pickFun ? db.Fun : db.Normals;
        var candidates = pool
            .Where(g => !tracker.IsUsed(g.CompositeKey)
                     && (transientExclude == null || !transientExclude.Contains(g.CompositeKey)))
            .ToList();
        if (candidates.Count == 0) {
            // The picked DB had only transient-excluded items; switch sides
            pool = pickFun ? db.Normals : db.Fun;
            candidates = pool
                .Where(g => !tracker.IsUsed(g.CompositeKey)
                         && (transientExclude == null || !transientExclude.Contains(g.CompositeKey)))
                .ToList();
            if (candidates.Count == 0) return null;
        }
        return candidates[rng.Next(candidates.Count)];
    }

    bool HasAvailable(List<GridTemplate> pool, HashSet<string> transient) =>
        pool.Any(g => !tracker.IsUsed(g.CompositeKey)
                   && (transient == null || !transient.Contains(g.CompositeKey)));
}
```

### 5.e `WordFitter.cs` — the CSP

Standard backtracking with MRV (minimum remaining values) variable ordering and forward checking. Pseudo:

```
Fit(grid, difficulty, maxBacktracks):
    slots = grid.ExtractSlots(3)
    letters = new 13×13 char array, all '\0' on active cells
    assignment = {}
    counter = { backtracks: 0 }
    if Solve(slots, letters, assignment, difficulty, counter, maxBacktracks):
        return BuildPuzzle(grid, assignment)
    return null

Solve(slots, letters, assignment, difficulty, counter, budget):
    if counter.backtracks > budget: return false
    unassigned = slots.Where(s => !assignment.ContainsKey(s))
    if unassigned.Count == 0: return true

    # MRV: pick slot with the fewest valid candidates
    slot = unassigned.OrderBy(s => CountCandidates(s, letters, difficulty)).First()

    candidates = wordDb.WordsOfLength(slot.length, difficulty)
                       .Where(w => Matches(w, slot, letters))
                       .OrderBy(_ => rng())               # randomise for variety
                       .Take(50)                          # cap exploration breadth

    foreach word in candidates:
        prior = WriteLetters(letters, slot, word)        # remember overwritten ' '/letters
        assignment[slot] = word

        # forward check: every intersecting unassigned slot still has ≥1 candidate?
        if ForwardCheckOK(slot, letters, slots, assignment, difficulty):
            if Solve(...): return true

        # undo
        RestoreLetters(letters, slot, prior)
        assignment.Remove(slot)
        counter.backtracks++

    return false
```

Notes:

- `Matches(word, slot, letters)`: for each i in 0..length-1, either `letters[slot.CellAt(i)]` is `\0` (empty) or it equals `word[i]`.
- `ForwardCheckOK`: for each intersecting slot that has cells now constrained by the assignment, confirm at least one word from the difficulty pool still satisfies the partial pattern.
- `WordsOfLength(len, difficulty)` should already exist (or be trivially added) on `CrosswordDatabase`.
- Cap candidates per slot (`.Take(50)`) to keep search bounded. Random ordering ensures different runs produce different puzzles.

`BuildPuzzle` produces whatever shape `CrosswordGenerator` currently returns — same struct/class the rest of the codebase expects.

### 5.f Changes to `CrosswordDatabase.cs`

Add (if missing):

```csharp
public IReadOnlyList<string> WordsOfLength(int len, Difficulty d);
public int WordCountForDifficulty(Difficulty d);
public int WordCountForLength(int len, Difficulty d);
```

Used by both the fitter and `WordAvailabilityMonitor`. Cache by length on first call.

---

## 6. Modified `CrosswordGenerator.cs`

Replace the seed-and-grow body. Outline:

```csharp
public class CrosswordGenerator : MonoBehaviour {
    public GenerationConfig config;
    public CrosswordDatabase wordDb;

    GridDatabase gridDb;
    UsedGridsTracker tracker;
    GridSelector selector;
    WordFitter fitter;
    WordAvailabilityMonitor monitor;

    void Init() {
        gridDb = new GridDatabase();
        gridDb.LoadFromPersistentPath();
        tracker = new UsedGridsTracker();
        tracker.Load();
        selector = new GridSelector { db = gridDb, tracker = tracker };
        fitter = new WordFitter { wordDb = wordDb };
        monitor = new WordAvailabilityMonitor(wordDb);
    }

    public Puzzle GenerateOne(Difficulty difficulty) {
        if (gridDb == null) Init();

        // Pre-flight check on word availability
        var warning = monitor.CheckForDifficulty(difficulty);
        if (warning == WarningLevel.Critical) {
            ShowBlockingError(monitor.MessageFor(difficulty));
            return null;
        }
        if (warning == WarningLevel.Low) {
            ShowToast(monitor.MessageFor(difficulty));
        }

        var transient = new HashSet<string>();
        for (int attempt = 0; attempt < config.maxGridAttempts; attempt++) {
            var grid = selector.PickRandom(transient);
            if (grid == null) {
                // Both databases exhausted — reset and try once more
                tracker.Reset();
                grid = selector.PickRandom(transient);
                if (grid == null) {
                    ShowError("No grids available — grid databases may be empty.");
                    return null;
                }
            }

            if (grid.stats.h_words < config.minH || grid.stats.v_words < config.minV) {
                transient.Add(grid.CompositeKey);
                continue;
            }

            var puzzle = fitter.Fit(grid, difficulty, config.maxBacktracks);
            if (puzzle == null) {
                transient.Add(grid.CompositeKey);
                continue;
            }

            tracker.Mark(grid.CompositeKey);
            tracker.Save();
            return puzzle;
        }

        ShowError($"Couldn't generate a {difficulty} crossword after {config.maxGridAttempts} grid attempts.");
        return null;
    }

    public List<Puzzle> GenerateBatch(Difficulty difficulty, int count) {
        var puzzles = new List<Puzzle>();
        for (int i = 0; i < count; i++) {
            var p = GenerateOne(difficulty);
            if (p != null) puzzles.Add(p);
        }
        return puzzles;
    }
}

[Serializable]
public class GenerationConfig {
    public int minH = 12;
    public int minV = 12;
    public int maxGridAttempts = 20;
    public int maxBacktracks = 10000;
    public int cooldownSize = 5;
}
```

**Delete / archive** the old seed-and-grow code: the seed-cell picker, the grow loop, the no-butt-adjacency / no-parallel-neighbour / require-intersection guards. (Keep the file in version control so it's recoverable, but the new flow replaces it entirely.)

---

## 7. Warnings system (`WordAvailabilityMonitor.cs`)

Two distinct kinds of warning:

### Grid availability
Surfaced from `CrosswordGenerator.GenerateOne` when `tracker.Reset()` fires. Non-blocking toast:

> *"Grid variety reset — you've seen every layout once. Cycling fresh."*

### Word availability (per difficulty)

```csharp
public enum WarningLevel { None, Low, Critical }

public class WordAvailabilityMonitor {
    readonly CrosswordDatabase db;
    public WordAvailabilityMonitor(CrosswordDatabase db) { this.db = db; }

    // Rough word budget per puzzle ~= 25 words averaging length 5 = 125 letters of vocab.
    // We need many multiples of that to avoid the same words appearing every puzzle.
    public WarningLevel CheckForDifficulty(Difficulty d) {
        int total = db.WordCountForDifficulty(d);
        if (total < 200)  return WarningLevel.Critical;
        if (total < 500)  return WarningLevel.Low;

        // Also check distribution: do we have enough words at every length 3..13?
        for (int len = 3; len <= 13; len++) {
            int n = db.WordCountForLength(len, d);
            if (len <= 7 && n < 20) return WarningLevel.Low;
            if (len >  7 && n < 5)  return WarningLevel.Low;
        }
        return WarningLevel.None;
    }

    public string MessageFor(Difficulty d) {
        int total = db.WordCountForDifficulty(d);
        return $"The {d} word library has {total} entries. Crosswords may " +
               $"repeat words or fail to generate. Consider adding more.";
    }
}
```

Thresholds are starting guesses — tune after running batches and seeing where failures cluster.

---

## 8. Display considerations (`CrosswordManager.cs`)

The new grids look different from the old greedy output: normals are dense rectangles with internal black squares, fun grids are sparse silhouettes. Two things to verify in the manager:

1. **Inactive cells.** Confirm how the manager renders cells where the puzzle JSON marks the position inactive. The reference style (puzzles 140 and 143 you showed) shows **no cell at all** outside the silhouette — the grey background shows through. If the manager currently renders a black square in those positions, change it to either hide the cell GameObject or use a transparent material.

2. **Cell numbering.** Standard crossword convention: scan top-left to bottom-right; number any active cell that starts an across or down word (cell has either no active cell to its left OR no active cell above, and the resulting word is length ≥ 3). The existing numbering should already do this, but verify it works on sparse grids — old greedy puzzles had cells in a connected blob, new puzzles can have detached-looking pockets within the silhouette.

3. **Selection highlight.** No change expected — green active-word / cursor highlight should work fine.

**Likely no logic changes needed** if the manager already reads cell active/inactive state from the puzzle JSON. If yes, just confirm visually. If the manager assumes a rectangular puzzle area (every cell present), it needs a minor pass.

---

## 9. Failure handling matrix

| Scenario | Handling |
|---|---|
| One JSON missing | Log warning, generate only from the other |
| Both JSONs missing/empty | Show blocking error: *"No grid databases found. Place grid_templates_*.json in Application.persistentDataPath/Grids/."* |
| Grid has fewer than `minH` or `minV` slots | Skip pre-CSP, mark transient, try another |
| CSP fails (budget exhausted) | Skip, mark transient, try another |
| All grids exhausted (used + transient) | `tracker.Reset()` + one retry; if still fails, show error |
| Word DB has < 200 words for difficulty | Block generation with helpful message |
| Word DB has 200–499 | Generate but show non-blocking toast warning |

---

## 10. Tests to write

Use the project's existing test framework (NUnit + Unity Test Runner).

### Unit
- `GridTemplate_ExtractSlots_FindsAllRunsOver3`
- `GridTemplate_ExtractSlots_SkipsLength2`  *(should never appear in valid grids, but the extractor must be defensive)*
- `UsedGridsTracker_MarkAndIsUsed`
- `UsedGridsTracker_ResetPreservesCooldown`
- `GridSelector_PreferentialFallback_WhenOneDbExhausted`
- `WordFitter_Fits_TrivialGrid`  *(3×3 with a small word list)*
- `WordFitter_Returns_Null_OnImpossible`

### Integration
- Generate 100 consecutive puzzles, assert no grid name repeats among the first `totalGrids - cooldownSize`.
- Generate 30 puzzles across three difficulties; assert the global used set is never violated until both DBs are exhausted.
- Force a tiny word DB (10 words) and confirm the generator returns null gracefully instead of looping forever.

### Manual smoke
1. Wipe `Application.persistentDataPath/CrossWaves/`.
2. Drop both grid JSONs into `<persistentDataPath>/Grids/`.
3. Trigger Generate from the Editor menu — verify rendered cells form the expected silhouette/pattern.
4. Generate ~30 puzzles in a row; flick through them in-app and check no two look the same.
5. Generate at Easy, Medium, Hard in sequence; verify no grid repeats across difficulties.

---

## 11. Implementation order

Do these in order; each step compiles and is independently testable.

1. **JSON library checkpoint (Section 12)** — *stop here, report to human, wait for the green light.* Don't write any deserialisation code yet.
2. **`GridTemplate` + `GridDatabase`** — load the two JSONs, dump counts to log. Verify by inspecting `Debug.Log` after a play.
3. **`UsedGridsTracker`** — exercise via a debug button: Mark a few, Save, Restart, Load, confirm persistence.
4. **`GridSelector`** — debug button that picks 50 random grids and prints names. Confirm no repeats until cap reached.
5. **`WordFitter`** — start in isolation. Feed it a known grid and watch the CSP run. Log backtrack count. Try a few different grids to confirm it's not too slow.
6. **Wire `CrosswordGenerator`** — replace the old loop. Run from Editor menu. Generate 1 puzzle. Inspect the JSON.
7. **`CrosswordManager` rendering check** — load a generated puzzle in-app, verify visuals. Adjust inactive-cell rendering if needed.
8. **`WordAvailabilityMonitor`** — add and wire warnings. Test by temporarily restricting the word DB.
9. **Tests** as listed in Section 10.

Estimated effort: 2–3 focused days. The fitter is the trickiest piece — start there if you want to de-risk early. Everything else is plumbing.

---

## 12. Checkpoint: JSON library

**STOP before writing any deserialisation code.**

Unity's built-in `JsonUtility` cannot parse `int[][]` (jagged arrays) — it only handles flat fields and `[Serializable]` classes with non-nested arrays. The grid JSON schema uses `cells: int[][]`, so we have two options:

- **Option A (recommended): use `Newtonsoft.Json`** — handles jagged arrays natively. Unity ships it as a package (`com.unity.nuget.newtonsoft-json`) and most production projects already pull it in.
- **Option B: flatten** — change the JSON to store `cells` as `int[169]` row-major and reshape on load. Avoids a dependency but means the Python grid generator needs a one-line schema change, and the JSON is less human-readable.

**Claude Code task at this checkpoint:**
1. Search the project for existing Newtonsoft usage:
   - Look in `Packages/manifest.json` for `com.unity.nuget.newtonsoft-json`.
   - Look for `using Newtonsoft.Json;` in any `.cs` file.
   - Look for `Newtonsoft.Json.dll` in `Assets/Plugins/`.
2. Report findings to the human in this format:
   - "Newtonsoft is already in the project at `<location>`, recommend Option A."
   - **or** "Newtonsoft is not in the project. Recommend adding it via Package Manager (Option A) because it's a standard Unity dependency, low-risk, and keeps the JSON files human-readable. Alternative is Option B (flatten cells)."
3. **Wait for the human to confirm the choice before continuing.** The human will return to Claude Desktop after this to update the implementation plan with the chosen path, then resume here.

---

## 13. Open decisions for the human

These choices weren't fully specified; flag them in the PR description and pick a default:

- **DB pick weighting**: currently 50/50 between fun and normal. Could be biased by user preference (e.g. "I want more fun puzzles") via a settings toggle.
- **`MIN_H` / `MIN_V`**: set to 12 each based on the conversation. Some fun shapes (vertical objects like *candle* with only 3 V words) will be pre-filtered out. If we want them usable, lower `MIN_V`. Alternative: a single combined `MIN_TOTAL = 24`.

**Difficulty is intentionally NOT a factor in grid selection.** Grid choice is fully random across the union of both databases — difficulty only affects which words the fitter draws from when filling the chosen grid. Do not add any difficulty→grid coupling.

---

## 14. Unity setup steps (after code is done)

Once all the C# scripts are written, compiled, and the JSON library question is resolved, perform these steps in this exact order. Each instruction is for the human operator working in the Unity Editor, not for Claude Code.

### Step 14.1 — Open the project and confirm clean compile

1. Open the CrossWaves project in Unity.
2. Wait for compilation. Open **Window → General → Console** and clear it.
3. There should be **zero red errors**. Yellow warnings are fine. If any red errors appear, return to Claude Code with the exact error text before continuing.

### Step 14.2 — Install Newtonsoft (only if Option A was chosen)

If the checkpoint in Section 12 decided to use Newtonsoft:

1. Open **Window → Package Manager**.
2. Top-left dropdown → **Unity Registry**.
3. Search for `Newtonsoft Json` (the package id is `com.unity.nuget.newtonsoft-json`).
4. Click **Install**. Wait for Unity to reimport.
5. Verify by re-checking the Console — still zero errors.

Skip this step entirely if Option B (flatten to `int[169]`) was chosen — no package install needed.

### Step 14.3 — Locate the persistent data path

The persistentDataPath differs by OS. To find it on this specific machine:

1. In the project, temporarily add a debug `Debug.Log(Application.persistentDataPath);` call (or use the Editor menu **Edit → Preferences → Diagnostics** if exposed) and run once to print it.
2. Note the printed path. On typical setups:
   - **Windows**: `C:\Users\<you>\AppData\LocalLow\<CompanyName>\CrossWaves\`
   - **macOS**: `~/Library/Application Support/<CompanyName>/CrossWaves/`
   - **Linux**: `~/.config/unity3d/<CompanyName>/CrossWaves/`
3. Open that folder in your OS file explorer (create it if it doesn't exist — running the Editor once should create it).

### Step 14.4 — Place the grid JSONs

1. Inside the persistentDataPath folder, create a subfolder named exactly `Grids` (capital G).
2. Copy these two files into it:
   - `grid_templates_normal.json` (the 200-grid normals database)
   - `grid_templates_fun.json` (the 50-grid fun shapes database) — *if your file is named differently, e.g. `batchA.json`, rename it now to `grid_templates_fun.json` so the loader picks it up*.
3. Confirm both files are readable plain JSON. Open one in a text editor and check it starts with `{` and has a `"templates":` array.

Final layout:

```
<persistentDataPath>/
  Grids/
    grid_templates_normal.json
    grid_templates_fun.json
```

### Step 14.5 — Configure the GenerationConfig asset

If `GenerationConfig` was implemented as a `ScriptableObject` (recommended):

1. In the Project window, right-click → **Create → CrossWaves → Generation Config**.
2. Save it as `Assets/Settings/GenerationConfig.asset`.
3. Select it in the Project window and confirm the Inspector shows:
   - Min H: `12`
   - Min V: `12`
   - Max Grid Attempts: `20`
   - Max Backtracks: `10000`
   - Cooldown Size: `5`
4. Drag this asset into the `CrosswordGenerator` component's `Config` field (in whichever scene/prefab `CrosswordGenerator` lives).

If `GenerationConfig` was implemented as a plain `[Serializable]` struct/class embedded on `CrosswordGenerator`, skip this step — the values are edited directly on the component in the Inspector. Set the same defaults there.

### Step 14.6 — Smoke test: generate one puzzle

1. Open the scene containing `CrosswordGenerator` (per the architecture this lives in `Assets/Scenes/`).
2. In the Editor menu, find the **CrossWaves → Generate Puzzle** menu item (or whatever the existing menu item is called — likely defined inside `CrosswordGenerator.cs` with `[MenuItem(...)]`). If it doesn't exist yet, Claude Code needs to expose one.
3. Click it. Watch the Console.
4. **Expected log output (roughly):**
   ```
   GridDatabase: loaded 200 normals, 50 fun
   [CrosswordGenerator] Picked grid 'normal:normal_137' (attempt 1)
   [WordFitter] Solved in 247 backtracks
   [CrosswordGenerator] Marked normal:normal_137 as used (1/250)
   Puzzle saved to <persistentDataPath>/Crosswords/crossword_<N>.json
   ```
5. If you see **errors**, copy the full Console output and return to Claude Code. Likely culprits: JSON parse failure (Section 12 choice not consistent with the code), grid file missing, word DB too small.

### Step 14.7 — Smoke test: render the puzzle

1. Play the scene that uses `CrosswordManager` (the runtime crossword display).
2. Confirm the newly generated puzzle loads and displays as a sparse grid — no black squares where cells should be inactive, just the silhouette / rectangular pattern visible against the grey background.
3. Verify:
   - Cells form a recognisable shape (heart, tree, etc. for fun grids; rectangular block-pattern for normals).
   - Cell numbering appears at word starts.
   - Tapping a cell highlights the active word.
   - Clue text appears at the bottom.
4. If inactive cells render as black squares instead of background-coloured, this is the rendering tweak called out in Section 8 — go fix the `CrosswordCell` prefab or the manager's render code.

### Step 14.8 — Batch test: generate 30 puzzles

1. From the Editor menu, run **CrossWaves → Generate Batch (30)** (Claude Code should expose this; it calls `GenerateBatch(difficulty, 30)`).
2. Wait — this should take under a minute. Watch the Console for repeated grid names (there should be none).
3. After completion:
   - Console should show 30 success lines, no errors.
   - `<persistentDataPath>/Crosswords/` should contain 30 new JSON files.
   - Open 5–6 of them in a text editor and confirm visually distinct grid layouts (eyeball the `cells` arrays).
4. In-app, flick through the generated puzzles and confirm none look identical.

### Step 14.9 — Cross-difficulty test

1. Generate 5 Easy, 5 Medium, 5 Hard puzzles back to back via the Editor menu.
2. Console should never repeat a grid name across the 15 puzzles.
3. After the 15th, if your total grid count is 250, the used count should be 15/250 — well below exhaustion.

### Step 14.10 — Exhaustion test (optional but recommended)

To validate the reset behaviour:

1. With ~250 grids total, generate 250 puzzles in a row (use a batch call).
2. On puzzle ~250 the Console should show:
   ```
   [GridSelector] No grids available — resetting tracker
   [UsedGridsTracker] Reset complete (cooldown preserved: 5 grids)
   ```
3. Puzzle 251 should succeed with a freshly-available grid. None of the most recent 5 grid names should appear in puzzles 251–255.

### Step 14.11 — Word availability warning test

1. Temporarily edit `CrosswordDatabase` to limit returned words to, say, 100 entries.
2. Trigger Generate. The Editor should surface a warning toast / modal:
   > *"The Medium word library has 100 entries. Crosswords may repeat words or fail to generate. Consider adding more."*
3. Revert the change. Warning should disappear on next generate.

### Step 14.12 — Commit and document

1. Commit all new and modified scripts to version control.
2. In your commit message or README, note:
   - The grid JSONs live in persistentDataPath, **not** in `Assets/` — they are deployment artifacts, not source.
   - To regenerate grid JSONs, use the Python pipeline (separate repo / scripts).
   - To add more fun grids: drop new entries into `grid_templates_fun.json` and bump the `count` field — no code change needed.

---

End of plan.
