using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPhotoBehaviour : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public string SaveCameraView(string name)
    {
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        cam.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        cam.Render();

        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;

        byte[] byteArray = renderedTexture.EncodeToPNG();
        string d = Application.dataPath + "/races/" + name + "-" + DateTime.Now.ToString("yyMMdd_HHmm");
        System.IO.Directory.CreateDirectory(d);
        System.IO.File.WriteAllBytes(d + "/map.png", byteArray);
        return d;
    }
}
