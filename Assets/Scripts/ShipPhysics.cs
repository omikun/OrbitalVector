using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;		// Be sure to include this if you want an object to have Xbox input
//public class Controller
public class ShipPhysics : MonoBehaviour 
{
	public XboxController controller;
    public void QueryControllers()
    {
        int queriedNumberOfCtrlrs = XCI.GetNumPluggedCtrlrs();

        if (queriedNumberOfCtrlrs == 1)
        {
            Debug.Log("Only " + queriedNumberOfCtrlrs + " Xbox controller plugged in.");
        }
        else if (queriedNumberOfCtrlrs == 0)
        {
            Debug.Log("No Xbox controllers plugged in!");
        }
        else
        {
            Debug.Log(queriedNumberOfCtrlrs + " Xbox controllers plugged in.");
        }

        XCI.DEBUG_LogControllerNames();

        // This code only works on Windows
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("Windows Only:: Any Controller Plugged in: " + XCI.IsPluggedIn(XboxController.Any).ToString());

            Debug.Log("Windows Only:: Controller 1 Plugged in: " + XCI.IsPluggedIn(XboxController.First).ToString());
            Debug.Log("Windows Only:: Controller 2 Plugged in: " + XCI.IsPluggedIn(XboxController.Second).ToString());
            Debug.Log("Windows Only:: Controller 3 Plugged in: " + XCI.IsPluggedIn(XboxController.Third).ToString());
            Debug.Log("Windows Only:: Controller 4 Plugged in: " + XCI.IsPluggedIn(XboxController.Fourth).ToString());
        }
    }
    bool lastPrint = false;
    public void GetController()
    {
        //RightStick
        if (XCI.GetButtonDown(XboxButton.RightStick, controller))
        {
            //TODO call rightstick delegate, which can be remapped to func
            nullSpin = true;
            NullSpin();
        }
        if (XCI.GetButtonDown(XboxButton.A, controller))
        {
            //fire!
            Fire();
        }
        if (XCI.GetButtonDown(XboxButton.B, controller))
        {
        }
        if (XCI.GetButtonDown(XboxButton.X, controller))
        {
            FireMissile();
        }
        if (XCI.GetButtonDown(XboxButton.Y, controller))
        {
        }

        // Left stick movement
		float axisY = XCI.GetAxis(XboxAxis.LeftStickY, controller);
        //if (axisY != 0) Debug.Log("axisY: " + axisY);
        float axisX = -XCI.GetAxis(XboxAxis.LeftStickX, controller);
        float rotScalar = 0.2f;
        float pitchAmt = Mathf.Sign(axisY) * Mathf.Sqrt(Mathf.Abs(axisY)) * rotScalar;
        float rollAmt = Mathf.Sign(axisX) * Mathf.Sqrt(Mathf.Abs(axisX)) * rotScalar;
        //actual camera+ship rotation
        PitchUp(-pitchAmt, true);
        RollLeft(-rollAmt, true);
        float neg = 1;
        //just ship rotation, for show
        var rot = Quaternion.Euler(pitchAmt*20, 0, 0) * Quaternion.Euler(0, 0, rollAmt*20);
        ship.transform.localRotation = rot;
    }
//}
//public class ShipPhysics : MonoBehaviour 
//{
    Rigidbody rb;
    public bool nullSpin = false;
    public float Torque = 10;
    public float engineScalar = 6;
    public KeyCode kLeft = KeyCode.A;
    public KeyCode kRight = KeyCode.D;
    public KeyCode kUp = KeyCode.W;
    public KeyCode kDown = KeyCode.S;

    public float SlowDown = 1;
    public GameObject ship;
    public GameObject lEngine, rEngine;
    Transform leftEngine, rightEngine;
    public GameObject beam, gun;
    public GameObject missile;
    public GameObject target;
    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        leftEngine = transform.Find("engine_left");
        rightEngine = transform.Find("engine_right");
        if (CheckNull())
        {
            leftEngine = lEngine.transform;
            rightEngine = rEngine.transform;
            CheckNull();
        }

        QueryControllers();
    }
    void FireMissile()
    {
        var newMissile = Instantiate(missile);
        //set rot and pos to gun
        newMissile.SetActive(true);
        newMissile.transform.position = gun.transform.position;
        newMissile.transform.rotation = gun.transform.rotation;
        //give speed
        var speed = gun.transform.forward * 20;
        newMissile.GetComponent<Rigidbody>().velocity = speed;
        //sound!
        //set die time
        var missileLogic = newMissile.GetComponent<MissileLogic>();
        missileLogic.BornTime = Time.time;
        missileLogic.DieAfterTime = 30;
        missileLogic.enabled = true;
        missileLogic.target = target;
    }
    void Fire()
    {
        //take a copy of the beam, 
        var newBeam = Instantiate(beam);
        //set rot and pos to gun
        newBeam.SetActive(true);
        newBeam.transform.position = gun.transform.position;
        newBeam.transform.rotation = gun.transform.rotation;
        //give speed
        var speed = gun.transform.forward * 100;
        newBeam.GetComponent<Rigidbody>().velocity = speed;
        Debug.Log("beam velocity: " + speed.magnitude);
        //sound!
        //set die time
        var beamLogic = newBeam.GetComponent<BeamLogic>();
        beamLogic.BornTime = Time.time;
        beamLogic.DieAfterTime = 5;
        beamLogic.enabled = true;
    }
    bool CheckNull()
    { 
        if (leftEngine == null
            || rightEngine == null)
        {
            Debug.Log("Engine(s) not found!");
            return false;
        }
        return true;
	}
    void PitchUp(float torque, bool up = true)
    {
        float neg = (up) ? -1 : 1;
        var amt = neg * torque;
        rb.AddTorque(transform.right * amt);

        //var rot = Quaternion.Euler(-transform.right * neg * torque*200);
        //Debug.Log("pitching " + amt);
        var rot = Quaternion.Euler(amt*20, 0, 0);
        //ship.transform.Rotate(transform.forward, -amt);
        ship.transform.localRotation = rot;
    }
    //spins engine to rotate by 1 engine rotation
    void PitchUpWithEngineSpin(float amt)
    {
        //TODO rewrite to rotate engine by 1 rotation only
        leftEngine.Rotate(transform.right, engineScalar * amt);
        rightEngine.Rotate(transform.right, engineScalar * amt);
        transform.Rotate(transform.right, -amt);
        rb.angularVelocity = Vector3.zero;
    }
    void RollLeft( float torque, bool left = true)
    {
        float neg = (left) ? -1 : 1;
        var amt = neg * torque;
        rb.AddTorque(transform.forward * amt);
        //var rot = Quaternion.Euler(-transform.right * neg * torque*200);
        //Debug.Log("rolling " + amt);
        var rot = Quaternion.Euler(0, 0, amt*20);
        //ship.transform.Rotate(transform.forward, -amt);
        ship.transform.localRotation = rot;
        /*
        leftEngine.Rotate(transform.right, neg * engineScalar * amt);
        rightEngine.Rotate(transform.right, -neg * engineScalar * amt);
        transform.Rotate(transform.forward, -amt);
        rb.angularVelocity = Vector3.zero;
        */
    }
    //use rotational inertia of engines to rotate
    void GetKeyboard2()
    {
        if (Input.GetKey(kLeft)) RollLeft(Torque);
        else if (Input.GetKey(kRight)) RollLeft(Torque, false);
        else if (Input.GetKey(kUp)) PitchUp(Torque, false);
        else if (Input.GetKey(kDown)) PitchUp(Torque, true);
    }
    //use thrusters to rotate
    void GetKeyboard()
    {
        if (Input.GetKey(kLeft)) rb.AddTorque(transform.forward * Torque);
        else if (Input.GetKey(kRight)) rb.AddTorque(-transform.forward * Torque);
        else if (Input.GetKey(kUp)) PitchUp(Torque, true);
        else if (Input.GetKey(kDown)) rb.AddTorque(-transform.right * Torque);
    }
    void NullSpin()
    {
        if (nullSpin)
        {
            if (rb.angularVelocity.magnitude > SlowDown * Time.deltaTime)
            {
                rb.AddTorque(-rb.angularVelocity.normalized * SlowDown, ForceMode.Acceleration);
            } else
            {
                nullSpin = false;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
	// Update is called once per frame
	void Update () {
        GetKeyboard();
        GetController();
        NullSpin();
        /*
        Spin(ref SpinLeft, transform.up);
        Spin(ref SpinRight, -transform.up);
        Spin(ref SpinUp, transform.right);
        Spin(ref SpinDown, -transform.right);
        */
    }
}
