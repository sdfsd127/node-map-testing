using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static bool FindMultiple(Vector2 eq1, Vector2 eq2, out Vector2 multiples)
    {
        // Define output var
        multiples = new Vector2(-1, -1);

        // Setup modifiable vectors to keep the original permanent
        Vector2 modEQ1 = eq1, modEQ2 = eq2;
        // Store the value of the multiple if found
        float eq1Multiple = 0, eq2Multiple = 0;

        bool multipleFound = false;
        const int MAX_LOOPS = 50;

        // Loop through, multiplying each equation and comparing the values
        // Checks (MAX_LOOPS * MAX_LOOPS) combination of multiples to find matching coefficients
        for (int i = 1; i < MAX_LOOPS; i++)
        {
            modEQ1 = new Vector2(eq1.x * i, eq1.y * i);

            for (int j = 1; j < MAX_LOOPS; j++)
            {
                modEQ2 = new Vector2(eq2.x * j, eq2.y * j);

                Debug.Log("CHECKING: (" + modEQ1.x + ", " + modEQ1.y + ") and (" + modEQ2.x + ", " + modEQ2.y + ")");
                Debug.Log(modEQ1.x == modEQ2.x || modEQ1.y == modEQ2.y);

                if (modEQ1.x == modEQ2.x || modEQ1.y == modEQ2.y) // Check if they are equal (POTENTIAL ERROR: floating point multiplications -> Is it going to be accurate enough)
                {
                    multipleFound = true;

                    eq1Multiple = i;
                    eq2Multiple = j;
                    break;
                }
            }

            if (multipleFound) // If a multiple has been found, exit the loop
                break;
        }

        if (multipleFound)
        {
            multiples = new Vector2(eq1Multiple, eq2Multiple);
            return true;
        }
        else
            return false;
    }
}
