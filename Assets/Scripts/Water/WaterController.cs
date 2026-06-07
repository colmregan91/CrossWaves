using UnityEngine;

public class WaterController : MonoBehaviour
{
    [SerializeField] private Material oceanMaterial;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private Camera cam;
    [SerializeField] private int texelsPerCell = 4;  // 13 * 4 = 52x52 SDF texture

    private Texture2D _sdfTex;

    private static readonly int SDFTexID     = Shader.PropertyToID("_SDFTex");
    private static readonly int GridRectUVID = Shader.PropertyToID("_GridRectUV");
    private static readonly int FoamOnID     = Shader.PropertyToID("_FoamOn");

    private const int GridSize = 13;

    private void Awake()
    {
        int texSize = GridSize * texelsPerCell;
        _sdfTex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false, true);
        _sdfTex.wrapMode = TextureWrapMode.Clamp;
        _sdfTex.filterMode = FilterMode.Bilinear;
    }

    private void Start()
    {
        CrosswordManager.Instance.OnGridLoaded += HandleGridLoaded;
        CrosswordManager.Instance.OnPuzzleLeft += HandlePuzzleLeft;
        EnableFoam(false);
    }

    private void HandleGridLoaded()
    {
        var grid = CrosswordManager.Instance.grid;
        bool[,] landMask = new bool[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
            for (int y = 0; y < GridSize; y++)
                landMask[x, y] = grid[x, y] != null && grid[x, y].HasLetter;

        CoastlineSDF.Compute(landMask, GridSize, GridSize, texelsPerCell, _sdfTex);
        oceanMaterial.SetTexture(SDFTexID, _sdfTex);
        PushGridRectUV();
        EnableFoam(true);
    }

    private void PushGridRectUV()
    {
        Vector3[] corners = new Vector3[4];
        gridContainer.GetWorldCorners(corners);
        // corners[0] = bottom-left, corners[2] = top-right
        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        var screenSize = new Vector2(Screen.width, Screen.height);

        oceanMaterial.SetVector(GridRectUVID, new Vector4(
            bl.x / screenSize.x,
            bl.y / screenSize.y,
            tr.x / screenSize.x,
            tr.y / screenSize.y
        ));
    }

    private void HandlePuzzleLeft()
    {
        EnableFoam(false);
    }

    public void EnableFoam(bool on)
    {
        oceanMaterial.SetFloat(FoamOnID, on ? 1f : 0f);
    }

    private void OnDestroy()
    {
        if (CrosswordManager.Instance != null)
        {
            CrosswordManager.Instance.OnGridLoaded -= HandleGridLoaded;
            CrosswordManager.Instance.OnPuzzleLeft -= HandlePuzzleLeft;
        }
        if (_sdfTex != null)
            Destroy(_sdfTex);
    }
}
