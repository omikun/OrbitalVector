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

        bool buttonUp = Input.GetButtonUp("Fire1");
        bool buttonEvent = Input.GetButton("Fire1");
        bool buttonDown = Input.GetButtonDown("Fire1");

        if (Input.GetButtonDown("Fire1")) lastMousePos = Input.mousePosition;
        if (buttonEvent) rotateWorldMouse();

        if (hit)
        {
            var hitObj = hitInfo.transform.gameObject;
            var uiButton = hitObj.GetComponent<UIButton>();
            var uiDrag = hitObj.GetComponent<UIDraggable>();
			var intShip = hitObj.GetComponent<InteractableShip>();
			var intPlot = hitObj.GetComponent<InteractablePlot> ();
            
			if (uiButton != null && true)
				uiButton.Hover();
			
			if (uiButton != null && buttonUp) 
				uiButton.Clicked();
			
			if (uiDrag != null && buttonEvent) 
				uiDrag.Drag();
			
			if (intShip != null && buttonDown) 
				intShip.StartUsing(hitInfo.transform.gameObject);
			
			if (intPlot != null && buttonEvent) 
				intPlot.StartMouseUsing(hitInfo);

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
