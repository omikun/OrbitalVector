using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

public class OR_Controller : MonoBehaviour
{
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
    List<Vector3> worldObjectPositions = new List<Vector3>();
    List<Quaternion> worldObjectRotations = new List<Quaternion>();


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
    }

    private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "released", e);
        gripsPressed--;
        afterGrabs = false;
    }

    // Use this for initialization
    void Start()
    {
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
            int i = 0;
            foreach (var obj in worldObjects)
            {
                //worldObjectScales.Add(Vector3.zero);
                //worldObjectPositions.Add(Vector3.zero);
                worldObjectScales.Add(obj.transform.localScale);
                worldObjectPositions.Add(obj.transform.position);
                worldObjectRotations.Add(obj.transform.rotation);
                i++;
            }
        }
    }

    public static float scale = 1;
    float baseScale;
    // Update is called once per frame
    void Update()
    {
        if (line.enabled && odata != null)
        {
            line.SetPosition(0, originPos);

            //FIXME using newpos with locked y to get around wonky inc/lan in rv2oe()
            var newpos = transform.position;
            //newpos.y = originPos.y;
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
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    worldObjectScales[i] = worldObjects[i].transform.localScale;
                    worldObjectPositions[i] = worldObjects[i].transform.position;
                    worldObjectRotations[i] = worldObjects[i].transform.rotation;
                }
                baseScale = OrbitData.scale;
            }
            else
            {
                //Vector3 newVector = controllers[0].transform.position - controllers[1].transform.position;
                Vector3 newVector = transform.position - leftController.transform.position;
                //calculate initial vector, magnitude as baseline
                scale = newVector.magnitude / baselineVector.magnitude;
                //var currentScale = scale * newVector.magnitude / baselineVector.magnitude;
                //Quaternion rot = Quaternion.LookRotation(newVector-baselineVector);
                float costheta = Vector3.Dot(newVector, baselineVector);
                float angle = Mathf.Acos(costheta);
                Quaternion rot = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);
                //subseqpent vector/magnitudes are rotation/scales,
                //center of two controllers control translation
                //iterate over all 
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    worldObjects[i].transform.localScale = scale * worldObjectScales[i];// new Vector3(.05f, .05f, .05f);
                    worldObjects[i].transform.rotation = rot * worldObjectRotations[i];
                    //worldObjects[i].transform.position = scale * worldObjectPositions[i];// new Vector3(.05f, .05f, .05f);
                }
                OrbitData.scale = baseScale * scale;

            }
        }
        if (false)//else
        {
            for (int i = 0; i < worldObjects.Count; i++)
            {
                //scale doesn't change, so no need to keep updating those
                //worldObjects[i].transform.localScale = scale * worldObjectScales[i];// new Vector3(.05f, .05f, .05f);
                worldObjects[i].transform.position = scale * worldObjectPositions[i];// new Vector3(.05f, .05f, .05f);
            }
        }
    }
}
