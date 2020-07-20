using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleCloudTTS
{
    [System.Serializable]
    public class SpeechSynthesizerClient
{
        private string ServiceEndPoint = "https://texttospeech.googleapis.com";
        private string RestResource = "/v1/text:synthesize";
        private string APIKey;
        public SpeechSynthesizerResponse response;

        public SpeechSynthesizerClient()
        {
            this.ServiceEndPoint = GoogleCloudService.TextToSpeechServiceEndPoint;
            this.RestResource = GoogleCloudService.TextToSpeechRestResource;
            this.APIKey = GoogleCloudService.APIKey;
        }

        public string GetURL()
        {
            return this.ServiceEndPoint + this.RestResource + "?key=" + this.APIKey;
        }

        /// <summary>
        /// Invokes the Google Cloud Text-To-Speech service.
        /// </summary>
        /// <param name="req">The request to execute.</param>
        /// <returns></returns>
        public IEnumerator SendWebRequest(SpeechSynthesizerRequest req)
        {
            //serialize the request object to JSON
            string jsonData = JsonUtility.ToJson(req, false);
            //Debug.Log(jsonData);
            if (jsonData != string.Empty)
            {
                //if there is JSON data to post

                //byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
                //switched to UTF8 encoding due to compile errors.
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

                //construct the URL - this is the URL that the request will be posted to
                string url = this.GetURL();
                //Debug.Log(url);
                UnityWebRequest unityReq = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

                //create an upload handler
                unityReq.uploadHandler = new UploadHandlerRaw(postData);
                //create a downloadhandler
                DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
                unityReq.downloadHandler = dH;
                //set the request header
                unityReq.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

                //wait until the download is done
                yield return unityReq.SendWebRequest();
                if (unityReq.isNetworkError || unityReq.isHttpError)
                {
                    //something went wrong with the download
                    //therefore, create a null response and exit
                    this.response = new SpeechSynthesizerResponse(true, null);
                    this.response.error = unityReq.error;
                    yield break;
                }
                else
                {
                    //get the response text
                    string resText = unityReq.downloadHandler.text;
                    //the response text is in JSON, so we need to convert it
                    GoogleSpeechSynthesizerResponse res = JsonUtility.FromJson<GoogleSpeechSynthesizerResponse>(resText);
                    //the synthesized audio is represented as a base64-encoded string
                    //we need to convert this to an audio clip

                    //convert the string to a byte array
                    byte[] bytes = System.Convert.FromBase64String(res.audioContent);
                    
                    //had trouble converting to float array for the purpose of creating an audio clip
                    //so serializing byte to audio file
                    string path = Application.temporaryCachePath;
                    string filename = System.IO.Path.Combine(path, "speech.wav");
                    //Debug.Log("Speech output file: " + filename);
                    System.IO.File.WriteAllBytes(filename, bytes);

                    //now we need to load the audio file
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filename, AudioType.WAV))
                    {
                        yield return www.SendWebRequest();
                        if (www.isNetworkError || www.isHttpError)
                        {
                            //we encountered an error, so create null response
                            this.response = new SpeechSynthesizerResponse(true, null);
                            this.response.error = unityReq.error;
                            yield break;
                        }
                        else
                        {
                            //the clip was loaded
                            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                            this.response = new SpeechSynthesizerResponse(false, clip);
                            yield break;
                        }
                    }
                }
            }
            yield break;  
        }

    }

    /// <summary>
    /// Represent the response that is returned from the Google Cloud Text-To-Speech service.
    /// </summary>
    [System.Serializable]
    public class GoogleSpeechSynthesizerResponse
    {
        public string audioContent;
    }

    /// <summary>
    /// A wrapper for the <see cref="GoogleSpeechSynthesizerResponse"/> class.
    /// </summary>
    [System.Serializable]
    public class SpeechSynthesizerResponse
    {
        public bool isError = false; //set to true if an error occurred
        public AudioClip audioClip; //the audioclip returned by the response.
        public string error; //a string providing information about errors.

        public SpeechSynthesizerResponse(bool isError, AudioClip clip)
        {
            this.isError = isError;
            this.audioClip = clip;
        }
    }

    /// <summary>
    /// Represents the request that will be sent to the Google Cloud Text-to-Speech service. Instances of this class are serialized to JSON.
    /// </summary>
    [System.Serializable]
    public class SpeechSynthesizerRequest
    {
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;
    
    }

    /// <summary>
    /// Represents the text to be verbalized.
    /// </summary>
    [System.Serializable]
    public class Input
    {
        public string text;
    }

    /// <summary>
    /// Provides information about the voice to use for the verbalization of the input text.
    /// </summary>
    [System.Serializable]
    public class Voice
    {
        public string languageCode = "en-GB";
        public string name = "en-GB-Standard-A";
        public string ssmlGender = "FEMALE";
    }

    /// <summary>
    /// Represents information about the type of audio encoding to use for the verbalized text.
    /// </summary>
    [System.Serializable]
    public class AudioConfig
    {
        public string audioEncoding;
    }

}
