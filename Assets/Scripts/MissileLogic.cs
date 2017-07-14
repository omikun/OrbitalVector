using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMinOf
{
    public float index = float.PositiveInfinity;
    public float time = 0;
    public Vector3 result = Vector3.zero;
    public GetMinOf() { }
    public void Update(float i, float t, Vector3 d)
    {
        if (i < index)
        {
            index = i;
            time = t;
            result = d;
        }
    }
    public float GetMinIndex() { return index; }
    public Vector3 GetMin() { return result; }
}
public class MissileLogic : MonoBehaviour {
    public float BornTime = 0;
    public float DieAfterTime = 300;
    public float FireDelay = 1;
    public GameObject target;
    public GameObject rbObj;
    public float Acceleration = 10;
    public GameObject mainCamera;
    Rigidbody rb;

    //PN algorithm
    public float N = 30000;
    public float Nt = 9.8f;
    Vector3 oldRTM = Vector3.zero;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
    //don't use this, confusing as all hell
    //http://www.moddb.com/members/blahdy/blogs/gamedev-introduction-to-proportional-navigation-part-i
	Vector3 ProportionalNavigation()
    {

        Vector3 accel = Vector3.zero;
        //accel = N * Vc * LOS_rate
        //N = navigation gain/constant
        //Vc = closing velocity
        //line of sight rotation rate

        //to get LOS rate
        var newRTM = target.transform.position - transform.position;
        newRTM = newRTM.normalized;
        if (oldRTM == Vector3.zero)
        {
            oldRTM = newRTM;
            Debug.Log("no oldRtm");
            return Vector3.zero;
        }
        var perpLOS = Vector3.Cross(oldRTM, newRTM); //TODO might need to referse the order
        var LOSDelta = (newRTM - oldRTM);
        var LOSRate = LOSDelta.magnitude;
        oldRTM = newRTM;

        var Vc = -LOSRate; //range closing rate
        accel = newRTM * N * Vc * LOSRate + LOSDelta * Nt * (0.5f * N);
        accel = accel.normalized;// Mathf.Min(300, accel.magnitude);

        accel = newRTM * N * Vc * LOSRate;
        var pn = N * LOSRate;
        var turnQuat = Quaternion.FromToRotation(transform.forward, LOSDelta);
        var angle = Vector3.Angle(transform.forward, LOSDelta);
        var pnQuat = Quaternion.AngleAxis(pn, perpLOS);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, pnQuat, 40 * Time.deltaTime);
        transform.rotation = turnQuat * transform.rotation;
        Debug.Log("losrate " + angle);

        return accel;//what if this is the steering vector?
    }
    //derived from Mariano Appendino
    //https://www.youtube.com/watch?v=yVtfD29WN9M
    //https://pastebin.com/yu9ZmKbS
    Vector3 AgumentedProportionalNavigation()
    {
        float N = 4;
        float kp = 0.02f;
        float deltaTime = Time.deltaTime;
        var newRTM = target.transform.position - transform.position;
        if (oldRTM == null)
        {
            oldRTM = newRTM;
            return Vector3.zero;
        }
        var targetDir = newRTM;
        //var deltaPitch = Mathf.Asin(Mathf.Sin())
        //TODO add grav vector
        Vector3 accel = Vector3.zero;
        return accel;
    }
    //Pt = t1 Vt + Pt0
    //Pm = t1 Vm + Pm0
    //t1 = some time in future
    //Vt target velocity 
    //Vm missile velocity
    //Pt0 target pos at t=0
    //Pm0 missile pos at t=0
    GetMinOf min = new GetMinOf();
    Vector3 SlowDogCurve()
    {
        var line = GetComponent<LineRenderer>();
        //find desired velocity vector
        var Vt = target.GetComponent<Rigidbody>().velocity;
        var Pt0 = target.transform.position;
        var Pm0 = transform.position;
        float Sm = 10;
        //loop over all time t = [0, 100)
        line.SetWidth(.1f, 1f);
        line.SetPosition(0, new Vector3(40, 0, 0));
        line.SetPosition(1, Vector3.zero);
        line.SetPosition(2, new Vector3(0, 40, 0));
        int i = 0;
        for (float t = 0.1f; t < 97; t += 0.1f)
        {
            Vector3 Vm = (t * Vt + Pt0 - Pm0).normalized * Sm;
            var Pt = t * Vt + Pt0;
            var Pm = t * Vm + Pm0;
            var dist = (Pt - Pm).magnitude;
            min.Update(dist, t, Vm);
            line.SetPosition(i+3, new Vector3((float)i / 10, dist/10, 0));
            i++;
        }
        Debug.Log("t=" + min.time + " minDist: " + min.index + " minV: " + min.result);
        //if too slow, use target velocity vector
        //var desiredVm = (min.index > 1 ) ? min.result : Vt;
        var desiredVm = min.result;
        var changeReq = desiredVm - rb.velocity;
        changeReq = changeReq.normalized * Mathf.Min(changeReq.magnitude, 10); //cap change speed, but still in direction of desired vel, not necessarily orthogonal to current Vm though...
        //rb.velocity = minVm;
        return changeReq;
    }
    //orbital homing missile algorithm derived from Peter Sharpe
    //https://www.youtube.com/watch?v=rhZ1_mOBDzQ
    //https://pastebin.com/mpqWFVbX
    Vector3 LongRangeHoming()
    {
        Vector3 accel = Vector3.zero;
        return accel;
    }
	// Update is called once per frame
	void Update () {
		if (Time.time - BornTime > DieAfterTime)
        {
            Debug.Log("Born time: " + BornTime + " Now: " + Time.time);
            Destroy(gameObject);
            return;
        }
		if (Time.time - BornTime > FireDelay)
        {
            if (target == null)
                return;
            //rb.AddForce(dir.normalized * Acceleration * Time.deltaTime);
            //rb.velocity += dir.normalized * Acceleration * Time.deltaTime;
            //rocket acceleration
            //accelerate towards target!
            var rocketAccel = Vector3.zero;
            //rocketAccel += ProportionalNavigation(); 
            if (true ) // proportional navigation
            {
                ProportionalNavigation();
                rocketAccel += transform.forward * 20;
                SlowDogCurve();
            } else {    // slow dog curve 
                rocketAccel = SlowDogCurve();
            }
            Debug.Log("accel: " + rocketAccel.magnitude);
            Debug.Log("velocity: " + rb.velocity.magnitude);
            rb.velocity += rocketAccel * Time.deltaTime;
            //transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
	}
}
