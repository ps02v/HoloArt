using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPanelManager : MonoBehaviour
{
    private List<GameObject> _displayPlanels;
    void Start()
    {
        this._displayPlanels = new List<GameObject>();
    }

    public void AddPanel(GameObject go)
    {
        this._displayPlanels.Add(go);
    }

    public void CloseWindow(GameObject go)
    {
        this._displayPlanels.Remove(go);
        Destroy(go);
    }

    public void CloseWindows()
    {
        foreach (GameObject go in _displayPlanels)
        {
            ; go.SetActive(false);
            Destroy(go);
        }
    }
}
