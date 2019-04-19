using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriLib;
using System;
using System.Linq;

public class LoadModel : MonoBehaviour
{
    public const float maxSize=20;
    public const float minSize=2;
    public static GameObject load (String assetLocation) {
        GameObject g = null;
        using (var assetLoader = new AssetLoader())
        {
            try
            {
                var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
                assetLoaderOptions.RotationAngles = Vector3.zero;
                GameObject loadedGameObject = assetLoader.LoadFromFile(@"./" + assetLocation, assetLoaderOptions, null, null);

                g = loadedGameObject;

                // First lets find the size of the model
                SizeBoundingBox.CreateBox(g.transform);
                var box = g.GetComponent<BoxCollider>();
                if(Vector3.Scale(box.size,g.transform.localScale).x > maxSize) g.transform.localScale *=  maxSize/Vector3.Scale(box.size,g.transform.localScale).x;
                if(Vector3.Scale(box.size,g.transform.localScale).y > maxSize) g.transform.localScale *= maxSize/Vector3.Scale(box.size,g.transform.localScale).y;
                if(Vector3.Scale(box.size,g.transform.localScale).z > maxSize) g.transform.localScale *= maxSize/Vector3.Scale(box.size,g.transform.localScale).z;
                else {
                    if(Vector3.Scale(box.size,g.transform.localScale).x < minSize) g.transform.localScale *= minSize/Vector3.Scale(box.size,g.transform.localScale).x;
                    if(Vector3.Scale(box.size,g.transform.localScale).y < minSize) g.transform.localScale *= minSize/Vector3.Scale(box.size,g.transform.localScale).y;
                    if(Vector3.Scale(box.size,g.transform.localScale).z < minSize) g.transform.localScale *= minSize/Vector3.Scale(box.size,g.transform.localScale).z;
                }
                g.transform.Rotate(90,0,0);
                
                var rs = g.transform.GetComponentsInChildren<Renderer>();
                foreach(Renderer r in rs) {
                    if(r?.materials?.Any() == true)
                        foreach(Material mat in r.materials)
                            mat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
                }
                box.enabled = false;
                g.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }   
        return g;   
    }
}
