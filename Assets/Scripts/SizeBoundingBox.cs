using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SizeBoundingBox : MonoBehaviour
{
    public bool createBox = false;
    private bool boxCreated = false;
    void Update()
    {  
        if(createBox && !boxCreated){
            //CalculateLocalBounds();
            CreateBox(transform);
            boxCreated = true;
        }
        
    }
    public static Bounds CreateBox (Transform t) {
        BoxCollider box = t.gameObject.AddComponent<BoxCollider>();

        var renderers = t.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(renderers.FirstOrDefault().transform.position, Vector3.zero);
        foreach( Renderer child in renderers ) {
            bounds.Encapsulate(child.bounds.min );
            bounds.Encapsulate(child.bounds.max );
        }
        
        box.center = Vector3.Scale((bounds.center - t.position), new Vector3(1/t.localScale.x,1/t.localScale.y,1/t.localScale.z));;
        box.size = bounds.size; //Vector3.Scale(bounds.size, new Vector3(1/t.localScale.x,1/t.localScale.y,1/t.localScale.z));

        return box.bounds;
    }
}
