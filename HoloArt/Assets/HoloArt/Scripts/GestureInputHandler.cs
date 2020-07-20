using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class GestureInputHandler : MonoBehaviour, IInputClickHandler
{
    private ImageCapture imgCapture;
    //private ImageCaptureTest testScript;

    void Start()
    {
        //this.testScript = this.GetComponent<ImageCaptureTest>();
        this.imgCapture = this.GetComponent<ImageCapture>();
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.PopFallbackInputHandler();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        //need to check if any game object is in focus.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit rayCastInfo;
        if (Physics.Raycast(ray, out rayCastInfo, 10f))
        {
            if (rayCastInfo.transform.gameObject.layer == 5)
            {
                //ignore the UI layer
                return;
            }
        }
        //only run image capture if the user is not looking at a previously rendered UI object.
        this.imgCapture.GetImage();
    }
}
