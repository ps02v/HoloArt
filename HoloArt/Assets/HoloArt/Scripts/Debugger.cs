using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public bool isActive = true;
    public TMP_Text debugWindow;

    private void Start()
    {
        debugWindow.gameObject.SetActive(this.isActive);
    }

    public void Print(string msg)
    {
        if (this.isActive)
        {
            this.debugWindow.text = msg;
        }
    }
    public void PrintPlus(string msg)
    {
        if (this.isActive)
        {
            string s = this.debugWindow.text;
            s += @"\n" + msg;
            this.debugWindow.text = s;
        }
    }
}
