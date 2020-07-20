using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DBpedia;
using System.Numerics;
using GoogleCloudTTS;
using System.Security.Cryptography;
using System.IO;
using UnityEngine.Analytics;
using TMPro;
using UnityEngine.Networking;

[RequireComponent(typeof(ImageCapture), typeof(ImageProcessor))]
public class InformationDisplay : MonoBehaviour
{
    public GameObject displayPanel;
    public AudioSource audioPlayer;
    public AudioClip failurePrompt;
    //public Texture2D thumbnailDownloadErrorImage;
    //public float displayDisplacementFromCamera = 1.5f; // position new UIs 1.5 meters in front of the user
    public bool useSpeech = true;
    public GenderHelper.GenderType gender = GenderHelper.GenderType.FEMALE;
    public DisplayPanelManager displayPanelManager;

    //internal int _thumbnailImageWidth = 370;
    //internal int _thumbnailImageHeight = 400;
    internal DBpediaResult _dbpediaResult;

    private ImageProcessor _imageProcessor;
    private ImageCapture _imageCapture;
    private bool _processing = false;
    private Debugger debug;
    private DisplayPanelPosition displayPanelInfo;

    //private TestScript testScript;
   
    internal void ResetProcessing()
    {
        debug.PrintPlus("in reset processing");
        this._imageCapture.ResetProcessing();
        this._imageProcessor.ResetProcessing();
        this._dbpediaResult = null;
        this._processing = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        this._imageProcessor = GetComponent<ImageProcessor>();
        this._imageCapture = GetComponent<ImageCapture>();
        //this.testScript = GetComponent<TestScript>();
        this.displayPanelInfo = GetComponent<DisplayPanelPosition>();
        this.debug = GetComponent<Debugger>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (this._processing)
            return;
        if ((this._imageProcessor.output != null) && (this._imageProcessor._processing == true) && (this._imageProcessor._completed == true))
        {
            //we have some output to process
            this._processing = true;
            this._dbpediaResult = this._imageProcessor.output;
            //this.TestScriptDisplay();
            StartCoroutine(CreateDisplay());
        }
    }

    //private void TestScriptDisplay()
    //{
    //    if (this._dbpediaResult.Success == false)
    //    {
    //        //we could not identify the image, so activate the voice prompt
    //        this.audioPlayer.PlayOneShot(this.failurePrompt);
    //    }
    //    else
    //    {
    //        if (this.useSpeech)
    //        {
    //            SpeakText s = new SpeakText();
    //            StartCoroutine(s.Speak(this._dbpediaResult.Painting + " by " + this._dbpediaResult.Artist, this.audioPlayer, LanguageHelper.LanguageType.ENGLISH, this.gender));
    //        }
    //        //this.testScript.PlaceObject();
    //    }
    //    //finally reset processing
    //    ResetProcessing();
    //}

    IEnumerator CreateDisplay()
    {
        if (this._dbpediaResult.Success == false)
        {
            //we could not identify the image, so activate the voice prompt
            this.audioPlayer.PlayOneShot(this.failurePrompt);
        }
        else
        {
            if (this.useSpeech)
            {
                SpeakText s = new SpeakText();
                StartCoroutine(s.Speak(this._dbpediaResult.Painting + " by " + this._dbpediaResult.Artist, this.audioPlayer, LanguageHelper.LanguageType.ENGLISH, this.gender));
            }

            //now we need to create the SlateUIGUI prefab
            GameObject display = this.InstantiateDisplayPanel();
            this.displayPanelManager.AddPanel(display);
            //configure the panel
            DisplayPanel panelProp = display.GetComponent<DisplayPanel>();
            panelProp.depiction = this._dbpediaResult.Depiction;
            panelProp.paintingUIText.text = "<size=24>" + this._dbpediaResult.Painting + "</size>";
            panelProp.artistUIText.text = "<size=24>" + this._dbpediaResult.Artist + "</size>"; ;
            panelProp.timeUIText.text = (this._imageProcessor._endProcessingTime - this._imageProcessor._beginProcessingTime).ToString();
            panelProp.timeUIText.text = "<size=24>Time: " + panelProp.timeUIText.text + "</size>";
            panelProp.moreInfoLink = this._dbpediaResult.URL;

            //now we need to download a thumbnail of the painting.
            //yield return StartCoroutine(DownloadThumbnailImage(panelProp));
        }
        //finally reset processing
        debug.PrintPlus("ending display");
        ResetProcessing();
        yield break;
    }
    private GameObject InstantiateDisplayPanel()
    {
        UnityEngine.Vector3 position, rotation;
        if (this._imageCapture.useCalculatedDisplayPanelPosition)
        {
            position = this._imageCapture.displayPanelPosition;
            rotation = this._imageCapture.displayPanelRotation;
            return Instantiate(this.displayPanel, position, UnityEngine.Quaternion.Euler(rotation));
        }
        else
        {
            //we need to position the display panel in front of the user
            position = this.GetDisplayPanelLocation();
            //instantiate the UI at a specified distance in front of the main camera.
            GameObject display = Instantiate(this.displayPanel, position, UnityEngine.Quaternion.identity);
            if (display.transform.parent != null)
            {
                debug.Print(display.transform.parent.name);
            }
            display.transform.parent = null;
            //modify the rotation by adding 180
            UnityEngine.Vector3 lookPos = Camera.main.transform.position - position;
            lookPos.y = 0;
            UnityEngine.Quaternion qRotation = UnityEngine.Quaternion.LookRotation(lookPos);
            qRotation *= UnityEngine.Quaternion.Euler(0, 180, 0);
            display.transform.rotation = qRotation;
            return display;
        }       
    }

    //IEnumerator DownloadThumbnailImage(DisplayPanel display)
    //{
    //    UnityWebRequest wr = new UnityWebRequest(this._dbpediaResult.Depiction);
    //    //Debug.Log(this.depiction);
    //    DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
    //    wr.downloadHandler = texDl;
    //    yield return wr.SendWebRequest();
    //    if (!(wr.isNetworkError || wr.isHttpError))
    //    {
    //        //the texture was downloaded, so get a reference to the texture
    //        Texture2D t = texDl.texture;
    //        //we want to resize the texture
    //        int width = t.width;
    //        int height = t.height;
    //        float aspectRatio = (float)width / (float)height;
    //        int newImageWidth, newImageHeight;

    //        //calculate the new height of the image while preserving the aspect ratio
    //        //the image width is fixed by the size of the RawImage UI component in the display
    //        if (width >= height)
    //        {
    //            //if width is great than height, then fix the width
    //            newImageWidth = this._thumbnailImageWidth;
    //            newImageHeight = Mathf.RoundToInt(newImageWidth / aspectRatio);
    //        }
    //        else
    //        {
    //            //else fix the height
    //            newImageHeight = this._thumbnailImageHeight;
    //            newImageWidth = Mathf.RoundToInt(newImageHeight * aspectRatio);
    //        }
           
    //        //now we need to rescale the texture
    //        //TextureScale.Bilinear(t, newImageWidth, newImageHeight);
    //        //now assign the texture to the UI
    //        display.paintingUIImage.texture = t;
    //    }
    //    else
    //    {
    //        //something went wrong
    //        display.paintingUIImage.texture = this.thumbnailDownloadErrorImage;
    //    }
    //}

    private UnityEngine.Vector3 GetDisplayPanelLocation()
    {
        //return UnityEngine.Vector3.zero;
        return Camera.main.transform.position + Camera.main.transform.forward * displayPanelInfo.defaultDistanceFromPlayer;
    }


}
