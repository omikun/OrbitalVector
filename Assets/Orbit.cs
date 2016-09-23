using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OrbitalTools;
using System;

public class Orbit : MonoBehaviour {
    public int segments = 360;
    public static float timeScale = 1;
    LineRenderer line;
    public GameObject display;
    private Text textRef;
    public Vector3 parentPos;
    public static Vector3 accelVector;

    //This is just a test, not actually used for anything
    void drawEllipse()
    {
        float semiMajor = 1;
        float semiMinor = .5f;
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
        line.SetPosition(++i, rot*Vector3.up);

	}

    void Start()
    {
        line = GetComponent<LineRenderer>();

        textRef = display.GetComponent<Text>();
        accelVector = Vector3.zero;
    }
    // Update is called once per frame
    void Update ()
    {
        {

            var odata = GetComponent<OrbitData>();

            //calculate next step
            odata.rv = Util.rungeKutta4(0, timeScale * Time.deltaTime, odata.rv, odata.params_);
            odata.params_[4] = 0;
            odata.params_[5] = 0;
            odata.params_[6] = 0;
            DrawOrbit(ref odata.rv);
        }
    }

void DrawOrbit(ref VectorD rv) {
#if false
        Debug.Log("pos: " + Global.pos[0] + " " + Global.pos[2]);
        Debug.Log("vel: " + Global.vel[0] + " " + Global.vel[2]);
#endif
        //VectorD rv = Util.convertToRv(ref Global.pos, ref Global.vel);
        //VectorD rv = calcNextStep(rv, params_);

        var oe = Util.rv2oe(OrbitData.parentGM, rv);
#if true
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Orbital Vector\n");
        sb.Append("aop: " + (oe.aop*Mathf.Rad2Deg).ToString("#.0") + "\n");
        sb.Append("inc: " + (oe.inc*Mathf.Rad2Deg).ToString("#.0") + "\n");
        sb.Append("lan: " + (oe.lan*Mathf.Rad2Deg).ToString("#.0") + "\n");
        textRef.text = sb.ToString();
#endif

        //build rotation
        var aop = Quaternion.AngleAxis((float)oe.aop * Mathf.Rad2Deg, Vector3.up);
        var inc = Quaternion.AngleAxis((float)oe.inc * Mathf.Rad2Deg, Vector3.right);
        var lan = Quaternion.AngleAxis((float)oe.lan * Mathf.Rad2Deg, Vector3.up);
        var rotq = lan * inc * aop;

        drawOrbitalPath((float)oe.tra, (float)oe.sma, (float)oe.ecc, rotq);
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
      
}
