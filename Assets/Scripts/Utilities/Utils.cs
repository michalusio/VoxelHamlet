using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class Utils
    {
        public static bool IsVisibleFrom(Vector3 meshNormal, Vector3 meshPos, Vector3 camPos)
        {
            return Vector3.Dot(meshPos - camPos, meshNormal) > 0;
        }

        public static Vector3 Div(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 Inverse(this Vector3 a)
        {
            return new Vector3(1 / a.x, 1 / a.y, 1 / a.z);
        }

        public static Vector3 Abs(this Vector3 a)
        {
            Vector3 res = default;
            res.x = Mathf.Abs(a.x);
            res.y = Mathf.Abs(a.y);
            res.z = Mathf.Abs(a.z);
            return res;
        }

        public static Vector3Int Abs(this Vector3Int a)
        {
            Vector3Int res = default;
            res.x = Mathf.Abs(a.x);
            res.y = Mathf.Abs(a.y);
            res.z = Mathf.Abs(a.z);
            return res;
        }

        public static int Sum(this Vector3Int a)
        {
            return a.x + a.y + a.z;
        }

        public static Vector3 _x00(this Vector3 a)
        {
            Vector3 res = default;
            res.x = a.x;
            res.y = 0;
            res.z = 0;
            return res;
        }

        public static Vector3 _x0z(this Vector3 a)
        {
            Vector3 res = default;
            res.x = a.x;
            res.y = 0;
            res.z = a.z;
            return res;
        }

        public static Vector3 _0y0(this Vector3 a)
        {
            Vector3 res = default;
            res.x = 0;
            res.y = a.y;
            res.z = 0;
            return res;
        }

        public static Vector3 _00z(this Vector3 a)
        {
            Vector3 res = default;
            res.x = 0;
            res.y = 0;
            res.z = a.z;
            return res;
        }

        public static Vector4 ToVector4(this Color c)
        {
            Vector4 res = default;
            res.w = c.a;
            res.x = c.r;
            res.y = c.g;
            res.z = c.b;
            return res;
        }

        public static Vector4 ToVector4(this Vector3 v)
        {
            Vector4 res = default;
            res.w = 0;
            res.x = v.x;
            res.y = v.y;
            res.z = v.z;
            return res;
        }

        public static Vector3 ToVector3(this Vector3Int v)
        {
            Vector3 res = default;
            res.x = v.x;
            res.y = v.y;
            res.z = v.z;
            return res;
        }

        public static int Sign(int a)
        {
            return (int)Mathf.Sign(a);
        }

        public static int AddAbsolute1(int a)
        {
            if (a < 0)
            {
                return a - 1;
            }
            return a + 1;
        }

        public static int Add1IfNegative(int a)
        {
            if (a < 0)
            {
                return a + 1;
            }
            return a;
        }

        public static float Add1IfNegative(float a)
        {
            if (a < 0)
            {
                return a + 1;
            }
            return a;
        }

        public static Vector3Int Add1IfNegative(Vector3Int a)
        {
            if (a.x < 0)
            {
                a.x += 1;
            }
            if (a.y < 0)
            {
                a.y += 1;
            }
            if (a.z < 0)
            {
                a.z += 1;
            }
            return a;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (key == null) return defaultValue;
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        public static TValue GetReadOnlyValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (key == null) return defaultValue;
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
