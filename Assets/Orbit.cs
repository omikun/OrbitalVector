using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OrbitalTools;
using System;

public class Orbit : MonoBehaviour
{
    public int segments = 360;
    public static float timeScale = 1;
    List<LineRenderer> lines = new List<LineRenderer>();
    public GameObject display;
    private Text textRef;
    public Vector3 parentPos;
    public static Vector3 accelVector;
    public GameObject OrbitRenderer;

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
            lines[0].SetPosition(i, pos);
        }
    }

    void drawOrbitalPath(LineRenderer line, float tra, float a, float e, Quaternion rot)
    {
        //clear path?
        //inputs
        tra = (tra >= Mathf.PI) ? tra - 2 * Mathf.PI : tra;
        float theta = (a > 0) ? -Mathf.PI : tra;
        Vector3 pos = Vector3.zero;
        //pos.0 = -.2f;
        int i = 0;
        for (; theta < Mathf.PI; theta += Mathf.PI * 2 / segments, i++)
        {
            float denominator = (1.0f + e * Mathf.Cos(theta));
            if (denominator < 0)
            {
                //            continue;
            }
            float r = a * (1.0f - e * e) / denominator;
            pos.x = r * Mathf.Cos(theta);
            pos.z = r * Mathf.Sin(theta);
            line.SetPosition(i, rot * pos);
        }
        line.SetPosition(i++, Vector3.zero);
        //line.SetPosition(i++, rot*Vector3.up);

    }

    void Start()
    {
        textRef = display.GetComponent<Text>();
        accelVector = Vector3.zero;

        foreach (GameObject ship in GameObject.FindGameObjectsWithTag("ship"))
        {
            AddOrbitRenderer(ship);
        }

        InvokeRepeating("prepFindIntercept", 15.0f, .1f);
    }
    void AddOrbitRenderer(GameObject ship)
    {
        var newObj = Instantiate(OrbitRenderer);
        newObj.transform.parent = transform;
        var newLineRenderer = newObj.GetComponent<LineRenderer>();
        newLineRenderer.SetVertexCount(segments + 2);
        lines.Add(newLineRenderer);
    }
    static int FrameCount = 0;
    // Update is called once per frame
    void Update()
    {
        int count = 0;
        foreach (GameObject ship in GameObject.FindGameObjectsWithTag("ship"))
        {
            if (count >= lines.Count)
            {
                Debug.Log("More ships than there are orbit renderers!");
                AddOrbitRenderer(ship);
            }
            if (ship == null)
            {
                Debug.Log("ship is null!");
            }
            var odata = ship.GetComponent<OrbitData>();

            if (odata == null)
            {
                Debug.Log("no orbital data from ship!!");
            }
            //calculate next step
            odata.rv = Util.rungeKutta4(0, timeScale * Time.deltaTime, odata.rv, odata.params_);
            odata.params_[4] = 0;
            odata.params_[5] = 0;
            odata.params_[6] = 0;
            DrawOrbit(lines[count], ref odata.rv);

            count++;
        }

    }
    void prepFindIntercept()
    {
#if true
        //show markers on closest approach
        var od = GameObject.Find("ship_test1").GetComponent<OrbitData>();
        if (od == null) Debug.Log("can't find ship1.OrbitData");
        //Debug.Log("printing OrbitData: " + od.oe);
        //OrbitalElements oe1 = od.getOE();
        var oe1 = Util.rv2oe(OrbitData.parentGM, od.rv);
        if (oe1 == null) Debug.Log("can't find ship1.OrbitData.oe");

        var od2 = GameObject.Find("ship_test2").GetComponent<OrbitData>();
        var oe2 = Util.rv2oe(OrbitData.parentGM, od2.rv);
        if (oe2 == null) Debug.Log("can't find ship2.OrbitData.oe");
        //Debug.Log("calling findInterceptPoint");
        
        findInterceptPoints(oe1, oe2);
#endif
    }

    void DrawOrbit(LineRenderer line, ref VectorD rv)
    {
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
        sb.Append("aop: " + (oe.aop * Mathf.Rad2Deg).ToString("#.0") + "\n");
        sb.Append("inc: " + (oe.inc * Mathf.Rad2Deg).ToString("#.0") + "\n");
        sb.Append("lan: " + (oe.lan * Mathf.Rad2Deg).ToString("#.0") + "\n");
        textRef.text = sb.ToString();
#endif
        //var newRV = Util.oe2rv(OrbitData.parentGM, oe);
        //Debug.Log("rv[0] " + rv[3] + " rv2[0] " + newRV[3]);

        //build rotation
        var aop = Quaternion.AngleAxis((float)oe.aop * Mathf.Rad2Deg, Vector3.up);
        var inc = Quaternion.AngleAxis((float)oe.inc * Mathf.Rad2Deg, Vector3.right);
        var incOffset = Quaternion.AngleAxis(90f, Vector3.right);
        var lan = Quaternion.AngleAxis((float)oe.lan * Mathf.Rad2Deg, Vector3.up);
        var rotq = incOffset * lan * inc * aop;

        drawOrbitalPath(line, (float)oe.tra, (float)oe.sma, (float)oe.ecc, rotq);
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

    //find closest point
    public GameObject intMarker1, intMarker2;
    int steps = 0;
        double time = 0;
        float prevDiff = float.MaxValue;
    public void findInterceptPoints(OrbitalElements oe1, OrbitalElements oe2)
    {
        double gm = OrbitData.parentGM;
        double period = oe1.getPeriod();
        double timeStep = period / 10;
        double tra1 = oe1.tra, tra2 = oe2.tra;
        //TODO consider repeating this loop to find second closest intercept
#if true
        var debugLine = transform.Find("debug").GetComponent<LineRenderer>();
        //while (timeStep >= .01d && time < period && steps < 400)
        {
            tra1 = Program.anomalyAfterTime(OrbitData.parentGM, oe1, time);
            tra2 = Program.anomalyAfterTime(OrbitData.parentGM, oe2, time);
            var tempoe1 = oe1.copyOE();
            var tempoe2 = oe2.copyOE();
            tempoe1.tra = tra1;
            tempoe2.tra = tra2;
            var pos1 = Util.oe2r(OrbitData.parentGM, tempoe1);
            var pos2 = Util.oe2r(OrbitData.parentGM, tempoe2);
            float thisDiff = (pos1 - pos2).magnitude;
            if (thisDiff < prevDiff)
            {
            oe1.tra = tra1;
            oe2.tra = tra2;
                time += timeStep;
                prevDiff = thisDiff;
            } else {
                timeStep /= 2;
                time -= timeStep;
            }
            steps++;
            debugLine.SetPosition(0, pos1);
            debugLine.SetPosition(1, pos2);
        }
#endif
        {
            VectorD rv = Util.oe2rv(OrbitData.parentGM, oe1);
            Vector3 pos = new Vector3((float)rv[0],
                                      (float)rv[1],
                                      (float)rv[2]);
            //Debug.Log("intercept tra: " + tra1 + " pos: " + pos);
            pos = Util.oe2r(OrbitData.parentGM, oe2);
            //Debug.Log("intercept tra: " + tra2 + " pos: " + pos);
            intMarker1.transform.position = pos;
            intMarker2.transform.position = Util.oe2r(OrbitData.parentGM, oe2);
        } 
    }
    //find point of inclination, periapsis, apoapsis, etc
    //AN/DN - whatever points that are at y=0! :D
    //periapsis: lan + aop = tra of peri, plug that in to oe2r to get r
    //apoapsis: periapsis + 180
}
