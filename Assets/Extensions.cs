using System.Collections.Generic;
using System.Text;
using UnityEngine;


public static class Extensions
{
    /// <summary>
    /// Ensures that a list has at least the specified capacity, expanding it with default values if necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public static List<T> EnsureCapacity<T>(this List<T> list, int capacity)
    {
        if (list.Count < capacity)
        {
            //expand it by the proper amount
            int difference = capacity - list.Count;
            list.AddRange(new T[difference]);
        }
        return list;
    }

    public static List<T> SetRange<T>(this List<T> list, int startIndex, int count, T[] value)
    {
        //ensure the list has enough capacity
        list.EnsureCapacity(startIndex + count);

        for (int i = startIndex; i < startIndex + count; i++)
        {
            list[i] = value[i - startIndex];
        }
        return list;
    }

    public static string ToNewlineString<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return "EMPTY LIST";

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            result.Append(list[i] + "\n");
        }
        return result.ToString();
    }

    public static string ToDelineatedString<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return "EMPTY LIST";

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            result.Append(list[i] + " ");
        }
        return result.ToString();
    }

    public static string ToHexString(this List<byte> bytes)
    {
        StringBuilder hex = new StringBuilder(bytes.Count * 2);
        foreach (byte b in bytes)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    public static string ToDMXString(this Color col)
    {
        //get the byte representation of the color
        byte r = (byte)(col.r * 255);
        byte g = (byte)(col.g * 255);
        byte b = (byte)(col.b * 255);
        byte a = (byte)(col.a * 255);
        return (char)r + "" + (char)g + "" + (char)b + "" + (char)a;
    }

    public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize) 
    {
        //Debug.Log($"Dequeueing chunk of size {chunkSize} from queue with {queue.Count} items.");
        for (int i = 0; i < chunkSize && queue.Count > 0; i++)
        {
            yield return queue.Dequeue();
        }
    }
}
