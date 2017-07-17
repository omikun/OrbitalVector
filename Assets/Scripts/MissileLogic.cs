using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMinOf
{
    public float index = float.PositiveInfinity;
    public float time = 0;
    public Vector3 result = Vector3.zero;
    public GetMinOf() { }
    public void Clear()
    {
        index = float.PositiveInfinity;
        time = 0;
        result = Vector3.zero;
    }
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
    public float MaxAcceleration = 40;
    public float MaxTurnRate = 40; //40 degrees per second
    public GameObject mainCamera;
    Rigidbody rb;

    //PN algorithm
    public float N = 3;
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
        //Vc m/s towards target; closing rate
        newRTM = newRTM;
        if (oldRTM == Vector3.zero)
        {
            oldRTM = newRTM;
            Debug.Log("no oldRtm");
            return Vector3.zero;
        }
        var LOSDelta = (newRTM.normalized - oldRTM.normalized);
        var Vc = -1 * (newRTM.magnitude - oldRTM.magnitude) / Time.deltaTime;
        //var LOSDelta = (oldRTM - newRTM);
        oldRTM = newRTM;

        if (true) //from Desprez from Unity forum
        {
            Vector3 dirDelta = newRTM.normalized - oldRTM.normalized;
            float pnGain = N;
            dirDelta -= Vector3.Project(dirDelta, newRTM);
            // basic pro nav
             Vector3 a = dirDelta * pnGain;

            // augmented pro nav
            //Vector3 a = dirDelta * (pnGain + (dirDelta.magnitude * pnGain * .5f));

            var aimLoc = transform.position + (newRTM.normalized * rb.velocity.magnitude ) + a;
            transform.LookAt(aimLoc);

            return Vector3.zero;
        }
        
        //accel = newRTM * N * Vc * LOSRate + LOSDelta * Nt * (0.5f * N);
        //accel = accel.normalized;// Mathf.Min(300, accel.magnitude);
        var LOSRate = Vector3.Angle(transform.forward, LOSDelta); //ideal
        var pn = N * LOSRate;
        var turnRate = N * Vc * LOSRate;
        turnRate = Mathf.Min(turnRate, MaxTurnRate * Time.deltaTime); //overturn/realistic
        turnRate = Mathf.Max(turnRate, -MaxTurnRate * Time.deltaTime); //overturn/realistic

        var turnAxis = Vector3.Cross(transform.forward, LOSDelta); //TODO might need to referse the order
        transform.Rotate(turnAxis.normalized * turnRate );
        Debug.Log("LOSRate " + LOSRate);
        Debug.Log("turnRate " + turnRate);

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
        min.Clear();
        var line = GetComponent<LineRenderer>();
        //find desired velocity vector
        var Vt = target.GetComponent<Rigidbody>().velocity;
        var Vm0 = rb.velocity;
        var Pt0 = target.transform.position;
        var Pm0 = transform.position;
        float Sm = 20;
        //loop over all time t = [0, 100)
        line.SetWidth(.1f, 1f);
        line.SetPosition(0, new Vector3(40, 0, 0));
        line.SetPosition(1, Vector3.zero);
        line.SetPosition(2, new Vector3(0, 40, 0));
        int i = 0;
        for (float t = 0.1f; t < 97; t += 0.1f)
        {
            //assuming constant velocity
            Vector3 Vm = (t * Vt + Pt0 - Pm0).normalized * Sm;
            //assuming constant acceleration
            //Vector3 Pm = 2 * (Vt * t + Pt0 - Pm0 - Vm0 * t) / (t*t);
            var Pt = t * Vt + Pt0;
            var Pm = t * Vm + Pm0;
            var dist = (Pt - Pm).magnitude;
            min.Update(dist, t, Vm);
            line.SetPosition(i+3, new Vector3((float)i / 10, dist/10, 0));
            i++;
        }
        Debug.Log("t=" + min.time + " minDist: " + min.index + " minV: " + min.result.magnitude);
        //if too slow, use target velocity vector
        //var desiredVm = (min.index > 1 ) ? min.result : Vt;
        var desiredVm = min.result;
        var changeReq = desiredVm - rb.velocity;
        //changeReq = changeReq.normalized * MaxAcceleration;// Mathf.Min(changeReq.magnitude, MaxAcceleration); //cap change speed, but still in direction of desired vel, not necessarily orthogonal to current Vm though...
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
	void FixedUpdate () {
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
            var rocketAccel = Vector3.zero;
            //rocketAccel += ProportionalNavigation(); 
            if (false ) // proportional navigation
            {
                ProportionalNavigation();
                rocketAccel += transform.forward * MaxAcceleration;
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
