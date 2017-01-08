using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragMe : MonoBehaviour {
    Vector3 screenPoint;
    Vector3 offset;
    bool amDragged;
    bool dragMe = true;
    Transform[] connectors = new Transform[4]; //TODO adjustable number of connectors?
    //each connector needs to know if it is connected to something or not
	void Start () {
        connectors[0] = transform.FindChild("conn0");
        connectors[1] = transform.FindChild("conn1");
        connectors[2] = transform.FindChild("conn2");
        connectors[3] = transform.FindChild("conn3");
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
        } 
        amDragged = true;
        SnapToMouseRay();
    }
    void SnapToMouseRay() {
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
                Debug.Log(i + ": " + hit.transform.gameObject.tag);
                if (hit.distance < minDist && 
                    hit.transform.gameObject.tag == "connector")
                {
                    closestHit = hit;
                    minDist = hit.distance;
                }
            }
            //Debug.Log("raycast hit " + i + ": " + hit.transform.gameObject.name);
            if (closestHit.transform.gameObject.tag == "connector")
            {
                transform.rotation = closestHit.transform.parent.rotation;
                transform.Rotate(Vector3.up * 180, Space.Self);
                //transform.Rotate(hit.transform.parent.up * 180);//not quite right
            transform.position =  transform.rotation * - connectors[0].localPosition + closestHit.transform.position;
            }
        }
    }
    void OnMouseUp()
    {
        amDragged = false;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
