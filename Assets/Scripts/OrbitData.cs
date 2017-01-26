using UnityEngine;
using System.Collections;
using System;
using OrbitalTools;

public static class HoloManager
{
    public static float SolScale = (6371f * 1000f); //radius of earth (m) == 1 Unity Unit
    public static float SimZoomScale = 1f / SolScale;
    public static float SimTimeScale = 1;
}
public class OrbitData : MonoBehaviour {
    public VectorD rv;
    public VectorD params_;
    OrbitalElements oe = new OrbitalElements();
    float dv = 100;
    float maxDV = 200;
    public float GetDV() { return dv; }
    bool destroyed = false;
    public void Destroy() {
        if (destroyed)
            Debug.Log("Ship already destroyed!");
        destroyed = true;
    }
    public bool SubtractDV(float burnDv)
    {
        bool canBurn = dv >= burnDv;
        if (canBurn) dv -= burnDv;
        return canBurn;
    }
    public float AddDV(float addDV)
    {
        dv += addDV;
        if (dv > maxDV)
        {
            var ret = dv - maxDV;
            dv = maxDV;
            return ret;
        } else
        {
            return addDV;
        }
    }

    public static float scale = 1;
    static double r = 4;
    static double m = 5.972e24; //testing much larger mass so time scale is just 1
    static double G = 6.67408e-11;
    public static double parentGM = m * G;


    public Vector3 getVel()
    {
        return new Vector3((float)rv[3], 
                           (float)rv[4], (float)rv[5]);
    }
    public Vector3d getR()
    {
        return new Vector3d(rv[0], rv[1], rv[2]);
    }
    public Vector3d getV()
    {
        return new Vector3d(rv[3], rv[4], rv[5]);
    }

    public OrbitalElements getOE()
    {
        OrbitalElements ret = new OrbitalElements();
        ret.sma = oe.sma;
        ret.lan = oe.lan;
        ret.inc = oe.inc;
        ret.ecc = oe.ecc;
        ret.tra = oe.tra;
        ret.aop = oe.aop;
        return ret;
    }
    public void setOE(OrbitalElements oe_)
    {
        oe = oe_;
    }

    public double getPeriod()// doesn't this just assume circular orbits only?
    {
        return 2 * Math.PI * Math.Sqrt(oe.sma / parentGM);
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        rv = new VectorD();
        rv.Resize(6);

        Debug.Log("position " + transform.localPosition);
        rv[0] = transform.localPosition.x * HoloManager.SolScale;
        rv[1] = transform.localPosition.y * HoloManager.SolScale;
        rv[2] = transform.localPosition.z * HoloManager.SolScale;
        //comput velocity assuming pos is in the origin
        //TODO use double operations?
        var vel = Vector3.Cross(transform.localPosition, Vector3.up).normalized;
        vel *= Mathf.Sqrt((float)parentGM / (HoloManager.SolScale * transform.localPosition.magnitude));
        rv[3] = vel.x;
        rv[4] = vel.y;
        rv[5] = vel.z;


        params_ = new VectorD();
        params_.Resize(7);
        params_[0] = 0;
        params_[1] = 0;
        params_[2] = 0;
        params_[3] = parentGM;
        params_[4] = 0;
        params_[5] = 0;
        params_[6] = 0;
    }

    void Update()
    {
        var position = new Vector3((float)rv[0], (float)rv[1], (float)rv[2])
                           * HoloManager.SimZoomScale;
        transform.localPosition = position;
    }
}
