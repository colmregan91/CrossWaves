using UnityEngine;

// Computes a distance field from land (letter cells) and writes it into a reused Texture2D.
// Sea texels store normalised distance to nearest land (0 = at coast, 1 = far sea).
// Land texels store 0. The texture is written in-place — never reallocated between puzzles.
public static class CoastlineSDF
{
    // landMask[x, y] = true  → land (HasLetter cell, white)
    // landMask[x, y] = false → sea  (blocked cell, black)
    public static void Compute(bool[,] landMask, int gridW, int gridH, int texelsPerCell, Texture2D outTex)
    {
        int texW = gridW * texelsPerCell;
        int texH = gridH * texelsPerCell;

        bool[] isLand = new bool[texW * texH];
        for (int ty = 0; ty < texH; ty++)
        {
            for (int tx = 0; tx < texW; tx++)
            {
                int gx = Mathf.Clamp(tx / texelsPerCell, 0, gridW - 1);
                int gy = Mathf.Clamp(ty / texelsPerCell, 0, gridH - 1);
                isLand[ty * texW + tx] = landMask[gx, gy];
            }
        }

        // Collect land texel positions for the distance query
        int landCount = 0;
        for (int i = 0; i < isLand.Length; i++)
            if (isLand[i]) landCount++;

        int[] landX = new int[landCount];
        int[] landY = new int[landCount];
        int li = 0;
        for (int ty = 0; ty < texH; ty++)
            for (int tx = 0; tx < texW; tx++)
                if (isLand[ty * texW + tx]) { landX[li] = tx; landY[li] = ty; li++; }

        // Euclidean distance transform — brute force is fast at these tiny resolutions (≤80×80)
        float normalise = texW; // distances are normalised against texture width
        Color[] pixels = new Color[texW * texH];

        for (int ty = 0; ty < texH; ty++)
        {
            for (int tx = 0; tx < texW; tx++)
            {
                int idx = ty * texW + tx;
                if (isLand[idx]) { pixels[idx] = Color.black; continue; }

                float minDistSq = float.MaxValue;
                for (int k = 0; k < landCount; k++)
                {
                    float dx = tx - landX[k];
                    float dy = ty - landY[k];
                    float dSq = dx * dx + dy * dy;
                    if (dSq < minDistSq) minDistSq = dSq;
                }

                float normalized = Mathf.Clamp01(Mathf.Sqrt(minDistSq) / normalise);
                pixels[idx] = new Color(normalized, 0f, 0f, 1f);
            }
        }

        outTex.SetPixels(pixels);
        outTex.Apply();
    }
}
