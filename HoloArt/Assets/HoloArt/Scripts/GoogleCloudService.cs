using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GoogleCloudService
{
    public const string APIKey = "insert Google Platform API here";

    public const string TranslationServiceEndPoint = "https://translation.googleapis.com";
    public const string TranslationRestResource = "/language/translate/v2";

    public const string TextToSpeechServiceEndPoint = "https://texttospeech.googleapis.com";
    public const string TextToSpeechRestResource = "/v1/text:synthesize";

    public const string VisionServiceEndPoint = "https://vision.googleapis.com";
    public const string VisionRestResource = "/v1/images:annotate";
}
