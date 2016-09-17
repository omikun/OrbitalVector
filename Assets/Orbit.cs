using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OrbitalTools;
using System;

public class Orbit : MonoBehaviour {
    public int segments = 360;
    public float semiMajor = 1;
    public float semiMinor = .5f;
    LineRenderer line;
    public GameObject test;
    public Vector3 parentPos;
    public Vector3 accel;

    //This is just a test, not actually used for anything
    void drawEllipse()
    {

        Vector3 pos;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * i * 2;
            pos.x = (Mathf.Cos(rad) * semiMajor);
            pos.z = (Mathf.Sin(rad) * semiMinor);
            pos.y = (0);
            line.SetPosition(i, pos);
        }
    }

    void drawOrbitalPath(float tra, float a, float e, Quaternion rot)
    {
        //clear path?
        //assert path of certain size
        //inputs
        tra = (tra >= Mathf.PI) ? tra - 2 * Mathf.PI : tra;
        float theta = (a > 0) ? -Mathf.PI : tra;
        Vector3 pos = Vector3.zero;
        //pos.0 = -.2f;
        int i = 0;
        for (; theta < Mathf.PI; theta += Mathf.PI*2/segments, i++)
        {
            float denominator = (1.0f + e * Mathf.Cos(theta));
            if (denominator < 0)
            {
                //            continue;
            }
            float r = a * (1.0f - e * e) / denominator;
            pos.x = r * Mathf.Cos(theta);
            pos.z = r * Mathf.Sin(theta);
            line.SetPosition(i, rot*pos);
        }
        line.SetPosition(++i, Vector3.zero);
        line.SetPosition(++i, Vector3.up);

	}

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }
    // Update is called once per frame
    void Update ()
    {
        DrawOrbit();
    }
    static double r = 4;
    static double m = 7e10;
    static double G = 6.673e-11;
    static double parentGM = m * G;
    public static class Global
    {
        public static double[] vel = new double[] { .1d, .8d, Math.Sqrt(parentGM / r) };
        public static double[] pos = new double[] { r, -.2d, .1d};
    }
    Vector3 RotateAround(Vector3 center, Vector3 axis, float angle)
    {
        var pos = transform.position;
        var rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        var dir = pos - center; // find current direction relative to center
        dir = rot * dir; // rotate the direction
        return center + dir;
        // rotate object to keep looking at the center:
        //        var myRot = transform.rotation;
        //       transform.rotation *= Quaternion.Inverse(myRot) * rot * myRot;
    }
    void DrawOrbit() {
#if false
        Debug.Log("pos: " + Global.pos[0] + " " + Global.pos[2]);
        Debug.Log("vel: " + Global.vel[0] + " " + Global.vel[2]);
#endif
        VectorD rv = Util.convertToRv(ref Global.pos, ref Global.vel);
        var oe = Util.rv2oe(parentGM, rv);

            //build rotation
            var aop = Quaternion.AngleAxis((float)oe.aop, Vector3.up);
            var inc = Quaternion.AngleAxis((float)oe.inc, Vector3.right);
            var lan = Quaternion.AngleAxis((float)oe.lan, Vector3.up);
            var rotq = lan * inc * aop;
        drawOrbitalPath((float)oe.tra, (float)oe.sma, (float)oe.ecc, rotq);
        //drawEllipse();

        //calculate next pos/vel
        //params is parent pos, gm, and inject acceleration!
        double[] parentPos = new double[3];
        double[] accel = new double[3];
        VectorD params_ = Util.convertToParams(parentPos, parentGM, accel);
        params_.Print("params: ");
        rv = Util.rungeKutta4(0, Time.deltaTime, rv, params_);
        Global.pos[0] = rv[0];
        Global.pos[1] = rv[1];
        Global.pos[2] = rv[2];
        Global.vel[0] = rv[3];
        Global.vel[1] = rv[4];
        Global.vel[2] = rv[5];
        params_.Print("params: ");
        test.transform.position = new Vector3((float)Global.pos[0],
           (float)Global.pos[1],(float)Global.pos[2]);
    }
    
}
