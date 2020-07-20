using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenderHelper 
{

    public static string ConvertGenderTypeToString(GenderType t)
    {
        return t.ToString();
    }

    public enum GenderType
    {
        MALE,
        FEMALE
    }

}
