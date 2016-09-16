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
    Rigidbody rb;

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

    void drawOrbitalPath(float tra, float a, float e)
    {
        //clear path?
        //assert path of certain size
        //inputs
        tra = (tra >= Mathf.PI) ? tra - 2 * Mathf.PI : tra;
        float theta = (a > 0) ? -Mathf.PI : tra;
        Vector3 pos = Vector3.zero;
        //pos.0 = -.2f;
        for (int i=0; theta < Mathf.PI; theta += Mathf.PI*2/segments, i++)
        {
            float denominator = (1.0f + e * Mathf.Cos(theta));
            if (denominator < 0)
            {
                //            continue;
            }
            float r = a * (1.0f - e * e) / denominator;
            pos.x = r * Mathf.Cos(theta);
            pos.z = r * Mathf.Sin(theta);
            line.SetPosition(i, pos);
        }
        // Use this for initialization
        line = GetComponent<LineRenderer>();
	}

    void Start()
    {
        line = GetComponent<LineRenderer>();
        rb = test.GetComponent<Rigidbody>();

       
    }
    // Update is called once per frame
    void Update () {

var pos = test.transform.position;
var vel = rb.velocity;
        Debug.Log("vel: " + vel);
 var pos = test.transform.position;
        var vel = rb.velocity;
        double m = 7e12;
        double G = 6.673e-11;
        double parentGM = m * G;
        double r = 4;
        List<double> rv = new List<double> { r, 0, .1d,
            .1, 0, Math.Sqrt(parentGM/r)};
        //List<double> rv = new List<double>{ pos.x, pos.y, pos.z, vel.x, vel.y, vel.z };
        var oe = Program.rv2oe(parentGM, rv);
        oe.print();
        drawOrbitalPath((float)oe.tra, (float)oe.sma, (float)oe.ecc);
        //drawEllipse();
        
    }
}
