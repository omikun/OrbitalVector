using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragMe : MonoBehaviour {
    Vector3 screenPoint;
    Vector3 offset;
    bool amDragged;
    bool partIsConnected = false;
    bool dragMe = true;
    Transform[] connectors = new Transform[4]; //TODO adjustable number of connectors?
    GameObject root;
    //each connector needs to know if it is connected to something or not
	void Start () {
        connectors[0] = transform.FindChild("conn0");
        connectors[1] = transform.FindChild("conn1");
        connectors[2] = transform.FindChild("conn2");
        connectors[3] = transform.FindChild("conn3");
        root = GameObject.Find("ShipRoot");
	}
    public bool IsDragged()
    {
        return amDragged;
        //TODO else check if parent is being dragged
    }
    public void DetachFromDrag()
    {
        dragMe = false;
    }
	void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        dragMe = true;
        Debug.Log("Getting clicked down: " + name);
        //TODO get rid of rigidbody in any connector to avoid colliding w/ other stuff
    }
    void OnMouseDrag()
    {
        float distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        var curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen);
        var curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;

        //RaycastAll to see what the next closest object is to the camera,
        //if that object is a connector, snap to it!
        if (dragMe)
        {
            transform.position = curPosition;
            // on drag, self part is always disconnected until snapToMouse connects
        } else {
            return;
        }
        amDragged = true;
        partIsConnected = SnapToMouseRay();
        //if SnapToMouseRay does not connect, then the part is disconnected;
    }
    bool SnapToMouseRay() {
        //get ray from mouse, if it strikes another collidable in another object, move transform.position to otherObject.pos - child.transform.localPosition
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 30);
        RaycastHit closestHit;
        float minDist = float.PositiveInfinity;
        if (hits.Length > 0)
        {
            closestHit = hits[0];
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                //Debug.Log(i + ": " + hit.transform.gameObject.tag);
                if (hit.distance < minDist && 
                    hit.transform.gameObject.tag == "connector")
                {
                    closestHit = hit;
                    minDist = hit.distance;
                }
            }
            //Debug.Log("raycast hit " + i + ": " + hit.transform.gameObject.name);
            //found a connector, move self to it
            if (closestHit.transform.gameObject.tag == "connector"
            )
            //TODO enabling SnapMe check causes collision enter/exit flicker
            //&& closestHit.transform.gameObject.GetComponent<SnapMe>().GetConnection() == null)
            {
                //Debug.Log("Closest hit: " + closestHit.transform.parent.gameObject.name + "." + closestHit.transform.gameObject.name);
                //my rotation is their rotation (parent) + their connector's rotation and my connector's rotation;
                //except I need to invert one of the connector's rotation (undo it) so if they're both the same rotation, it's the same as their parent rotation
                transform.rotation =  
                closestHit.transform.localRotation
                * Quaternion.Inverse(connectors[0].transform.localRotation) 
                * closestHit.transform.parent.rotation;
                //flip it 180 so I'm facing the hit
                transform.Rotate(Vector3.up * 180, Space.Self);
                //same concept for position as rotation
                transform.position =  transform.rotation * - connectors[0].localPosition + closestHit.transform.position;

//move self into children of other connector
                transform.parent = closestHit.transform;
//redundant, task for removal
                //link up connectors
                var conn = connectors[0].GetComponent<SnapMe>();
                var otherConn = closestHit.transform.gameObject.GetComponent<SnapMe>();
                conn.SetConnection(otherConn);
                otherConn.SetConnection(conn);
                amDragged = false;
                return true;
            }
        }
        return false;
    }
    void OnMouseUp()
    {
        amDragged = false;
        if (!partIsConnected)
        {
            transform.parent = root.transform;
        }
        {
            //run update algorithm (actually, could just run the algorithm regardless)
            CheckAndInstallFS(gameObject);
        }
    }
    void CheckAndInstallFS(GameObject obj) {
        if (obj.transform.parent.gameObject == root)
        {
            var fship = obj.GetComponent<FunctionalShip>();
            if (fship == null)
            {
                obj.AddComponent(typeof(FunctionalShip));
                fship = obj.GetComponent<FunctionalShip>();
            }
            fship.UpdateShip();
        }
        else
        {
            CheckAndInstallFS(obj.transform.parent.gameObject);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
