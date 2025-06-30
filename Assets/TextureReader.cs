using ArtNet;
using Klak.Spout;
using UnityEngine;

public class TextureReader : MonoBehaviour
{
    public const int TextureWidth = 1920;
    public const int TextureHeight = 1080;
    public byte[] dmxData;
    public int universesToRead = 1;
    public Loader loader;
    public RenderTexture texture;
    public Texture2D texture2D;

    void Start()
    {
        texture2D = new Texture2D(TextureWidth, TextureHeight, TextureFormat.BGRA32, false);
    }

    void Update()
    {
        dmxData = new byte[universesToRead * 512];

        //set RT active
        RenderTexture.active = texture;
        texture2D.ReadPixels(new Rect(0, 0, TextureWidth, TextureHeight), 0, 0);
        RenderTexture.active = null;
        Color[] pixels = texture2D.GetPixels();

        for (int i = 0; i < universesToRead * 512; i++)
        {
            loader.currentDeserializer.DeserializeChannel(pixels, ref dmxData[i], i, TextureWidth, TextureHeight);
        }
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

    public static Color GetColor(Color[] pixels, int x, int y)
    {
        int index = PixelToIndex(x, y);
        if (index == -1) return new Color(0, 0, 0, 0); // Return transparent black if out of bounds
        if (index >= 0 && index < pixels.Length)
        {
            //need to convert from srgb to linear
            return pixels[index].gamma;
        }
        else
        {
            return new Color(0, 0, 0, 0); // Return transparent black if out of bounds
        }
    }
}
