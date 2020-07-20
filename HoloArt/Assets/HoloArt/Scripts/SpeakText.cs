using System;
using System.Collections;
using System.Collections.Generic;
using GoogleCloudTTS;
using UnityEngine;

[Serializable]
public class SpeakText
{ 
    public IEnumerator Speak(string text, AudioSource audioPlayer, LanguageHelper.LanguageType language, GenderHelper.GenderType gender)
    {
        //create a speech synthesizer request object
        SpeechSynthesizerRequest req = new SpeechSynthesizerRequest();
        //specify the text to be verbalized
        GoogleCloudTTS.Input input = new GoogleCloudTTS.Input();
        input.text = text;
        //configure the voice to be used for verbalization
        Voice v = new Voice();
        v.languageCode = LanguageHelper.GetVoiceLanguageCode(language);
        v.name = LanguageHelper.GetVoiceLanguageName(language, gender);
        v.ssmlGender = GenderHelper.ConvertGenderTypeToString(gender);
        //specify the audio configuration
        AudioConfig config = new AudioConfig();
        config.audioEncoding = "LINEAR16"; //this is the only format that works
        //populate the speech synthesizer request object
        req.input = input;
        req.voice = v;
        req.audioConfig = config;
        //creat a SpeechSynthesizerClient to post the request
        SpeechSynthesizerClient client = new SpeechSynthesizerClient();
        //send the request
        yield return client.SendWebRequest(req);
        //the SpeechSynthesizerClient will send the request and create response object
        //get a reference to the response object
        SpeechSynthesizerResponse res = client.response;
        //check if an error was encountered during the request
        if (res.isError)
        {
            //an error occurred during speech synthesis
            Debug.Log("Speech synthesis error: " + res.error);
        }
        else
        {
            //get a reference to the generated audio clip;
            AudioClip clip = res.audioClip;
            //play the clip using the specified audio source
            audioPlayer.PlayOneShot(clip);
        }
    }
}
