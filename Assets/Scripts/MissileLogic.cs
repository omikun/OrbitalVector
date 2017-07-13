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
    Rigidbody rb;

    //PN algorithm
    public float N = 3;
    Vector3 oldRTM;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
    //first pass, doesn't work very well; must be firing within 5 degrees of a stationary target :(
	Vector3 ProportionalNavigation()
    {
        Vector3 accel = Vector3.zero;
        //accel = N * Vc * LOS_rate
        //N = navigation gain/constant
        //Vc = closing velocity
        //line of sight rotation rate

        //to get LOS rate
        var newRTM = target.transform.position - transform.position;
        if (oldRTM == null)
        {
            oldRTM = newRTM;
            return Vector3.zero;
        }
        var LOSDelta = (newRTM - oldRTM);
        var LOSRate = LOSDelta.magnitude;
        oldRTM = newRTM;

        var Vc = LOSRate;
        accel = newRTM * N * Vc * LOSRate + LOSDelta * N;// t * (0.5 * N);
        accel = accel.normalized * Mathf.Min(300, accel.magnitude);

        return accel;
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
    GetMinOf getMin = new GetMinOf();
    Vector3 SlowDogCurve()
    {
        var line = GetComponent<LineRenderer>();
        //find desired velocity vector
        var Vt = target.GetComponent<Rigidbody>().velocity;
        var Pt0 = target.transform.position;
        var Pm0 = transform.position;
        float Sm = 10;
        //loop over all time t = [0, 100)
        int i = 0;
        for (float t = 0.1f; t < 100; t += 0.1f)
        {
            Vector3 Vm = (t * Vt + Pt0 - Pm0).normalized * Sm;
            var Pt = t * Vt + Pt0;
            var Pm = t * Vm + Pm0;
            var dist = (Pt - Pm).magnitude;
            getMin.Update(dist, t, Vm);
            line.SetPosition(i, new Vector3((float)i / 10, dist/10, 0));
            i++;
        }
        //minum is
        var minDist = getMin.index;
        var minT = getMin.time;
        var minVm = getMin.result;
        Debug.Log("t=" + minT + " minDist: " + minDist + " minV: " + minVm);
        //if too slow, use target velocity vector
        var desiredVm = minVm;
        var changeReq = desiredVm - rb.velocity;
        changeReq = changeReq.normalized * Mathf.Min(changeReq.magnitude, 10); //cap change speed at 5, but still in direction of desired vel, not necessarily orthogonal to current Vm
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
        //debug
        //SlowDogCurve();
        //return;
		if (Time.time - BornTime > FireDelay)
        {
            if (target == null)
                return;
            //accelerate towards target!
            var dir = target.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
            //rb.AddForce(dir.normalized * Acceleration * Time.deltaTime);
            //rb.velocity += dir.normalized * Acceleration * Time.deltaTime;
            //rocket acceleration
            var rocketAccel = rb.velocity.normalized * 10 * Time.deltaTime;
            //rb.velocity += ProportionalNavigation() * Time.deltaTime * 1f + rocketAccel;
            var changeAccel = SlowDogCurve();
            //var forwardAccel = rb.velocity.normalized * 10;

            rb.velocity +=  (changeAccel ) * Time.deltaTime; //always accelerating at
        }
	}
}
