using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using DBpedia;
using GoogleCloudVision;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ImageProcessor : MonoBehaviour
{
    public int maxResults = 5;
    //public TMP_Text debugMessage;

    internal Texture2D _sourceImage;
    internal DBpediaResult output = null;
    internal float _beginProcessingTime;
    internal float _endProcessingTime;
    internal bool _processing = false;
    internal bool _completed = false;

    private WebDetection _webDetection;
    private DBpediaResult _dbpediaResult;
    private Debugger debug;
    private ImageCapture _imgCapture;
    //private AnnotateImageResponses annotateImageResponses;


    private void Start()
    {
        this.debug = GetComponent<Debugger>();
        this._imgCapture = GetComponent<ImageCapture>();
        ResetProcessing();
    }

    public void ResetProcessing()
    {
        this._sourceImage = null;
        this._webDetection = null;
        this._dbpediaResult = null;
        this._beginProcessingTime = 0.0f;
        this._endProcessingTime = 0.0f;
        this.output = null;
        //this.annotateImageResponses = null;
        this._completed = false;
        this._processing = false;
    }

    private void Update()
    {
        //if the component is already processing then exit
        if (this._processing == true)
            return;
        //if the component is not processing and there is an image to process, then start processing
        //if (this._sourceImage != null)
        //{
        //    //set processing to true to prevent future processing cycles
        //    this._processing = true;
        //    StartCoroutine(BeginProcessing());
        //}
        if (this._imgCapture.locked == true && this._sourceImage != null)
        {
            //the image capture is locked and a source image is available
            this._processing = true;
            StartCoroutine(BeginProcessing());
        }
    }

    //private void PrintDebugMessage(string msg)
    //{
    //    this.debugMessage.text = msg;
    //}

    IEnumerator BeginProcessing()
    {
        this.debug.Print("in beginprocessing");
        //timestamp the beginning of the processing cycle
        this._beginProcessingTime = Time.time;

        //we have the source image.
        //we need to create a request and invoke the machine vision service

        //convert the source image into a base64-encoded string
        byte[] jpg = this._sourceImage.EncodeToJPG();
        string base64 = System.Convert.ToBase64String(jpg);

        //create the request collection object
        AnnotateImageRequests requests = new AnnotateImageRequests();
        requests.requests = new List<AnnotateImageRequest>();
        //create the request object
        AnnotateImageRequest request = new AnnotateImageRequest();
        //configure the request object
        request.image = new Image();
        request.image.content = base64;
        request.features = new List<Feature>();
        Feature feature = new Feature();
        //we are only interested in web detections
        feature.type = FeatureType.WEB_DETECTION.ToString();
        //we only want a single response
        feature.maxResults = this.maxResults;
        request.features.Add(feature);
        requests.requests.Add(request);
        //creat a MachineVisionClient to post the request
        MachineVisionClient client = new MachineVisionClient();
        //send the request
        yield return client.SendWebRequest(requests);
        //the MachineVisionClient will send the request and create response object
        //get a reference to the response object
        MachineVisionResponse res = client.response;
        //check if an error was encountered during the request
        if (res.isError)
        {
            //an error occurred during processing
            Debug.Log("Web detection object is null.");
            //return a null web detection object
            this._webDetection = null;
            //set the DPpedia result to null
            this.output = new DBpediaResult();
            yield break;
        }
        //if we get here, then we have successully returned a web detection from the Google Vision API
        this._webDetection = res.annotateImageResponse.webDetection;
        //now run the dbpedia queries
        yield return StartCoroutine(RunDBPediaQueries());
        if (this._dbpediaResult == null)
        {
            //the retrieval of results from dbpedia failed, so create a null dbpedia result
            this.output = new DBpediaResult();
        }
        else
        {
            this.output = this._dbpediaResult;
        }
        //if we get here then we have a result to process
        //timestamp the end processing time
        this._endProcessingTime = Time.time;
        //mark the processing as completed
        this._completed = true;
        this.debug.Print("db-result: " + this._dbpediaResult.Success.ToString());
    }

    IEnumerator RunDBPediaQueries()
    {
        DBpediaResult db = new DBpediaResult();
        foreach (WebEntity e in this._webDetection.webEntities)
        {
           string desc = e.description;
           string query = GetDBpediaQueryString(desc);
           using (UnityWebRequest req = UnityWebRequest.Get(query))
           {
                //request and wait for the desired page
                yield return req.SendWebRequest();
                if (req.isNetworkError)
                {
                    Debug.Log("Web request error: " + req.error);
                }
                else
                {
                    db = ProcessDBpediaJSON(req.downloadHandler.text);
                    if (db.Success)
                    {
                        break;
                    }
                }
           }
        }
        if (db.Success)
        {
            //we successfully resolved the image
            this._dbpediaResult = db;
        }
        else
        {
            //we failed to resolve the image
            this._dbpediaResult = null;
        }
    }

    private DBpediaResult ProcessDBpediaJSON(string json)
    {
        //Debug.Log(json);
        DBpedia.Root r = JsonUtility.FromJson<DBpedia.Root>(json);
        if (r == null)
            return new DBpediaResult();
        try
        {
            string artist = r.results.bindings[0].artist.value;
            string title = r.results.bindings[0].title.value;
            string depiction = r.results.bindings[0].depiction.value;
            string wikiURL = r.results.bindings[0].url.value;
            return new DBpediaResult(artist, title, depiction, wikiURL);
        }
        catch
        {
            return new DBpediaResult();
        }
    }

    private string GetDBpediaQueryString(string painting)
    {
        string url = "http://dbpedia.org/sparql?";
        string defaultGraphURI = "http://dbpedia.org";
        string query = GetSPARQL(painting);
        string format = "application/sparql-results+json";
        string cxmlRedirSubjs = "121";
        string cxmlRedirHrefs = "";
        string timeout = "30000";
        string debug = "on";
        string rn = "Run Query";
        //build query string
        url += "default-graph-uri" + "=" + URLEncodeString(defaultGraphURI) + "&";
        url += "query" + "=" + URLEncodeString(query) + "&";
        url += "format" + "=" + URLEncodeString(format) + "&";
        url += "CXML_redir_for_subjs" + "=" + URLEncodeString(cxmlRedirSubjs) + "&";
        url += "CXML_redir_for_hrefs" + "=" + URLEncodeString(cxmlRedirHrefs) + "&";
        url += "timeout" + "=" + URLEncodeString(timeout) + "&";
        url += "debug" + "=" + URLEncodeString(debug) + "&";
        url += "run" + "=" + URLEncodeString(rn);
        return url;
    }

    private string GetSPARQL(string painting)
    {
        //?painting foaf:depiction ?depiction . Previously used this to get a depiction of the painting
        //now using dbo:thumbnail to reduce download latency of thumbnail image
        string query;
        query = @"SELECT ?artist ?title ?depiction ?url 
WHERE {
    ?painting rdfs:label ?title .
    ?painting foaf:isPrimaryTopicOf ?url . 
    ?painting dbo:thumbnail ?depiction . 
    ?painting dbo:author ?creator . 
    ?creator rdfs:label ?artist . 
    FILTER(LANG(?artist) = ""en"") . 
    FILTER(LANG(?title) = ""en"") . 
    FILTER(CONTAINS(LCASE(STR(?title)), """ + painting.ToLower() + @""")) 
    } 
LIMIT 1";
        return query;
    }

    private string URLEncodeString(string s)
    {
        return UnityWebRequest.EscapeURL(s);
    }

}
