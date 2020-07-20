using HoloToolkit.Unity.SpatialMapping;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

[RequireComponent(typeof(ImageProcessor))]
public class ImageCapture : MonoBehaviour
{
    //public int resolutionWidth = 1280;
    //public int resolutionHeight = 720;
    public Camera sourceCamera;
    public Texture2D sampleTexture;
    public bool useSampleTexture;
    //public bool debugMode = true;
    public AudioSource audioPlayer;
    public AudioClip cameraClick;
    public bool enableCameraClick = true;
    //public float displayPanelOffset = 0.2f; // position the display panel 20cm in front of the wall

    internal bool locked = false;
    internal Vector3 hitPoint;
    internal Vector3 displayPanelPosition;
    internal Vector3 displayPanelRotation;
    internal bool useCalculatedDisplayPanelPosition = false;
    //internal Texture2D capturedImage = null;
    //internal bool completed = false;

    private ImageProcessor _imageProcessor;
    private PhotoCapture photoCaptureObject = null;
    //private Texture2D targetTexture = null;
    private Debugger debug;
    private DisplayPanelPosition displayPanelInfo;
     

    public void ResetProcessing()
    {
        this.hitPoint = Vector3.zero;
        displayPanelPosition = Vector3.zero;
        displayPanelRotation = Vector3.zero;
        useCalculatedDisplayPanelPosition = false;
        //useSampleTexture = false; /// gesture will set this to true,  this is for debugging purposes
        this.locked = false;
        //this.targetTexture = null;
        //this.capturedImage = null;
        //this.completed = false;
        //this.debug.Print("Debug");
    }

    void Start()
    {
        this.debug = GetComponent<Debugger>();
        if (sourceCamera == null)
            this.sourceCamera = Camera.main;
        this._imageProcessor = GetComponent<ImageProcessor>();
        this.displayPanelInfo = GetComponent<DisplayPanelPosition>();
        this.ResetProcessing();
        //this.InitCamera();
    }

    

    public void GetImage()
    {
        
        //check to see if the component is locked
        //if so, return
        if (this.locked == true)
            return;
        this.locked = true;
        //lock the component, so we don't begin an additional processing cycle
        //if (this.debugMode == false)
        //{
        //    this.locked = true;
        //}
        if (enableCameraClick && (this.cameraClick != null))
        {
            this.audioPlayer.PlayOneShot(this.cameraClick);
        }
        
        if (this.useSampleTexture && (this.sampleTexture != null))
        {
            //PrintDebugMessage("set texture");
            this._imageProcessor._sourceImage = this.sampleTexture;
            //this.capturedImage = this.sampleTexture;
            //this.completed = true;
            return;
        }

        //calculate the position and target of the display panel
        //this.CalculateDisplayPanelPosition();
        //CalculateDisplayPosition seems to throw an exception relating to SpatialMapping (SpatialMapping called too early).
        this.displayPanelPosition = Vector3.zero;
        this.displayPanelRotation = Vector3.zero;
        this.useCalculatedDisplayPanelPosition = false;

        this.TakePhoto();

        //Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
        ////Resolution cameraResolution = new Resolution();
        //cameraResolution.width = this.resolutionWidth;
        //cameraResolution.height = this.resolutionHeight;
        //this.targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        //insert text to test on Unity

        // Create a PhotoCapture object
        //PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
        //    photoCaptureObject = captureObject;
        //    CameraParameters cameraParameters = new CameraParameters();
        //    cameraParameters.hologramOpacity = 0.0f;
        //    cameraParameters.cameraResolutionWidth = cameraResolution.width;
        //    cameraParameters.cameraResolutionHeight = cameraResolution.height;
        //    cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        //    // Activate the camera
        //    photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
        //        // Take a picture
        //        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        //    });
        //});
        //this.completed = true;
    }

    private  bool GetLookAtPosition(Vector3 headPosition, Vector3 gazeDirection, out Vector3 hitPoint)
    {
        float maxDistance = 10f;

        if (SpatialMappingManager.Instance == null)
        {
            hitPoint = Vector3.zero;
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(headPosition, gazeDirection, out hit, maxDistance, SpatialMappingManager.Instance.LayerMask))
        {
            Vector3 physicalHitPoint = hit.point;
            Vector3 v = physicalHitPoint - headPosition; //goto = v
            v = ((v.magnitude - this.displayPanelInfo.defaultDistanceFromWall) / v.magnitude) * v;
            hitPoint = v;
            return true;
        }
        else
        {
            hitPoint = Vector3.zero;
            return false;
        }
    }

    public void CalculateDisplayPanelPosition()
    {
        Vector3 positionToPlace;
        Transform camTransform = Camera.main.transform;
        Vector3 lookBack = camTransform.rotation.eulerAngles;
        lookBack.x = lookBack.z = 0f;
        lookBack.y += 180f;

        if (GetLookAtPosition(camTransform.position, camTransform.forward, out positionToPlace))
        {
            //InstantiateObject(positionToPlace, lookBack);
            this.displayPanelPosition = positionToPlace;
            this.displayPanelRotation = lookBack;
            this.useCalculatedDisplayPanelPosition = true;
        }
        else
        {
            this.displayPanelPosition = Vector3.zero;
            this.displayPanelRotation = Vector3.zero;
            this.useCalculatedDisplayPanelPosition = false;
        }
    }


    //void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    //{
    //    // Copy the raw image data into our target texture
    //    this.debug.Print("capturedphoto");
    //    photoCaptureFrame.UploadImageDataToTexture(targetTexture);
    //    photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    //    this._imageProcessor._sourceImage = this.targetTexture;
    //}

    //void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    //{
    //    // Shutdown our photo capture resource
    //    photoCaptureObject.Dispose();
    //    photoCaptureObject = null;
    //}

    //microsoft code

    //private void InitCamera()
    //{
    //    if (this.photoCaptureObject == null)
    //        return;
    //    PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    //}

    void TakePhoto()
    {
        //this.debug.PrintPlus("taking photo");
        ////create a photocapture object
        //if (this.photoCaptureObject == null)
        //{
        //    this.InitCamera();
        //}
        //else
        //{
        //    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        //}
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        this.debug.PrintPlus("in onphotocapturecreate");
        //reference the captureobject
        photoCaptureObject = captureObject;
        //find the first camera resolution
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        //create a camera parameters object
        CameraParameters c = new CameraParameters();
        //set the hologram opacity to zero to prevent hologram rendering
        c.hologramOpacity = 0.0f;
        //set the camera resolution width
        c.cameraResolutionWidth = cameraResolution.width;
        //set the camera resolution height
        c.cameraResolutionHeight = cameraResolution.height;
        //set the pixel format
        c.pixelFormat = CapturePixelFormat.BGRA32;
        //start photo capture mode
        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }
   
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        this.debug.PrintPlus("in onphotomodestarted");
        if (result.success)
        {
            this.debug.PrintPlus("result as success");
            //switch to photo capture mode was successful
            //take a photo
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            this.debug.PrintPlus("Unable to start photo mode!");
            //an error occurred so we need to reset processing
            this.ResetProcessing();
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        this.debug.PrintPlus("in oncapturephototomemory");
        if (result.success)
        {
            // Create our Texture2D for use and set the correct resolution
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            //create new texture object
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            //stop the photo mode
            //photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            //set the new texture as the sourceimage of the image processor object
            this.debug.PrintPlus("setting imageprocessor texture");
            this._imageProcessor._sourceImage = targetTexture;
            //this.capturedImage = this.targetTexture;
            //this.debug.Print("capturedphoto");
        }
        else
        {
            //an error occurred, so we need to reset processing
            this.debug.PrintPlus("could not capture photo...resetting");
            this.ResetProcessing();
        }
        // finally implement clean up operations
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        this.debug.PrintPlus("in onstoppedphotomode");
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }





}
