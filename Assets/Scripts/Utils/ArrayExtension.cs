using System;

namespace Utils
{
    public static class ArrayExtension
    {
        public static T[] TrimArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}