using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageHelper 
{
    public static string GetVoiceLanguageCode(LanguageType language)
    {
        string l = ConvertLanguageTypeToString(language);
        switch (language)
        {
            case LanguageType.ENGLISH:
                return l + "-GB";
            default:
                return l + "-" + l.ToUpper();
        }
        
    }
    public static string GetVoiceLanguageName(LanguageType language, GenderHelper.GenderType gender)
    {
        
        if (gender == GenderHelper.GenderType.FEMALE)
        {
            return LanguageHelper.GetVoiceLanguageCode(language) + "-Standard-A";
        }
        else
        {
            return LanguageHelper.GetVoiceLanguageCode(language) + "-Standard-B";
        }
        
    }

    public static string ConvertLanguageTypeToString(LanguageType t)
    {
        switch (t)
        {
            case LanguageType.ENGLISH:
                return "en";
            case LanguageType.GERMAN:
                return "de";
            case LanguageType.FRENCH:
                return "fr";
            case LanguageType.PORTGUESE:
                return "pt";
            case LanguageType.PERSIAN:
                return "fa";
            default:
                return "en";
        }
    }

    public enum LanguageType
    {
        ENGLISH,
        GERMAN,
        FRENCH,
        PORTGUESE,
        PERSIAN
    }

}
