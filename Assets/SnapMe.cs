using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapMe : MonoBehaviour {
    static Transform otherParentTransform;
    static Vector3 otherPos;
    DragMe dm;
	// Use this for initialization
	void Start () {
        GameObject parent = transform.parent.gameObject;
        dm = parent.GetComponent<DragMe>();
        otherParentTransform = null;
        otherPos = Vector3.zero;
	}
	void OnCollisionEnter()//Collision col)
    {
        Debug.Log(gameObject.name + "part collided!");
        if (dm.IsDragged())
        {
            Debug.Log("This part is being dragged also!");
            //snap parent position P to other collidable - self collidable localposition
            
        } else
        {
            otherParentTransform = transform.parent.transform;
            otherPos = transform.position;
        }
    }
    // Update is called once per frame
    void Update () {
        if (dm.IsDragged())
        {
            if (otherParentTransform != null)
            {
                transform.parent.transform.position = otherPos + transform.localPosition;
                transform.parent.transform.rotation = otherParentTransform.rotation;
                transform.parent.transform.Rotate(otherParentTransform.up * 180);
                dm.DetachFromDrag();
                otherParentTransform = null;
                otherPos = Vector3.zero;
            }
        }
	}
}
