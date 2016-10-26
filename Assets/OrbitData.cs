using UnityEngine;
using System.Collections;
using System;
using OrbitalTools;

public class OrbitData : MonoBehaviour {

    public VectorD rv;
    public VectorD params_;
    OrbitalElements oe = new OrbitalElements();

    public static float scale = 1;
    static double r = 4;
    static double m = 7e10;
    static double G = 6.673e-11;
    public static double parentGM = m * G;
    public static class Global
    {
        public static double[] vel = new double[] { .1d, 0, Math.Sqrt(parentGM / r) };
        public static double[] pos = new double[] { r+.2d, 0, .1d};
    }
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

    public double getPeriod()
    {
        return 2 * Math.PI * Math.Sqrt(oe.sma / parentGM);
    }
    void Start()
    {
        rv = new VectorD();
        rv.Resize(6);

        Debug.Log("position " + transform.position);
        rv[0] = transform.position.x;
        rv[1] = transform.position.y;
        rv[2] = transform.position.z;
        //comput velocity assuming pos is in the origin
        var vel = Vector3.Cross(transform.position, Vector3.up).normalized;
        vel *= Mathf.Sqrt((float)parentGM / transform.position.magnitude);
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
        var position = scale * new Vector3((float)rv[0], 
                                         (float)rv[1], (float)rv[2]);
        //transform.Rotate(0, OR_Controller.totalAngle, 0, Space.World);
        transform.position = Quaternion.AngleAxis(OR_Controller.totalAngle, Vector3.up) * position;
        transform.Translate(OR_Controller.totalMoveY, Space.World);

    }
}
