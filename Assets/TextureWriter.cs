using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtNet;
using Klak.Spout;
using TMPro;
using UnityEngine;

public class TextureWriter : MonoBehaviour
{
    public DmxManager dmxManager;
    public Texture2D texture;
    public const int TextureWidth = 1920;
    public const int TextureHeight = 1080;
    private const string indexKey = "SelectedSerializer";
    public SpoutSender spoutSender;
    public int count = 1;

    List<IDMXSerializer> serializers;
    public TMP_Dropdown serializerDropdown;
    private IDMXSerializer currentSerializer;
    public ChannelRemapper channelRemapper;
    public UVRemapper uvRemapper;

    void Start()
    {
        texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        spoutSender.sourceTexture = texture;

        //load in all the serializers
        var type = typeof(IDMXSerializer);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p));

        serializers = types
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Select(t => (IDMXSerializer)Activator.CreateInstance(t))
            .ToList();

        Debug.Log($"Loaded {serializers.Count} serializers");

        //populate the dropdown
        serializerDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var typei in serializers)
        {
            options.Add(typei.GetType().Name);
        }

        serializerDropdown.AddOptions(options);

        serializerDropdown.onValueChanged.AddListener((s) =>
        {
            currentSerializer = serializers[s];
            Debug.Log($"Selected serializer: {currentSerializer.GetType().Name}");
            PlayerPrefs.SetString(indexKey, currentSerializer.GetType().Name);
        });

        //select the first serializer by default
        if (serializers.Count > 0)
        {
            string prefSavedName = PlayerPrefs.GetString(indexKey, "0");
            int prefSavedIndex = 0;
            //check if a type exists with the name
            if (serializers.Any(s => s.GetType().Name == prefSavedName))
            {
                prefSavedIndex = serializers.FindIndex(s => s.GetType().Name == prefSavedName);
                Debug.Log($"Found saved serializer: {prefSavedName} at index {prefSavedIndex}");
            }
            else
            {
                Debug.LogWarning($"No serializer found with name {prefSavedName}, using index 0 instead.");
            }

            currentSerializer = serializers[prefSavedIndex];
            serializerDropdown.value = prefSavedIndex;
            serializerDropdown.RefreshShownValue();
            Debug.Log($"Default serializer: {currentSerializer.GetType().Name}");
        }
        else
        {
            Debug.LogError("No serializers found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Color32[] pixels = new Color32[TextureWidth * TextureHeight];

        //fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }

        var universes = dmxManager.Universes();

        //merge all universes into one byte array
        List<byte> mergedDmxValues = new List<byte>();
        for (int u = 0; u < universes.Length; u++)
        {
            byte[] dmxValues = dmxManager.DmxValues(universes[u]);
            mergedDmxValues.AddRange(dmxValues);
        }

        //remap channels
        channelRemapper.RemapChannels(ref mergedDmxValues);

        //for (int i = 0; i < mergedDmxValues.Count; i++)
        for (int i = 0; i < mergedDmxValues.Count; i++)
        {
            /*
            if (i > count)
            {
                continue;
            } */

            currentSerializer.MapChannel(ref pixels, mergedDmxValues[i], i, TextureWidth, TextureHeight);
        }

        //send to the UV Remapper

        texture.SetPixels32(pixels);
        texture.Apply();
        uvRemapper.RemapUVs(ref texture);
    }

    public static int PixelToIndex(int x, int y)
    {
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
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = color;
                }
            }
        }
    }
}
