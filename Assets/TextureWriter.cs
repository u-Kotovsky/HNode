using System.Collections;
using System.Collections.Generic;
using ArtNet;
using Klak.Spout;
using UnityEngine;

public class TextureWriter : MonoBehaviour
{
    public DmxManager dmxManager;
    public Texture2D texture;
    public SpoutSender spoutSender;

    void Start()
    {
        texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        spoutSender.sourceTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Turn this into a interface system where different scripts define their screen mapping, and we just output

        Color32[] pixels = new Color32[1920 * 1080];

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

        //for (int i = 0; i < mergedDmxValues.Count; i++)
        for (int i = 0; i < mergedDmxValues.Count; i++)
        {
            /*
            if (i > 50)
            {
                continue;
            }
            */
            MapChannel(ref pixels, mergedDmxValues[i], i);
        }

        //MakeColorBlock(ref pixels, 0, 0, new Color32(255, 0, 0, 255), 10); // Clear the top-left corner

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private int PixelToIndex(int x, int y)
    {
        //make sure y is flipped
        y = texture.height - 1 - y;
        return y * texture.width + x;
    }

    private void MapChannel(ref Color32[] pixels, byte channelValue, int channel)
    {
        //convert the channel to x y
        //x is channel % 24
        //y is channel / 24
        const int blockSize = 4; // 10x10 pixels per channel block
        const int blocksPerCol = 16; // 10 channels per column
                                       //split the value into 8 bits
        var bits = new BitArray(new byte[] { channelValue });
        List<bool> bitsList = new List<bool>();
        for (int i = 0; i < bits.Length; i++)
        {
            bitsList.Add(bits[i]);
        }
        bitsList.Add(false); // Add a dummy bit to make it 9 bits, needed for easy interlacing

        for (int i = 0; i < bitsList.Count; i += 3)
        {
            int newChannel = (channel * 3) + i / 3; //3 because we interlace with color
            int x = (newChannel / blocksPerCol) * blockSize;
            int y = (newChannel % blocksPerCol) * blockSize;
            if (x >= texture.width || y >= texture.height)
            {
                continue; // Skip if the calculated pixel is out of bounds
            }
            //convert the x y to pixel index
            //return 4x4 area
            var color = new Color32(
                (byte)(bitsList[i] ? 255 : 0),
                (byte)(bitsList[i + 1] ? 255 : 0),
                (byte)(bitsList[i + 2] ? 255 : 0),
                255
            );
            MakeColorBlock(ref pixels, x, y, color, blockSize);
        }
    }

    private void MakeColorBlock(ref Color32[] pixels, int x, int y, Color32 color, int size)
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
