using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DisplayPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text artistUIText;
    public TMP_Text timeUIText;
    public TMP_Text paintingUIText;
    public RawImage paintingUIImage;
    public string moreInfoLink;
    public string depiction;
    public Texture2D thumbnailDownloadErrorImage;

    internal int _thumbnailImageWidth = 370;
    internal int _thumbnailImageHeight = 400;

    private bool _downloadedThumbnail = false;

    private void Update()
    {

        if (this._downloadedThumbnail == false)
        {
            this._downloadedThumbnail = true;
            StartCoroutine(DownloadThumbnailImage());
        }

    }

    IEnumerator DownloadThumbnailImage()
    {
        UnityWebRequest wr = new UnityWebRequest(this.depiction);
        //Debug.Log(this.depiction);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (!(wr.isNetworkError || wr.isHttpError))
        {
            //the texture was downloaded, so get a reference to the texture
            Texture2D t = texDl.texture;
            //we want to resize the texture
            int width = t.width;
            int height = t.height;
            float aspectRatio = (float)width / (float)height;
            int newImageWidth, newImageHeight;

            //calculate the new height of the image while preserving the aspect ratio
            //the image width is fixed by the size of the RawImage UI component in the display
            if (width >= height)
            {
                //if width is great than height, then fix the width at 370 pixels
                newImageWidth = this._thumbnailImageWidth;
                newImageHeight = Mathf.RoundToInt(newImageWidth / aspectRatio);
            }
            else
            {
                //else the height is greater than the width, so fix the height at 400 pixels
                newImageHeight = this._thumbnailImageHeight;
                newImageWidth = Mathf.RoundToInt(newImageHeight * aspectRatio);
            }

            //now we need to rescale the texture
            TextureScale.Bilinear(t, newImageWidth, newImageHeight);
            //now assign the texture to the UI
            this.paintingUIImage.texture = t;
        }
        else
        {
            //something went wrong
            this.paintingUIImage.texture = this.thumbnailDownloadErrorImage;
        }
    }

    public void LaunchURL()
    {
        Application.OpenURL(this.moreInfoLink);
    }

}
