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
    
    void uibClicked(UIButton uib)
    {
        if (uib == null) return;
        Debug.Log("mouse click up");
        uib.Clicked();
    }
    void uibHover(UIButton uib)
    {
        if (uib == null) return;
        //Debug.Log("mouse hover");
        //uib.StartUsing(hitInfo.transform.gameObject);
        uib.Hover();
    }
    void uibDrag(UIButton uib)
    {
        if (uib == null) return;
        //Debug.Log("mouse drag");
        uib.Drag();
    }
    void ipDrag(InteractablePlot ip, RaycastHit hitInfo)
    {
        if (ip == null) return;
        Debug.Log("mouse plot click");
        ip.StartMouseUsing(hitInfo);
    }
    void isClick(InteractableShip isc, RaycastHit hitInfo)
    {
        if (isc == null) return;
        Debug.Log("ship click");
        isc.StartUsing(hitInfo.transform.gameObject);
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
            //hover state
            uibHover(uibComp);
            //up state
            if (Input.GetButtonUp("Fire1")) uibClicked(uibComp);
            //drag/down state
            if (Input.GetButton("Fire1"))
            {
                //click down state
                if (Input.GetButtonDown("Fire1"))
                {
                    var isComp = hitObj.GetComponent<InteractableShip>();
                    isClick(isComp, hitInfo);
                }
                uibDrag(uibComp);
                var ipComp = hitObj.GetComponent<InteractablePlot> ();
                ipDrag(ipComp, hitInfo);
            }
        }

        if (Input.GetButtonDown("Fire1")) lastMousePos = Input.mousePosition;

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
