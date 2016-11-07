using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour {
    Vector3 lastMousePos;
    GameObject root;
    float totalAngle = 0, totalAnglex = 0;
	// Use this for initialization
	void Start () {
        root = GameObject.Find("HoloRoot");
	}
	void rotateWorldMouse()
    {
        float angle =  -.5f * (Input.mousePosition.x - lastMousePos.x);
        float xangle = .5f * (Input.mousePosition.y - lastMousePos.y);
        lastMousePos = Input.mousePosition;
        totalAngle += angle;
        if (totalAnglex + xangle > 80 || totalAnglex + xangle < -80)
            return;
        totalAnglex += xangle;


        root.transform.Rotate(0, angle, 0, Space.Self);
        root.transform.Rotate(xangle, 0, 0, Space.World);
    }
    
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Fire1"))
        {
            if (Input.GetButtonDown("Fire1"))
                lastMousePos = Input.mousePosition;

            rotateWorldMouse();
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray))
        }
	}
}
