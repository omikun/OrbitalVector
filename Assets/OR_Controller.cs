using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using MuMech;

public class OR_Controller : MonoBehaviour
{
    public GameObject mov_origin; //obj when trigger first pressed
    Vector3 originPos;
    LineRenderer line;
    Vector3 accelVector;
    public GameObject leftController;
    Vector3 lastPos;
    GameObject root;

    public static int gripsPressed = 0;
    public static bool afterGrabs = false;
    Vector3 baselineVector;
    Vector3[] gripPositions = new Vector3[2];
    List<GameObject> controllers = new List<GameObject>();

    List<GameObject> worldObjects = new List<GameObject>();
    List<Vector3> worldObjectScales = new List<Vector3>();


    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        return;//TODO turn this on for controller debug
        Debug.Log("Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }
    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "pressed", e);
        ;

        if (false)//leftController == null)
        {
            PorkChopPlot.triggerPork = true;
         
            double time = 30d;
            var thisodata = UXStateManager.GetSource().GetComponent<OrbitData>();
            Vector3d r1 = thisodata.getR();

            var oe2 = UXStateManager.GetTarget().GetComponent<OrbitData>().getOE();
            var tra2 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe2, time);
            oe2.tra = tra2;
            Vector3d r2 = OrbitalTools.Util.oe2rd(OrbitData.parentGM, oe2);

            Vector3d initVel, finalVel;
            LambertSolver.Solve(r1, r2, time, OrbitData.parentGM, true, out initVel, out finalVel);

            thisodata.rv[3] = initVel.x;
            thisodata.rv[4] = initVel.y;
            thisodata.rv[5] = initVel.z;
        }
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "released", e);
        mov_origin.SetActive(false);
        line.enabled = false;
        Orbit.accelVector = Vector3.zero;

        Orbit.timeScale = 1;
    }

    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "pressed", e);
        //OR_Controller.gripsPressed++;
        gripsPressed++;
            lastPos = transform.position;
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

        root = GameObject.Find("HoloRoot");
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
                i++;
            }
        }
    }
    //returns degrees
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
    public static float scale = 1;
    public static float totalAngle = 0;
    public static Vector3 totalMoveY;
    float baseScale;
    float lastAngle = 0;
    Vector3 lastMoveY;
    Vector3 lastY;
    // Update is called once per frame
    public static bool useRoot = true;
    void rotateWorld()
    {
        float angle = -AngleSigned(transform.position, lastPos, Vector3.up);
        lastPos = transform.position;
        angle *= 10;
        totalAngle += angle;

        if (!useRoot)
        {
            for (int i = 0; i < worldObjects.Count; i++)
                worldObjects[i].transform.Rotate(0, angle, 0, Space.World);
        }
        else
            root.transform.Rotate(0, angle, 0);
    }

    public void EnableOAccelerate()
    {
        mov_origin.transform.position = transform.position;
        originPos = transform.position;
        mov_origin.SetActive(true);
        line.enabled = true;

        Orbit.timeScale = 0.1f;
    }
    void OAccelerate()
    {
        if (line.enabled && UXStateManager.GetSource() != null)
        {
            line.SetPosition(0, originPos);
            line.SetPosition(1, transform.position);

            Vector3 velVector = transform.position - originPos;

            //velVector is the apparent vector, but we want the real vector before world rotation, so we'll need to rotate back apparent vector to get real vector
            var antiWorldRotation = Quaternion.AngleAxis(-totalAngle, Vector3.up);
            velVector = antiWorldRotation * velVector;
            velVector *= 3f;
            var odata = UXStateManager.GetSource().GetComponent<OrbitData>();
            odata.params_[4] = (double)velVector.x;
            odata.params_[5] = (double)velVector.y;
            odata.params_[6] = (double)velVector.z;
        }
    }
    void simpleMove()
    {
        if (DataStore.simpleMovement && line.enabled)
        {
            //take userSelection and move to destination on trigger release
            //draw line from userSelection.transform.position to transform.position
            line.SetPosition(0, UXStateManager.GetSource().transform.position);
            line.SetPosition(1, transform.position);
        }
    }
    int gripState = 0;
    void Update()
    {
        OAccelerate();
        simpleMove();

        if (gripsPressed == 0)
            gripState = 0;
        else if (gripsPressed == 1 )
        {
            if (gripState == 1)
                rotateWorld();
            if (gripState == 0)
            {
                Debug.Log("To gripState 1");
                gripState = 1;
            }
        } else if (gripsPressed == 2 && leftController != null)
        {
            gripState = 2;
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
                }
                baseScale = OrbitData.scale;
                lastY = (transform.position + leftController.transform.position);
                lastAngle = 0;

            }
            else
            {
                Vector3 newVector = transform.position - leftController.transform.position;
                scale = newVector.magnitude / baselineVector.magnitude;

                var diff = newVector - baselineVector;
                var angle = Vector3.Angle(baselineVector, newVector);
/*
                angle = -AngleSigned(newVector, baselineVector, Vector3.up);
                var tempAngle = angle;
                angle -= lastAngle;
                lastAngle = tempAngle;
                totalAngle += angle;
*/
                var moveY = newVector - baselineVector;
                var y = transform.position + leftController.transform.position;
                moveY = (y - lastY)/2;
                totalMoveY += moveY;
                lastY = y;
                //subseqpent vector/magnitudes are rotation/scales,
                //center of two controllers control translation
                if (!useRoot)
                {
                    for (int i = 0; i < worldObjects.Count; i++)
                    {
                        worldObjects[i].transform.localScale = scale * worldObjectScales[i];
                        worldObjects[i].transform.Translate(moveY, Space.World);
                    }
                    OrbitData.scale = baseScale * scale;
                }
                else
                {
                    root.transform.Translate(moveY);
                }
            }
        }
    }
}
