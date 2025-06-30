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
                (int)mapping.SourceUV.x,
                tex.height - (int)mapping.SourceUV.y - (int)mapping.SourceUV.w, // Invert Y coordinate for Unity's texture coordinate system
                (int)mapping.SourceUV.z,
                (int)mapping.SourceUV.w);

            tex.SetPixels(
                (int)mapping.TargetUV.x,
                tex.height - (int)mapping.TargetUV.y - (int)mapping.SourceUV.w, // Invert Y coordinate for Unity's texture coordinate system
                (int)mapping.SourceUV.z,
                (int)mapping.SourceUV.w,
                source);
        }

        tex.Apply();
        Profiler.EndSample();
    }

    public struct UVMapping
    {
        public Vector4 SourceUV; //x y is top left, z w is width height
        public Vector2 TargetUV; //x y is top left

        public UVMapping(Vector4 sourceUV, Vector2 targetUV)
        {
            SourceUV = sourceUV;
            TargetUV = targetUV;
        }
    }
}
