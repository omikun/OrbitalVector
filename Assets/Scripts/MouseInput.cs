using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour {
    Vector3 lastMousePos;
    GameObject root;
	GameObject camera;
    float totalAngle = 0, totalAnglex = 0;
	// Use this for initialization
	void Start () {
        root = GameObject.Find("HoloRoot");
        camera = GameObject.Find("Camera");
        if (camera == null)
        {
            Debug.Log("Cannot find camera, disabling mouse input");
            gameObject.SetActive(false);
            return;
        }
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

        var cam = camera.GetComponent<Camera>();
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (hit)
        {
            var hitObj = hitInfo.transform.gameObject;
            var uibComp = hitObj.GetComponent<UIButton>();
            if (uibComp)
                //uibComp.StartUsing(hitInfo.transform.gameObject);
                uibComp.Hover();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            Debug.Log("button up!");
            if (hit)
            {
                var hitObj = hitInfo.transform.gameObject;
                var uibComp = hitObj.GetComponent<UIButton>();
                if (uibComp)
                {
                    Debug.Log("mouse click up");
                    uibComp.Clicked();
                }
            }
        }
        if (Input.GetButton("Fire1"))
        {
            //click down state
			if (Input.GetButtonDown ("Fire1")) {
				lastMousePos = Input.mousePosition;
                if (hit)
                {
                    var hitObj = hitInfo.transform.gameObject;
                    Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                    var isComp = hitObj.GetComponent<InteractableShip>();
                    if (isComp)
                        isComp.StartUsing(hitInfo.transform.gameObject);

                    
                }
            }
            
            //hover state
            if (hit)
            { 
                var hitObj = hitInfo.transform.gameObject;
                var uibComp = hitObj.GetComponent<UIButton>();
                if (uibComp)
                {
                    Debug.Log("mouse click/dragging");
                    uibComp.Drag();
                }
                var ipComp = hitObj.GetComponent<InteractablePlot> ();
				if (ipComp)
					ipComp.StartMouseUsing (hitInfo);
            }

            rotateWorldMouse();
        }
        var deltaScroll = Input.GetAxis ("Mouse ScrollWheel");
		if (deltaScroll != 0)
		{
			var zoom = Mathf.Min(1.3f, Mathf.Max(0.7f, (1 - deltaScroll)));
			if (zoom < 1 && root.transform.localScale.magnitude > .1f)
				root.transform.localScale *= zoom;
			if (zoom > 1 && root.transform.localScale.magnitude < 3f)
				root.transform.localScale *= zoom;
		}
	}
}
