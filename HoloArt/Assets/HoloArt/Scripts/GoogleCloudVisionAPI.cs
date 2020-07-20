using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleCloudVision
{
    [System.Serializable]
    public class MachineVisionClient
    {
        private string ServiceEndPoint = "https://vision.googleapis.com";
        private string RestResource = "/v1/images:annotate";
        private string APIKey;
        public MachineVisionResponse response;

        public MachineVisionClient()
        {
            this.ServiceEndPoint = GoogleCloudService.VisionServiceEndPoint;
            this.RestResource = GoogleCloudService.VisionRestResource;
            this.APIKey = GoogleCloudService.APIKey;
        }

        public string GetURL()
        {
            return this.ServiceEndPoint + this.RestResource + "?key=" + this.APIKey;
        }

        /// <summary>
        /// Invokes the Google Cloud machine vision service.
        /// </summary>
        /// <param name="req">The request to execute.</param>
        /// <returns></returns>
        public IEnumerator SendWebRequest(AnnotateImageRequests requests)
        {
            //serialize the request object to JSON
            string jsonData = JsonUtility.ToJson(requests, false);
            if (jsonData != string.Empty)
            {
                //if there is json data to post
                //create a byte array containing the json data

                //byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
                //changed to UTF8 due to compoile error
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);
                
                //construct the URL
                string url = this.GetURL();

                // Create a a web request object
                UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                //create an upload handler
                req.uploadHandler = new UploadHandlerRaw(postData);
                //create a downloadhandler
                DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
                req.downloadHandler = dH;
                //set the request header
                req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                // Wait until the download is done
                yield return req.SendWebRequest();

                if (req.isNetworkError || req.isHttpError)
                {
                    Debug.Log("Web Request Error: " + req.error);
                    this.response = new MachineVisionResponse(true, null);
                    this.response.error = req.error;
                    yield break;
                }
                else
                {
                    //get the response text
                    string resText = req.downloadHandler.text;
                    //Debug.Log(resText);
                    //instantiate the google response class from json
                    AnnotateImageResponses responses = JsonUtility.FromJson<AnnotateImageResponses>(resText);
                    if ((responses.responses == null) || (responses.responses.Count == 0))
                    {
                        //we don't have any response objects to process
                        this.response = new MachineVisionResponse(true, null);
                        this.response.error = "No response objects to process.";
                        yield break;
                    }
                    //create the wrapper class and add the first response object
                    this.response = new MachineVisionResponse(false, responses.responses[0]);
                 }
            }
            yield break;
        }

    }

    /// <summary>
    /// Represents the request that will be sent to the Google Cloud Vision service. Instances of this class are serialized to JSON.
    /// </summary>
    [System.Serializable]
    public class AnnotateImageRequests
    {
        public List<AnnotateImageRequest> requests;
    }

    [System.Serializable]
    public class AnnotateImageRequest
    {
        public Image image;
        public List<Feature> features;
    }

    [System.Serializable]
    public class Image
    {
        public string content;
    }

    [System.Serializable]
    public class Feature
    {
        public string type;
        public int maxResults;
    }

    /// <summary>
    /// A wrapper for the <see cref="AnnotateImageResponse"/> class.
    /// </summary>
    [System.Serializable]
    public class MachineVisionResponse
    {
        public bool isError = false; //set to true if an error occurred
        public AnnotateImageResponse annotateImageResponse; //the first response returned by the machine vision service.
        public string error; //a string providing information about errors.

        public MachineVisionResponse(bool isError, AnnotateImageResponse response)
        {
            this.isError = isError;
            this.annotateImageResponse = response;
        }
    }

    [System.Serializable]
    public class AnnotateImageResponses
    {
        public List<AnnotateImageResponse> responses;
    }

    /// <summary>
    /// /response objects
    /// </summary>
    [System.Serializable]
    public class AnnotateImageResponse
    {
        public WebDetection webDetection;
    }

    [System.Serializable]
    public class WebDetection
    {
        public List<WebEntity> webEntities;
        public List<MatchingImage> fullMatchingImages;
        public List<MatchingImage> partialMatchingImages;
        public List<PageWithMatchingImage> pagesWithMatchingImages;
        public List<MatchingImage> visuallySimilarImages;
        public List<BestGuessLabel> bestGuessLabels;
    }

    [System.Serializable]
    public class PageWithMatchingImage
    {
        public string url;
        public string pageTitle;
        public List<MatchingImage> fullMatchingImages;
        public List<MatchingImage> partialMatchingImages;
    }

    [System.Serializable]
    public class BestGuessLabel
    {
        public string label;
        public string languageCode; 
    }

    [System.Serializable]
    public class MatchingImage
    {
        public string url;
    }

    [System.Serializable]
    public class WebEntity
    {
        public string entityId;
        public float score;
        public string description;
    }

    public enum FeatureType
    {
        TYPE_UNSPECIFIED,
        FACE_DETECTION,
        LANDMARK_DETECTION,
        LOGO_DETECTION,
        LABEL_DETECTION,
        TEXT_DETECTION,
        DOCUMENT_TEXT_DETECTION,
        SAFE_SEARCH_DETECTION,
        IMAGE_PROPERTIES,
        WEB_DETECTION
    }
}
