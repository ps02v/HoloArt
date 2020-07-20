using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public InformationDisplay infoDisplay;

    private DisplayPanelManager _panelManager;


    private void Start()
    {
        this._panelManager = GetComponent<DisplayPanelManager>();
    }
    public void ResetApp()
    {
        this._panelManager.CloseWindows();
        this.infoDisplay.ResetProcessing();
    }
}
