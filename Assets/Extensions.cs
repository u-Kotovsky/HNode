using System.Collections.Generic;


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
}
