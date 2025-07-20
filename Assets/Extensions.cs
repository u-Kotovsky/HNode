using System.Collections.Generic;
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

    public static string ToDMXString(this Color col)
    {
        //get the byte representation of the color
        byte r = (byte)(col.r * 255);
        byte g = (byte)(col.g * 255);
        byte b = (byte)(col.b * 255);
        byte a = (byte)(col.a * 255);
        return (char)r + "" + (char)g + "" + (char)b + "" + (char)a;
    }
}
