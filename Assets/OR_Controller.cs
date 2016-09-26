using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

public class OR_Controller : MonoBehaviour {
    public GameObject mov_origin; //obj when trigger first pressed
    Vector3 originPos;
    LineRenderer line;
    OrbitData odata;
    Vector3 accelVector;
    public GameObject leftController;

    public static int gripsPressed = 0;
    public static bool afterGrabs = false;
    Vector3 baselineVector;
    Vector3[] gripPositions = new Vector3[2];
    List<GameObject> controllers = new List<GameObject>();

    List<GameObject> worldObjects = new List<GameObject>();
    List<Vector3> worldObjectScales = new List<Vector3>();


    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        Debug.Log("Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }
    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "pressed", e);
        mov_origin.transform.position = transform.position;
        originPos = transform.position;
        mov_origin.SetActive(true);
        line.enabled = true;

        Orbit.timeScale = 0.1f;
        if (DataStore.userSelection != null)
            odata = DataStore.userSelection.GetComponent<OrbitData>();
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "released", e);
        mov_origin.SetActive(false);
        line.enabled = false;
        Orbit.accelVector = Vector3.zero;

        Orbit.timeScale = 1;
        odata = null;
    }
    
    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "pressed", e);
        //OR_Controller.gripsPressed++;
        gripsPressed++;

        Orbit.timeScale = 0.1f;
        if (DataStore.userSelection != null)
            odata = DataStore.userSelection.GetComponent<OrbitData>();
    }

    private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "released", e);
        gripsPressed--;
        afterGrabs = false;
    }

	// Use this for initialization
	void Start () {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_ControllerEvents_ListenerExample is required to be attached to a SteamVR Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        //Setup controller event listeners
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        GetComponent<VRTK_ControllerEvents>().GripPressed += new ControllerInteractionEventHandler(DoGripPressed);
        GetComponent<VRTK_ControllerEvents>().GripReleased += new ControllerInteractionEventHandler(DoGripReleased);

        mov_origin.SetActive(false);
        line = GetComponent<LineRenderer>();
        line.enabled = false;

        //only on right controller (master)
        if (leftController != null)
        {

#if false
        //this doesn't always get both controllers for some reason... initialization order error?
        foreach (var controller in GameObject.FindGameObjectsWithTag("GameController"))
        {
            Debug.Log("found a game controller");
            Debug.Log("game trnasform: " + controller.transform.position);
            controllers.Add(controller);
        }
        Debug.Log("game trnasform: " + controllers[1].transform.position);
#endif
            //populate all objects that will be affected
            worldObjects.Add(GameObject.Find("Earth"));
            worldObjects.Add(GameObject.Find("Moon"));
            worldObjects.Add(GameObject.Find("OrbitManager"));
            foreach (var ship in GameObject.FindGameObjectsWithTag("ship"))
            {
                worldObjects.Add(ship);
            }

            //resize worldObjectScales to match with worldObjects
            foreach (var obj in worldObjects)
            {
                worldObjectScales.Add(Vector3.zero);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (line.enabled && odata != null)
        {
            line.SetPosition(0, originPos);

            //FIXME using newpos with locked y to get around wonky inc/lan in rv2oe()
            var newpos = transform.position;
            newpos.y = originPos.y;
            line.SetPosition(1, newpos);

            Vector3 velVector = newpos - originPos;
            velVector *= 3f;
            odata.params_[4] = (double)velVector.x;
            odata.params_[5] = (double)velVector.y;
            odata.params_[6] = (double)velVector.z;
        }
        if (gripsPressed == 2 && leftController != null)
        {
            if (afterGrabs == false)
            {
                Debug.Log("two grips pressed!");
                afterGrabs = true;
                Debug.Log("controllers found: " + controllers.Count);
                //baselineVector = controllers[0].transform.position - controllers[1].transform.position;
                baselineVector = transform.position - leftController.transform.position;
                currentScale = GameObject.Find("Earth").transform.localScale;
                for (int i=0; i < worldObjects.Count; i++)
                {
                    worldObjectScales[i] = worldObjects[i].transform.localScale;
                }
            }
            else {
                //Vector3 newVector = controllers[0].transform.position - controllers[1].transform.position;
                Vector3 newVector = transform.position - leftController.transform.position;
            //calculate initial vector, magnitude as baseline
                float scale = newVector.magnitude / baselineVector.magnitude;

                //subseqpent vector/magnitudes are rotation/scales,
                //center of two controllers control translation
                //iterate over all 
                for (int i=0; i < worldObjects.Count; i++)
                {
                    worldObjects[i].transform.localScale = scale * worldObjectScales[i];// new Vector3(.05f, .05f, .05f);
                }
            }
        }
    }
    Vector3 currentScale;
}
