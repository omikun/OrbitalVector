using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
            cameraRadar.SelectNextTarget();
        }
        if (XCI.GetButtonDown(XboxButton.B, controller))
        {
            Fire();
        }
        if (XCI.GetButtonDown(XboxButton.X, controller))
        {
            FireMissileFlag = true;
        }
        if (XCI.GetButtonDown(XboxButton.Y, controller))
        {
        }

        {
            // Left stick movement
            float axisY = XCI.GetAxis(XboxAxis.LeftStickY, controller);
            //if (axisY != 0) Debug.Log("axisY: " + axisY);
            float axisX = -XCI.GetAxis(XboxAxis.LeftStickX, controller);
            //if (axisX != 0) Debug.Log("axisX: " + axisX);
            float rotScalar = Torque;
            float pitchAmt = Mathf.Sign(axisY) * Mathf.Sqrt(Mathf.Abs(axisY)) * rotScalar;
            float rollAmt = Mathf.Sign(axisX) * Mathf.Sqrt(Mathf.Abs(axisX)) * rotScalar;
            //actual camera+ship rotation
            PitchUp(pitchAmt, false);
            RollLeft(rollAmt, false);
            float neg = 1;
            //just ship rotation, for show
            var rot = Quaternion.Euler(pitchAmt * 20, 0, 0) * Quaternion.Euler(0, 0, rollAmt * 20);
            _ship.transform.localRotation = rot;
        }
        {
            // Right stick movement
            float axisY = XCI.GetAxis(XboxAxis.RightStickY, controller);
            //if (axisY != 0) Debug.Log("axisY: " + axisY);
            float axisX = -XCI.GetAxis(XboxAxis.RightStickX, controller);
            //if (axisX != 0) Debug.Log("axisX: " + axisX);
            //rotate from default pos by up to 180 degrees on each axis
            float maxRotDegrees = 160;
            float pitch = 0;
            if (axisY > 0)
            {
                pitch = Mathf.Lerp(0, 80f - localCameraAngleX, axisY);
            } else if (axisY < 0)
            {
                pitch = Mathf.Lerp(0, -80 - localCameraAngleX, -axisY);
            }
            //var pitch = Mathf.Min(90f * axisY, 90f - localCameraAngleX);
            if (axisY != 0) Debug.Log("pitch: " + pitch);
            _camera.transform.localPosition = Quaternion.Euler(pitch, maxRotDegrees * axisX, 0) * localCameraPosition;
            _camera.transform.localRotation = Quaternion.LookRotation(-_camera.transform.localPosition + new Vector3(0, 1, 0));
        }

    }
    Vector3 localCameraPosition; //at start of game; default local pos
    float localCameraAngleX;
    //}
    //public class ShipPhysics : MonoBehaviour 
    //{
    Rigidbody rb;
    public GameObject _camera;
    TargetRadar cameraRadar;
    public bool nullSpin = false;
    public bool FireMissileFlag = false;
    public float Torque = 10;
    public float engineScalar = 6;
    public KeyCode kLeft = KeyCode.A;
    public KeyCode kRight = KeyCode.D;
    public KeyCode kUp = KeyCode.W;
    public KeyCode kDown = KeyCode.S;

    public float SlowDown = 1;
    public GameObject _ship;
    public GameObject lEngine, rEngine;
    Transform leftEngine, rightEngine;
    public GameObject _beam, _gun;
    public GameObject _missile;
    public GameObject _target;
    public GameObject _debugCameraLineObj;
    LineRenderer _debugCameraLine;
    public GameObject _debugMarker;
    public GameObject _missileCount;
    Text missileText;
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
        _debugCameraLine = _debugCameraLineObj.GetComponent<LineRenderer>();
        QueryControllers();
        missileFFSM = new WeaponFiringFSM(FireMissile);
        missileText = _missileCount.GetComponent<Text>();

        localCameraPosition = _camera.transform.localPosition;
        localCameraAngleX = 10.722f;//hack _camera.transform.localRotation.x;
        Debug.Log("initial angle: " + localCameraAngleX);

        cameraRadar = _camera.GetComponent<TargetRadar>();
    }
    GameObject CameraFollowMissile;
    float b;
    void CameraFollow(GameObject missile)
    {
        return;
        Debug.Log("Following!");
        _camera.transform.parent = null;//
        //_debugMarker.transform.parent = missile.transform;
        CameraFollowMissile = missile;
        b = (missile.transform.position - _camera.transform.position).magnitude;
        CameraFollow();
    }
    float QuadraticSolver(float A, float B, float C)
    {
        float a = float.NaN;
        var determinant = B * B - 4 * A * C;
        var sqrtD = Mathf.Sqrt(determinant);
        if (determinant < 0)
        {
            Debug.Log("complex conjugates, give up camera follow for now");
        } else if (determinant == 0)
        {
            //only 1 root
            a = (-1 * B + sqrtD) / 2;
        } else
        {
            //2 roots, find larger root
            var sol1 = (-1 * B + sqrtD) / (2 * A);
            var sol2 = (-1 * B - sqrtD) / (2 * A);
            a = Mathf.Max(sol1, sol2);
        }
        return a;
    }
    void CheckValidTriangle(float a, float b, float c)
    {
        //Debug.Log("triangle sides a: " + a + " b: " + b + " c: " + c);
        var largest = Mathf.Max(a, Mathf.Max(b, c));
        if (largest > largest - a + b + c)
        {
            //Debug.Log("invalid triangle");
            return;
        }
    }
    //camera chases the missile while keeping the target in view
    //This function finds the camera position and angle that fits the following criteria:
    // - Always some fixed distance from the missile
    // - Has the missile and target within a specific field of view (FOV)
    //camera, missile, target forms a triangle; camera2missile has a fixed distance, but we don't know the direction; we must angle it correctly such that the missile and target is within fov degrees from camera's perspective, so we know the interior angle from the camera corner, but we need to find the angle from the missile corner. All we have is the vector from from missile2target.
    //but if we treat the this as an abstract triangle problem, we know the distance of two sides (missile2target and camera2missile) and we know one angle (from camera corner)
    //So by using generalized pythagora's thm:
    //c^2 = a^2 + b^2 - 2abcos(fov) 
    //where a is camera2target, b is camera2missile, and c is missile2target
    //We know b, c and fov, so we just solve for a
    //then we can solve for the angle, phi, in the missile corner
    //and since we know the missile2target vector, we can start there and rotate by angle phi and get the desired camera position
    //but the camera needs to be pointing the right way. Since we know the missile and target are fov degrees apart (a given), we just need to rotate the camera fov/2 degrees in the correct direction.
    //all of this assumes a 2D environment. Since we are working with a 3 points, the problem is natively a 2D problem as all three are always on some plane. We just need to rotate along the right axis (orthogonal to the plane). We can get that axis by the cross product of any two vectors along that plane
    public GameObject cameraDummy;
    void CameraFollow()
    {
        //TODO reuse somewhere else, on hold A?
        if (CameraFollowMissile == null || !CameraFollowMissile.activeSelf
            || _target == null || !_target.activeSelf)
        {
            _camera.transform.position = cameraDummy.transform.position;
            _camera.transform.rotation = cameraDummy.transform.rotation;
            return;
        }
        var missile = CameraFollowMissile;
        float fov = 20; //fov in degrees
        var camera2missile = (missile.transform.position - _camera.transform.position);
        var missile2target = (_target.transform.position - missile.transform.position);
        float a = 0; //camera2target need to find this first
        //float b = camera2missile.magnitude; this is a global
        float c = missile2target.magnitude;

        //for debug visualization
        //_debugCameraLine.SetPosition(0, _target.transform.position);
        //_debugCameraLine.SetPosition(1, missile.transform.position);
        //_debugCameraLine.SetPosition(2, _camera.transform.position);

        //find a, camera2target
        //using c^2 = a^2 + b^2 - 2abcos(fov); find a
        var B = -2 * b * Mathf.Cos(Mathf.Deg2Rad * fov);
        var C = b * b - c * c;
        a = QuadraticSolver(1, B, C);
        CheckValidTriangle(a, b, c);

        //now we know all sides of the triangle, time to find angle between target and camera relative to missile using pythagorean thm
        var phi = Mathf.Acos((b * b + c * c - a * a) / (2 * b * c)) * Mathf.Rad2Deg;
        //Debug.Log("phi: " + phi );

        //get axis orthogonal to a,b,c
        var orthAxis = Vector3.Cross(camera2missile, missile2target).normalized;
        //_debugCameraLine.SetPosition(3, orthAxis*5 + _camera.transform.position);
        //_debugCameraLine.SetPosition(4, _camera.transform.position);
        //move camera to the right place by starting vector missile to target
        var newCameraPos = (missile2target.normalized) * b;//camera2missile dist
        //then rotate by phi around axis
        newCameraPos = Quaternion.AngleAxis(phi, orthAxis) * newCameraPos + missile.transform.position;
        //_debugMarker.transform.position = missile2target + missile.transform.position;
        if (float.IsNaN(newCameraPos.x))
            return;
        _camera.transform.position = newCameraPos;
        //var newCamera2Target = _target.transform.position - _camera.transform.position;
        //var newCamera2Missile = missile.transform.position - _camera.transform.position;
        //Debug.Log("fov from phi: " + Vector3.Angle(newCamera2Missile, newCamera2Target));

        //turn towards middle between missile and target
        //start w/ dir from camera to misisle
        var lookAtMissile = Quaternion.LookRotation( missile.transform.position - _camera.transform.position);
        //turn back fov/2
        _camera.transform.rotation = Quaternion.AngleAxis(fov / 2, orthAxis) * lookAtMissile;
        //_debugCameraLine.SetPosition(5, _camera.transform.forward*30 + _camera.transform.position);
    }
    Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }
    public WeaponFiringFSM missileFFSM;
    int numMissilesAtOnce = 6;
    int numMissilesFiredThisVolley = 0;
    float delayBetweenMissileFire = .2f;
    float delayBetweenVolley = 5f;
    float timeSinceLastVolley = 0f;
    float timeSinceLastMissileFire = 0f;
    [ContextMenu("Fire Missile!")]
    public GameObject FireMissile()
    {
#if false
        if (FireMissileFlag)
        {
            if (numMissilesFiredThisVolley < numMissilesAtOnce)
            {
                if (numMissilesFiredThisVolley == 0 )
                {
                    if (Time.time < timeSinceLastVolley + delayBetweenVolley)
                        return null;
                    timeSinceLastVolley = Time.time;
                }
                if (Time.time > timeSinceLastMissileFire + delayBetweenMissileFire)
                {
                    timeSinceLastMissileFire = Time.time;
                    numMissilesFiredThisVolley++;
                }
                else
                {
                    return null;
                }
            } else
            {
                FireMissileFlag = false;
                numMissilesFiredThisVolley = 0;
                return null;
            }
        } else
        {
            return null;
        }
#endif
        Debug.Log("Fire missile");
        var newMissile = Instantiate(_missile);
        //set rot and pos to gun
        newMissile.SetActive(true);
        newMissile.transform.position = _gun.transform.position;
        newMissile.transform.rotation = _gun.transform.rotation;
        //give speed
        var upGain = Random.Range(-1.0f, 1f);
        var rightGain = Random.Range(-1f, 1f);
        var forwardGain = Random.Range(1f, 3f);
        var velocity = _gun.transform.forward * forwardGain
                    + _gun.transform.up * upGain
                    + _gun.transform.right * rightGain;
        newMissile.GetComponent<Rigidbody>().velocity = velocity.normalized * 3;
        //sound!
        //set die time
        var missileLogic = newMissile.GetComponent<MissileLogic>();
        missileLogic.BornTime = Time.time;
        missileLogic.DieAfterTime = 30;
        missileLogic.enabled = true;
        missileLogic.target = UXStateManager.GetTarget();
        CameraFollow(newMissile);
        return newMissile;
    }
    
    void Fire()
    {
        //take a copy of the beam, 
        var newBeam = Instantiate(_beam);
        //set rot and pos to gun
        newBeam.SetActive(true);
        newBeam.transform.position = _gun.transform.position;
        newBeam.transform.rotation = _gun.transform.rotation;
        //give speed
        var speed = _gun.transform.forward * 100;
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
        //if (amt != 0) Debug.Log("pitching " + amt);
        var rot = Quaternion.Euler(amt, 0, 0);
        //ship.transform.Rotate(transform.forward, -amt);
        _ship.transform.localRotation = rot;
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
        //if (amt != 0) Debug.Log("rolling " + amt);
        var rot = Quaternion.Euler(0, 0, amt);
        //ship.transform.Rotate(transform.forward, -amt);
        _ship.transform.localRotation = rot;
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
	void FixedUpdate () {
        GetKeyboard();
        GetController();
        NullSpin();
        //CameraFollow();//HACK put me back in somehow; conflicts with camera rotate part
        missileFFSM.Tick(ref FireMissileFlag);
        missileText.text = "Missiles: " + missileFFSM.Ammo;
        /*
        Spin(ref SpinLeft, transform.up);
        Spin(ref SpinRight, -transform.up);
        Spin(ref SpinUp, transform.right);
        Spin(ref SpinDown, -transform.right);
        */
    }
}
