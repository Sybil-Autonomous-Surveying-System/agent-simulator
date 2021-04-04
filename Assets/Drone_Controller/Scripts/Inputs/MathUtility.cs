using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static bool IsBetweenRange(this float thisValue, float value1, float value2)
    {
        return thisValue >= Mathf.Min(value1, value2) && thisValue <= Mathf.Max(value1, value2);
    }
}
