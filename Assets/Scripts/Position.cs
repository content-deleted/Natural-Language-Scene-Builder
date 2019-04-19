using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    public static void PlaceObject(string id, Vector3 Start, Vector3 Direction, int count)
    {
        var startLocation = Start;
        var direction = Direction;

        for( int i = 0; i < count; i++) {
            var g = GameObject.Instantiate(InputParse.sceneObject.namedModels[id]);
            g.SetActive(true);
            var box = g.GetComponent<BoxCollider>();
            box.enabled = true;
            // Raycast in the opposite direction to place it at nearest spot from that direction
            var maxDistance = 1000f;
            var origin = startLocation;
            Debug.Log(direction);
            if( direction.y == 0){
                var dir = -direction.normalized;

                var radius = Vector3.Distance(Vector3.Scale(box.center+box.size/2,g.transform.localScale), Vector3.Scale(box.center-box.size,g.transform.localScale));
                RaycastHit hit;
                box.enabled = false;

                if (Physics.SphereCast(origin + maxDistance * direction.normalized, radius, dir, out hit, maxDistance,  ~(1 << 2)))
                {
                    Debug.Log("Adjusting position based on bounding mass");
                    Debug.Log(hit.collider.name);
                    g.transform.position = hit.point + (radius/2 + 2f*(UnityEngine.Random.value+UnityEngine.Random.value)) * direction.normalized; 
                }
                else {  
                    g.transform.position = origin + (radius/2 + UnityEngine.Random.value) * direction.normalized; 
                }
                box.enabled = true;

                //set the previous position for relative placing
                InputParse.previousPos = g.transform.position;
            
                // Correct height
                var p = g.transform.position;
                p.y = (box.size.z/2 + box.center.z) * g.transform.localScale.z;
                g.transform.position = p;
            }
            else
            {
                Debug.Log("Placing down");
            }

            startLocation = g.transform.position;
            direction = new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1);
        }
    }
}
