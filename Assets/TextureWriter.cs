using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtNet;
using Klak.Spout;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class TextureWriter : MonoBehaviour
{
    public DmxManager dmxManager;
    public TextureReader reader;
    public bool Transcode = true;
    public string transcodeKey = "Transcode";
    public Toggle transcodeToggle;
    public Texture2D texture;
    public const int TextureWidth = 1920;
    public const int TextureHeight = 1080;
    public SpoutSender spoutSender;

    public ChannelRemapper channelRemapper;
    public UVRemapper uvRemapper;
    public Loader loader;

    public List<int> maskedChannels = new List<int>();
    public bool invertMask = false;
    private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    public TextMeshProUGUI frameTime;

    void Start()
    {
        //set a target framerate to 60
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //maskedChannels.AddRange(Enumerable.Range(0, 25));
        //maskedChannels.Add(52);
        //maskedChannels.Add(102);

        texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        spoutSender.sourceTexture = texture;

        bool transcodeValue = PlayerPrefs.GetInt(transcodeKey, 0) == 1;
        transcodeToggle.isOn = transcodeValue;
        Transcode = transcodeValue;

        //setup callback
        transcodeToggle.onValueChanged.AddListener((value) =>
        {
            Transcode = value;
            PlayerPrefs.SetInt(transcodeKey, value ? 1 : 0);
        });
    }

    // Update is called once per frame
    void Update()
    {
        //start a profiler timer
        timer.Restart();

        Color32[] pixels = new Color32[TextureWidth * TextureHeight];

        Profiler.BeginSample("Texture Clear");
        //fill with transparent
        var color = new Color32(0,0,0,0);
        Array.Fill(pixels, color);
        Profiler.EndSample();

        Profiler.BeginSample("DMX Merge");
        List<byte> mergedDmxValues = new List<byte>();
        if (Transcode)
        {
            mergedDmxValues = reader.dmxData.ToList();
        }
        else
        {
            var universeCount = dmxManager.Universes().Length;

            //merge all universes into one byte array
            for (ushort u = 0; u < universeCount; u++)
            {
                byte[] dmxValues = dmxManager.DmxValues(u);
                mergedDmxValues.AddRange(dmxValues);
            }
        }
        Profiler.EndSample();

        //remap channels
        channelRemapper.RemapChannels(ref mergedDmxValues);

        loader.currentSerializer.InitFrame();

        Profiler.BeginSample("Serializer Loop");
        for (int i = 0; i < mergedDmxValues.Count; i++)
        {
            //skip the channel if its masked
            if (maskedChannels.Contains(i) ^ invertMask)
            {
                continue;
            }

            Profiler.BeginSample("Individual Channel Serialization");
            loader.currentSerializer.SerializeChannel(ref pixels, mergedDmxValues[i], i, TextureWidth, TextureHeight);
            Profiler.EndSample();
        }
        Profiler.EndSample();

        //send to the UV Remapper

        Profiler.BeginSample("Texture Write");
        texture.SetPixels32(pixels);
        texture.Apply();
        Profiler.EndSample();
        uvRemapper.RemapUVs(ref texture);

        timer.Stop();

        frameTime.text = $"Serialization Time: {timer.ElapsedMilliseconds} ms";
    }

    public static int PixelToIndex(int x, int y)
    {
        //check if its in bounds, and return -1 if not
        if (x < 0 || x >= TextureWidth || y < 0 || y >= TextureHeight)
        {
            return -1;
        }

        //make sure y is flipped
        y = TextureHeight - 1 - y;
        return y * TextureWidth + x;
    }

    public static void MakeColorBlock(ref Color32[] pixels, int x, int y, Color32 color, int size)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int index = PixelToIndex(x + i, y + j);
                if (index == -1) return;
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = color;
                }
            }
        }
    }

    public static void MixColorBlock(ref Color32[] pixels, int x, int y, byte channelValue, ColorChannel channel, int size)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int index = PixelToIndex(x + i, y + j);
                if (index == -1) return;
                if (index >= 0 && index < pixels.Length)
                {
                    //get the pixel color
                    Color32 pixelColor = pixels[index];

                    //assign just to the channel
                    switch (channel)
                    {
                        case ColorChannel.Red:
                            pixelColor.r = channelValue;
                            break;
                        case ColorChannel.Green:
                            pixelColor.g = channelValue;
                            break;
                        case ColorChannel.Blue:
                            pixelColor.b = channelValue;
                            break;
                    }
                    //force alpha to 255
                    pixelColor.a = 255;
                    pixels[index] = pixelColor;
                }
            }
        }
    }

    public enum ColorChannel
    {
        Red,
        Green,
        Blue
    }
}
