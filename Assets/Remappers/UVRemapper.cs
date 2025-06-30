using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class UVRemapper : MonoBehaviour
{
    public Loader loader;
    void Start()
    {
        //LoadPrefs();
        //debug remapping
        //sample mapping copying top left 20 pixel block to another space on screen
        //mappings.Add(new UVMapping(new Vector4(0, 0, 100, 100), new Vector2(500, 500))); // Example mapping: Copy from (0,0) to (100,100) with size (20,20)
    }

    public void RemapUVs(ref Texture2D tex)
    {
        Profiler.BeginSample("UV Remap");
        var mappings = loader.showconf.mappingsUV;
        //create a internal copy of the colors to avoid modifying the original array
        foreach (var mapping in mappings)
        {
            Color[] source = tex.GetPixels(
                mapping.SourceX,
                tex.height - mapping.SourceY - mapping.SourceHeight, // Invert Y coordinate for Unity's texture coordinate system
                mapping.SourceWidth,
                mapping.SourceHeight);

            tex.SetPixels(
                mapping.TargetX,
                tex.height - mapping.TargetY - mapping.SourceHeight, // Invert Y coordinate for Unity's texture coordinate system
                mapping.SourceWidth,
                mapping.SourceHeight,
                source);
        }

        tex.Apply();
        Profiler.EndSample();
    }

    public struct UVMapping
    {
        public int SourceX;
        public int SourceY;
        public int SourceWidth;
        public int SourceHeight;
        public int TargetX;
        public int TargetY;

        public UVMapping(int sourceX, int sourceY, int sourceWidth, int sourceHeight, int targetX, int targetY)
        {
            SourceX = sourceX;
            SourceY = sourceY;
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
            TargetX = targetX;
            TargetY = targetY;
        }
    }
}
