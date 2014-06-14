using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
    public static string UppercaseFirst(this string s)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }
    public static bool IsEmpty<T>(this IEnumerable<T> list)
    {
        if (list is ICollection<T>) return ((ICollection<T>)list).Count == 0;
        return !list.Any();
    }
}
public static class Utility
{
    public static Vector3 Vector3Multiply(Vector3 vector1, Vector3 vector2)
    {
        return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
    }
    public static Vector3 Vector3Divide(Vector3 vector1, Vector3 vector2)
    {
        if (Math.Abs(vector2.x - 0) < float.Epsilon || Math.Abs(vector2.y - 0) < float.Epsilon || Math.Abs(vector2.z - 0) < float.Epsilon)
        {
            Debug.Log("Can't divide by 0");
            return vector1;
        }
        return new Vector3(vector1.x / vector2.x, vector1.y / vector2.y, vector1.z / vector2.z);
    }
}