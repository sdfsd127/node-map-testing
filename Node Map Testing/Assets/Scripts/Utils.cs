using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathHelper
{
    public static class Utils
    {
        public static Vector2 CrossProduct(Vector2 lhs, Vector2 rhs)
        {
            Vector3 LHS = new Vector3(lhs.x, lhs.y, 0);
            Vector3 RHS = new Vector3(rhs.x, rhs.y, 0);

            return CrossProduct(LHS, RHS);
        }

        public static Vector3 CrossProduct(Vector3 lhs, Vector3 rhs)
        {
            Vector3 crossProduct;

            // (Ay * Bz) - (Az * By) = Cx
            crossProduct.x = (lhs.y * rhs.z) - (lhs.z * rhs.y);

            // (Az * Bx) - (Ax * Bz) = Cy
            crossProduct.y = (lhs.z * rhs.x) - (lhs.x * rhs.z);

            // (Ax * By) - (Ay * Bx) = Cz
            crossProduct.z = (lhs.x * rhs.y) - (lhs.y * rhs.x);

            return crossProduct;
        }

        public static float CrossProductMagnitude(Vector2 lhs, Vector2 rhs)
        {
            // (Ax * By) - (Ay * Bx) = Cz
            return (lhs.x * rhs.y) - (lhs.y * rhs.x);
        }

        public static float DotProduct(Vector2 lhs, Vector2 rhs)
        {
            // (Ax * Bx) + (Ay * By) = Dot Product
            // 0.0f = Perpendicular
            return ((lhs.x * rhs.x) + (lhs.y * rhs.y));
        }

        public static float DotProduct(Vector3 lhs, Vector3 rhs)
        {
            // (Ax * Bx) + (Ay * By) + (Az * Bz) = Dot Product
            // 0.0f = Perpendicular
            return ((lhs.x * rhs.x) + (lhs.y * rhs.y) + (lhs.z * rhs.z));
        }

        public static T GetItem<T>(List<T> list, int index)
        {
            if (index < 0)
                index = list.Count - 1;
            else if (index > list.Count - 1)
                index = 0;

            return list[index];
        }

        public static T GetItem<T>(T[] array, int index)
        {
            if (index < 0)
                index = array.Length - 1;
            else if (index > array.Length - 1)
                index = 0;

            return array[index];
        }

        public static void DebugList<T>(List<T> list)
        {
            foreach (T t in list)
            {
                Debug.Log(t);
            }
        }

        public static void DebugArray<T>(T[] array)
        {
            foreach (T t in array)
            {
                Debug.Log(t);
            }
        }
    }
}