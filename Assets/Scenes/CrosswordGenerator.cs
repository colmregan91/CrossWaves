using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CrosswordGenerator : MonoBehaviour
{
    public GeneratedEntry[,] grid;

    private int MaxGridX = 13;
    private int MaxGridY = 13;

    public List<CrosswordEntryPositional> UsedHorizontalEntries = new List<CrosswordEntryPositional>();
    public List<CrosswordEntryPositional> UsedVerticalEntries = new List<CrosswordEntryPositional>();

    public string SelectedDifficulty = "easy";
    public int CrosswordsToGenerate = 90;

    // Abandon a single template attempt after this many failed candidate placements.
    // Triggers a reshuffle-and-retry of the same template.
    private const int BacktrackLimit = 50_000;

    // How many reshuffle restarts to attempt per template before moving on.
    private const int RestartsPerTemplate = 5;

    // ─── Public API (signatures unchanged) ───────────────────────────────────

    public void GenerateNewCrossword() => StartCoroutine(GenerateBatch(CrosswordsToGenerate));

    public void SaveNewCrossword() =>
        CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);

    // Entry point for the benchmark: fills 50 grids and logs timing + backtrack stats.
    public void RunBenchmark() => StartCoroutine(BenchmarkFill(50));

    // ─── Batch generation ────────────────────────────────────────────────────

    private IEnumerator GenerateBatch(int count)
    {
        string path = Path.Combine(Application.persistentDataPath, "Grids", "GridTemplateNormals.json");
        var templateFile = JsonUtility.FromJson<GridTemplateFile>(File.ReadAllText(path));

        var database = CrosswordUtils.ReadDatabaseFromFile();
        var usedTemplateIds = LoadUsedTemplateIds(SelectedDifficulty);

        for (int i = 0; i < count; i++)
        {
            var available = templateFile.grids
                .Where(t => !usedTemplateIds.Contains(t.id))
                .ToArray();

            if (available.Length == 0)
            {
                usedTemplateIds.Clear();
                available = templateFile.grids;
                UnityEngine.Debug.Log("All templates used — resetting pool.");
            }

            var recentAnswers = CrosswordUtils.GetSavedAnswers(SelectedDifficulty, maxRecent: 30);
            var poolsByLength = database.Entries
                .Where(e => e.difficulty == SelectedDifficulty
                         && !e.isUsed
                         && !recentAnswers.Contains(e.answer)
                         && e.answer.Length >= 3
                         && e.answer.Length <= MaxGridX)
                .GroupBy(e => e.answer.Length)
                .ToDictionary(g => g.Key, g => g.ToList());

            // If the pool is too thin to fill any template, reset isUsed in memory.
            // recentAnswers still prevents repeats in nearby crosswords.
            if (!available.Any(t => CanPotentiallyFill(t, poolsByLength)))
            {
                foreach (var e in database.Entries) e.isUsed = false;
                poolsByLength = database.Entries
                    .Where(e => e.difficulty == SelectedDifficulty
                             && !recentAnswers.Contains(e.answer)
                             && e.answer.Length >= 3
                             && e.answer.Length <= MaxGridX)
                    .GroupBy(e => e.answer.Length)
                    .ToDictionary(g => g.Key, g => g.ToList());
                UnityEngine.Debug.Log("Word pool exhausted — reset isUsed in memory for remainder of batch.");
            }

            var screened = available.Where(t => CanPotentiallyFill(t, poolsByLength)).ToArray();
            var toTry = screened.Length > 0 ? screened.ToList() : available.ToList();
            Shuffle(toTry);

            InitializeGrid();

            bool filled = false;
            GridTemplateDef usedTemplate = null;
            int totalBt = 0;

            foreach (var template in toTry)
            {
                for (int restart = 0; restart < RestartsPerTemplate; restart++)
                {
                    foreach (var pool in poolsByLength.Values) Shuffle(pool);

                    ClearGrid();
                    ApplyTemplate(template);
                    UsedHorizontalEntries.Clear();
                    UsedVerticalEntries.Clear();

                    int bt = 0;
                    filled = TryFillTemplate(template, poolsByLength, ref bt);
                    totalBt += bt;

                    if (filled) { usedTemplate = template; break; }
                    yield return null;
                }
                if (filled) break;
            }

            if (!filled)
            {
                // Max slots of each length required by any single template
                var maxNeeded = toTry
                    .SelectMany(t => t.slots.GroupBy(s => s.length).Select(g => new { len = g.Key, count = g.Count() }))
                    .GroupBy(x => x.len)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        int required = g.Max(x => x.count);
                        int avail = poolsByLength.TryGetValue(g.Key, out var p) ? p.Count : 0;
                        string flag = avail < required ? "!" : "";
                        return $"{flag}len{g.Key}: need {required}, have {avail}";
                    });
                UnityEngine.Debug.LogWarning($"Crossword {i + 1}: could not fill — {string.Join(" | ", maxNeeded)}");
                yield return null;
                continue;
            }

            usedTemplateIds.Add(usedTemplate.id);
            SaveUsedTemplateIds(SelectedDifficulty, usedTemplateIds);

            foreach (var entry in UsedHorizontalEntries.Concat(UsedVerticalEntries))
                PlaceWord(entry);

            CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
            database = CrosswordUtils.ReadDatabaseFromFile();

            UnityEngine.Debug.Log($"Crossword {i + 1}/{count} saved (template {usedTemplate.id}, backtracks: {totalBt}).");
            yield return null;
        }

        UnityEngine.Debug.Log($"Batch complete: {count} crosswords saved.");
    }

    // ─── Benchmark (fill-only, no file saves) ────────────────────────────────

    private IEnumerator BenchmarkFill(int count)
    {
        string path = Path.Combine(Application.persistentDataPath, "Grids", "GridTemplateNormals.json");
        var templateFile = JsonUtility.FromJson<GridTemplateFile>(File.ReadAllText(path));
        var database = CrosswordUtils.ReadDatabaseFromFile();

        var poolsByLength = database.Entries
            .Where(e => e.difficulty == SelectedDifficulty
                     && e.answer.Length >= 3
                     && e.answer.Length <= MaxGridX)
            .GroupBy(e => e.answer.Length)
            .ToDictionary(g => g.Key, g => g.ToList());

        var templates = templateFile.grids;
        long totalMs = 0;
        long totalBt = 0;
        int successes = 0;

        for (int i = 0; i < count; i++)
        {
            foreach (var pool in poolsByLength.Values) Shuffle(pool);
            var template = templates[i % templates.Length];

            int bt = 0;
            long t0 = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool ok = TryFillTemplate(template, poolsByLength, ref bt);
            long elapsed = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t0;

            if (ok) { successes++; totalMs += elapsed; totalBt += bt; }
            yield return null;
        }

        UnityEngine.Debug.Log(
            $"Benchmark: {successes}/{count} filled | " +
            $"total {totalMs}ms | " +
            $"avg {(successes > 0 ? totalMs / successes : 0)}ms/fill | " +
            $"avg {(successes > 0 ? totalBt / successes : 0)} backtracks/fill");
    }

    // ─── Bitset word index ────────────────────────────────────────────────────
    //
    // Precomputed once per word-list. posIndex[pos][charIdx*blockCount + block]
    // is a ulong bitmask of which word indices have `charIdx+'A'` at position pos.
    // AND-ing constraints across positions narrows the candidate set in O(L*blockCount).

    private sealed class WordIndex
    {
        public readonly string[] Words;
        private readonly int blockCount;
        private readonly ulong[][] posIndex; // [position][char*blockCount + block]

        public WordIndex(List<CrosswordEntry> entries)
        {
            Words = entries.Select(e => e.answer).ToArray();
            int n = Words.Length;
            blockCount = (n + 63) / 64;
            int len = Words[0].Length;

            posIndex = new ulong[len][];
            for (int p = 0; p < len; p++)
                posIndex[p] = new ulong[26 * blockCount];

            for (int wi = 0; wi < n; wi++)
            {
                int blk = wi / 64, bit = wi % 64;
                for (int p = 0; p < len; p++)
                {
                    int ci = Words[wi][p] - 'A';
                    posIndex[p][ci * blockCount + blk] |= 1UL << bit;
                }
            }
        }

        // Bitset with all word indices set.
        public ulong[] AllBits()
        {
            int n = Words.Length;
            var bits = new ulong[blockCount];
            int rem = n % 64;
            for (int b = 0; b < blockCount - 1; b++) bits[b] = ulong.MaxValue;
            bits[blockCount - 1] = rem == 0 ? ulong.MaxValue : (1UL << rem) - 1;
            return bits;
        }

        // Narrow candidates: position pos must equal char c.
        public void AndConstraint(ulong[] bits, int pos, char c)
        {
            int ci = c - 'A';
            ulong[] row = posIndex[pos];
            for (int b = 0; b < blockCount; b++)
                bits[b] &= row[ci * blockCount + b];
        }

        public int Count(ulong[] bits)
        {
            int n = 0;
            foreach (ulong v in bits) n += PopCount(v);
            return n;
        }

        // Enumerate set bit indices (word indices) in ascending order.
        public IEnumerable<int> Enumerate(ulong[] bits)
        {
            for (int b = 0; b < blockCount; b++)
            {
                ulong v = bits[b];
                while (v != 0)
                {
                    int tz = TrailingZeros(v);
                    yield return b * 64 + tz;
                    v &= v - 1;
                }
            }
        }

        private static int PopCount(ulong v)
        {
            v -= (v >> 1) & 0x5555555555555555UL;
            v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
            v = (v + (v >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
            return (int)((v * 0x0101010101010101UL) >> 56);
        }

        private static int TrailingZeros(ulong v)
        {
            int n = 0;
            if ((v & 0xFFFFFFFFUL) == 0) { n += 32; v >>= 32; }
            if ((v & 0xFFFFUL) == 0) { n += 16; v >>= 16; }
            if ((v & 0xFFUL) == 0) { n += 8; v >>= 8; }
            if ((v & 0xFUL) == 0) { n += 4; v >>= 4; }
            if ((v & 0x3UL) == 0) { n += 2; v >>= 2; }
            if ((v & 0x1UL) == 0) n++;
            return n;
        }
    }

    // ─── Intersection descriptor ─────────────────────────────────────────────

    private struct Intersection
    {
        public int OtherSlot;
        public int MyPos;    // character position within this slot
        public int OtherPos; // character position within the other slot
    }

    // ─── CSP fill entry point ─────────────────────────────────────────────────

    private bool TryFillTemplate(GridTemplateDef template, Dictionary<int, List<CrosswordEntry>> poolsByLength, ref int backtracks)
    {
        var slots = template.slots;
        int n = slots.Length;

        // Build one WordIndex per required word length.
        var wordIndices = new Dictionary<int, WordIndex>();
        foreach (var s in slots)
        {
            if (wordIndices.ContainsKey(s.length)) continue;
            if (!poolsByLength.TryGetValue(s.length, out var pool)) return false;
            wordIndices[s.length] = new WordIndex(pool);
        }

        // Precompute which slot pairs share a grid cell (across+down only).
        var intersections = new Intersection[n][];
        for (int i = 0; i < n; i++)
        {
            var list = new List<Intersection>();
            for (int j = 0; j < n; j++)
            {
                if (i == j || slots[i].dir == slots[j].dir) continue;
                if (TryGetIntersection(slots[i], slots[j], out int myPos, out int otherPos))
                    list.Add(new Intersection { OtherSlot = j, MyPos = myPos, OtherPos = otherPos });
            }
            intersections[i] = list.ToArray();
        }

        // Initial candidate sets — all words valid for each slot's length.
        var candidates = new ulong[n][];
        for (int i = 0; i < n; i++)
            candidates[i] = wordIndices[slots[i].length].AllBits();

        var assigned = new int[n];
        for (int i = 0; i < n; i++) assigned[i] = -1;

        var letterGrid = new char[MaxGridX, MaxGridY];

        UsedHorizontalEntries.Clear();
        UsedVerticalEntries.Clear();

        if (!Solve(slots, assigned, 0, letterGrid, candidates, intersections, wordIndices, ref backtracks))
            return false;

        // Translate word-index assignments back to CrosswordEntryPositional records.
        for (int i = 0; i < n; i++)
        {
            var slot = slots[i];
            string word = wordIndices[slot.length].Words[assigned[i]];
            var entry = poolsByLength[slot.length].First(e => e.answer == word);
            var pos = new CrosswordEntryPositional
            {
                StartX = slot.col,
                StartY = slot.row,
                isHorizontal = slot.dir == "across",
                entry = entry
            };
            if (slot.dir == "across") UsedHorizontalEntries.Add(pos);
            else UsedVerticalEntries.Add(pos);
        }
        return true;
    }

    // ─── Recursive CSP solver ─────────────────────────────────────────────────

    private bool Solve(
        GridSlotDef[] slots,
        int[] assigned,
        int assignedCount,
        char[,] letterGrid,
        ulong[][] candidates,
        Intersection[][] intersections,
        Dictionary<int, WordIndex> wordIndices,
        ref int backtracks)
    {
        if (assignedCount == slots.Length) return true;

        // MRV: choose the unassigned slot with the fewest remaining valid candidates.
        // Slots with fewer options are more likely to cause failure early if wrong.
        int chosen = -1, minCount = int.MaxValue;
        for (int i = 0; i < slots.Length; i++)
        {
            if (assigned[i] != -1) continue;
            int c = wordIndices[slots[i].length].Count(candidates[i]);
            if (c < minCount) { minCount = c; chosen = i; }
            if (minCount == 0) break;
        }

        if (chosen == -1 || minCount == 0) return false;

        var slot = slots[chosen];
        var idx = wordIndices[slot.length];

        // Words already placed in other same-length slots (no duplicate answers per puzzle).
        var usedInLength = new HashSet<int>();
        for (int i = 0; i < slots.Length; i++)
            if (i != chosen && assigned[i] != -1 && slots[i].length == slot.length)
                usedInLength.Add(assigned[i]);

        // Iterate candidates in pool order (pre-shuffled each attempt for variety).
        foreach (int wi in idx.Enumerate(candidates[chosen]))
        {
            if (usedInLength.Contains(wi)) continue;

            string word = idx.Words[wi];

            // Write letters into the shared letter grid.
            // Track cells we wrote to so we can undo exactly our contribution.
            var freshCells = PlaceWordLetters(word, slot, letterGrid);

            // Forward checking: after placing this word, narrow candidates of every
            // intersecting unassigned slot. If any slot reaches zero candidates, we
            // have detected a dead end one level ahead — backtrack immediately.
            var inter = intersections[chosen];
            var savedSlots = new int[inter.Length];
            var savedBits  = new ulong[inter.Length][];
            int savedCount = 0;
            bool viable = true;

            for (int k = 0; k < inter.Length && viable; k++)
            {
                int other = inter[k].OtherSlot;
                if (assigned[other] != -1) continue;

                var otherIdx = wordIndices[slots[other].length];
                savedSlots[savedCount] = other;
                savedBits[savedCount]  = (ulong[])candidates[other].Clone();
                savedCount++;

                // Constrain: the other slot must have this char at their shared cell.
                otherIdx.AndConstraint(candidates[other], inter[k].OtherPos, word[inter[k].MyPos]);

                // Forward check failure — no valid words remain for this slot.
                if (otherIdx.Count(candidates[other]) == 0) viable = false;
            }

            if (viable)
            {
                assigned[chosen] = wi;
                if (Solve(slots, assigned, assignedCount + 1, letterGrid, candidates, intersections, wordIndices, ref backtracks))
                    return true;
                assigned[chosen] = -1;
            }

            backtracks++;
            if (backtracks >= BacktrackLimit) return false;

            // Undo: remove only the letters we were first to write, then restore
            // the candidate bitsets we narrowed during forward checking.
            UndoLetters(freshCells, letterGrid);
            for (int s = 0; s < savedCount; s++)
                candidates[savedSlots[s]] = savedBits[s];
        }

        return false;
    }

    // ─── Letter-grid helpers ──────────────────────────────────────────────────

    // Returns cells that were empty before this placement (needed for clean undo).
    // Intersection cells shared with already-placed words are written but not tracked,
    // so backtracking never erases letters owned by another slot.
    private List<(int x, int y)> PlaceWordLetters(string word, GridSlotDef slot, char[,] g)
    {
        bool isH = slot.dir == "across";
        var fresh = new List<(int, int)>(word.Length);
        for (int i = 0; i < word.Length; i++)
        {
            int x = isH ? slot.col + i : slot.col;
            int y = isH ? slot.row : slot.row + i;
            if (g[x, y] == '\0') fresh.Add((x, y));
            g[x, y] = word[i];
        }
        return fresh;
    }

    private void UndoLetters(List<(int x, int y)> fresh, char[,] g)
    {
        foreach (var (x, y) in fresh) g[x, y] = '\0';
    }

    // ─── Geometric intersection ───────────────────────────────────────────────

    private bool TryGetIntersection(GridSlotDef a, GridSlotDef b, out int myPos, out int otherPos)
    {
        myPos = otherPos = 0;
        var h = a.dir == "across" ? a : b;
        var v = a.dir == "across" ? b : a;

        // Shared cell is at (row=h.row, col=v.col).
        int hPos = v.col - h.col; // index into horizontal word
        int vPos = h.row - v.row; // index into vertical word

        if (hPos < 0 || hPos >= h.length || vPos < 0 || vPos >= v.length)
            return false;

        if (a.dir == "across") { myPos = hPos; otherPos = vPos; }
        else                   { myPos = vPos; otherPos = hPos; }
        return true;
    }

    // ─── Pre-screen (cheap, ignores intersections — unchanged) ───────────────

    private bool CanPotentiallyFill(GridTemplateDef template, Dictionary<int, List<CrosswordEntry>> pool)
    {
        var needed = template.slots.GroupBy(s => s.length).ToDictionary(g => g.Key, g => g.Count());
        foreach (var kvp in needed)
            if (!pool.TryGetValue(kvp.Key, out var words) || words.Count < kvp.Value)
                return false;
        return true;
    }

    // ─── Visual grid (unchanged) ──────────────────────────────────────────────

    private void ApplyTemplate(GridTemplateDef template)
    {
        for (int y = 0; y < template.rows.Length; y++)
            for (int x = 0; x < template.rows[y].Length; x++)
                if (template.rows[y][x] == '#')
                    grid[x, y].TurnOffGridElement();
    }

    private void PlaceWord(CrosswordEntryPositional entryInfo)
    {
        for (int i = 0; i < entryInfo.entry.answer.Length; i++)
        {
            int x = entryInfo.isHorizontal ? entryInfo.StartX + i : entryInfo.StartX;
            int y = entryInfo.isHorizontal ? entryInfo.StartY : entryInfo.StartY + i;
            if (grid[x, y].HasLetter) continue;
            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo);
        }
    }

    private void InitializeGrid()
    {
        grid = new GeneratedEntry[MaxGridX, MaxGridY];
        int childIndex = 0;
        for (int y = 0; y < MaxGridY; y++)
            for (int x = 0; x < MaxGridX; x++)
            {
                Transform child = transform.GetChild(childIndex);
                grid[x, y] = child.GetComponent<GeneratedEntry>();
                grid[x, y].Reset();
                childIndex++;
            }
    }

    private void ClearGrid()
    {
        foreach (var cell in grid) cell.Reset();
    }

    // ─── Template-ID persistence (unchanged) ─────────────────────────────────

    private HashSet<int> LoadUsedTemplateIds(string difficulty, string tag = "")
    {
        string fp = Path.Combine(Application.persistentDataPath, "Grids", $"usedTemplates{tag}_{difficulty}.json");
        if (!File.Exists(fp)) return new HashSet<int>();
        return new HashSet<int>(JsonUtility.FromJson<UsedTemplateIdList>(File.ReadAllText(fp)).ids);
    }

    private void SaveUsedTemplateIds(string difficulty, HashSet<int> ids, string tag = "")
    {
        string fp = Path.Combine(Application.persistentDataPath, "Grids", $"usedTemplates{tag}_{difficulty}.json");
        File.WriteAllText(fp, JsonUtility.ToJson(new UsedTemplateIdList { ids = ids.ToList() }));
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

[System.Serializable]
class UsedTemplateIdList { public List<int> ids = new List<int>(); }

[System.Serializable]
class GridSlotDef { public int number; public string dir; public int row, col, length; }

[System.Serializable]
class GridTemplateDef { public int id; public string[] rows; public GridSlotDef[] slots; }

[System.Serializable]
class GridTemplateFile { public GridTemplateDef[] grids; }
