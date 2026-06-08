# Plan: Azure Blob Storage Cloud Sync

## Context

The game's persistent data (crosswords, database, grid templates) currently lives only on whichever machine last ran it. The user works on two laptops and wants the data kept in sync automatically — no manual copying. On every boot the app should pull the latest files from the cloud (overwriting local). The crossword generator should push files up as soon as it saves them. Azure Blob Storage is the right call: no SDK needed, tiny JSON files cost effectively $0/month, and the REST API works directly with `UnityWebRequest`.

---

## Cost / Azure answer

Yes, essentially free. At this scale (~200KB total data, <20 reads/writes per day):
- Storage: ~$0.00002/month
- Operations: ~$0.001/month
- Azure gives 12 months free for new accounts; after that it's fractions of a cent

---

## Files to create / modify

| File | Action |
|---|---|
| `Assets/Scripts/CloudSyncService.cs` | **Create** — singleton, all Azure REST calls |
| `Assets/Scripts/CloudConfig.cs` | **Create** — stores account URL + SAS token (gitignore this) |
| `Assets/Scripts/RunTimeStartUp.cs` | **Modify** — call download on start |
| `Assets/Scenes/CrosswordGenerator.cs` | **Modify** — call upload after each save |

---

## Azure setup (one-time)

1. Create Azure Storage Account (Standard, LRS — cheapest)
2. Create a Blob container named `crosswaves` (private access)
3. Generate a SAS token on the container with permissions: **Read, Write, List, Delete** — set expiry to a few years
4. Manually upload the current persistent data files once (mirroring local folder structure):
   - `CrosswordDatabase.json`
   - `Grids/GridTemplateNormals.json`
   - `Grids/usedTemplates_easy.json`
   - `Crosswords/easycw_1.json` … `easycw_18.json`
5. Copy the container base URL + SAS token into `CloudConfig.cs`

---

## CloudConfig.cs

Simple static class — add to `.gitignore`:

```csharp
public static class CloudConfig
{
    // e.g. "https://mystorageaccount.blob.core.windows.net/crosswaves"
    public const string ContainerUrl = "YOUR_CONTAINER_URL";
    // SAS token string starting with "?" e.g. "?sv=2022-11-02&ss=b..."
    public const string SasToken = "YOUR_SAS_TOKEN";
}
```

---

## CloudSyncService.cs — design

MonoBehaviour singleton (`DontDestroyOnLoad`). Two public entry points:

### `IEnumerator DownloadAll(System.Action onComplete)`
1. Call Azure List Blobs API:
   `GET {ContainerUrl}?restype=container&comp=list{SasToken}`
   Returns XML listing every blob name in the container.
2. Parse blob names from XML using `System.Xml.XmlDocument`.
3. For each blob: `GET {ContainerUrl}/{blobName}{SasToken}`
4. Mirror blob name to local path under `Application.persistentDataPath`:
   - `Crosswords/easycw_1.json` → `{persistentDataPath}/Crosswords/easycw_1.json`
   - `CrosswordDatabase.json` → `{persistentDataPath}/CrosswordDatabase.json`
5. `Directory.CreateDirectory` as needed, then `File.WriteAllText`.
6. Call `onComplete` when all blobs are done.

### `IEnumerator UploadFile(string localPath, string blobName)`
1. `File.ReadAllText(localPath)` to get content.
2. `PUT {ContainerUrl}/{blobName}{SasToken}` with header `x-ms-blob-type: BlockBlob` and UTF-8 body.

### `IEnumerator UploadCrosswordBatch(string difficulty, int crosswordNumber)`
Convenience — calls `UploadFile` three times in sequence:
1. `Crosswords/{difficulty}cw_{crosswordNumber}.json`
2. `CrosswordDatabase.json`
3. `Grids/usedTemplates_{difficulty}.json`

---

## RunTimeStartUp.cs changes

```csharp
public class RunTimeStartUp : MonoBehaviour
{
    public static bool IsSyncComplete { get; private set; }

    IEnumerator Start()
    {
        var svc = gameObject.AddComponent<CloudSyncService>();
        yield return svc.DownloadAll(() => IsSyncComplete = true);
    }
}
```

`IsSyncComplete` lets any UI that loads crossword data wait for sync before reading files.

---

## CrosswordGenerator.cs changes

In `GenerateBatch`, after `CrosswordUtils.SaveNewCrossword(...)`:

```csharp
CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
// Upload the three files that just changed
var svc = FindObjectOfType<CloudSyncService>();
if (svc != null)
    yield return svc.UploadCrosswordBatch(SelectedDifficulty, newCrosswordNumber);
```

The crossword number (`newCrosswordNumber`) is `files.Length + 1` — compute it before calling `SaveNewCrossword` so it can be passed to the upload.

---

## Error handling

- Download fails (no internet, bad SAS): log warning, proceed with local files — game works offline.
- Upload fails: log warning but don't block generation — the other machine will pull on next boot.

---

## Verification

1. Run game on laptop A — confirm files download from Azure into `persistentDataPath`.
2. Run CrosswordGenerator — confirm new `easycw_N.json` appears in Azure portal blob list.
3. On laptop B, boot game — confirm it picks up the new crossword.
4. Test offline: disconnect internet, boot — confirm game loads with local files.
