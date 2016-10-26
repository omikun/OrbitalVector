using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OrbitalTools;
using System;
 using System.Text;
 using System.IO;

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

    public static List<String> output = new List<String>();
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
        //line.SetPosition(i++, Vector3.zero);
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

        //InvokeRepeating("prepFindIntercept", 5.0f, 2f);
    }
    void AddOrbitRenderer(GameObject ship)
    {
        var newObj = Instantiate(OrbitRenderer);
        newObj.transform.parent = transform;
        var newLineRenderer = newObj.GetComponent<LineRenderer>();
        newLineRenderer.SetVertexCount(segments+1);
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
            if (odata.params_[4] != 0)
                Debug.Log("acceleration detected!");
            odata.rv = Util.rungeKutta4(0, timeScale * Time.deltaTime, odata.rv, odata.params_);
            odata.params_[4] = 0;
            odata.params_[5] = 0;
            odata.params_[6] = 0;

            odata.setOE(Util.rv2oe(OrbitData.parentGM, odata.rv));
            var oe = odata.getOE();
            DrawOrbit(lines[count], ref oe);

            count++;
        }

    }
    public void prepFindIntercept()
    {
        var src = UXStateManager.GetSource();
        var tgt = UXStateManager.GetTarget();
        if (src == null || tgt == null)
            return;
        //show markers on closest approach
        var od1 = src.GetComponent<OrbitData>();
        var oe1 = Util.rv2oe(OrbitData.parentGM, od1.rv);

        var od2 = tgt.GetComponent<OrbitData>();
        var oe2 = Util.rv2oe(OrbitData.parentGM, od2.rv);
        
        findInterceptPoints(oe1, oe2);
    }

    void DrawOrbit(LineRenderer line, ref OrbitalElements oe)
    {
#if false
        Debug.Log("pos: " + Global.pos[0] + " " + Global.pos[2]);
        Debug.Log("vel: " + Global.vel[0] + " " + Global.vel[2]);
#endif
        //VectorD rv = Util.convertToRv(ref Global.pos, ref Global.vel);
        //VectorD rv = calcNextStep(rv, params_);

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

    bool writeOnce = true;
    //find closest point
    public GameObject intMarker1, intMarker2;
    public void findInterceptPoints(OrbitalElements oe1, OrbitalElements oe2)
    {
        int steps = 0;
        double time = 0;
        float prevDiff = float.MaxValue;
        double tra1, tra2;
        bool first = true;
        double timeStep = 0;
        bool forwardMode = true;

        if (steps >= 360)
        {
            if (writeOnce)
                //Savecsv();
            writeOnce = false;
            return;
        }

        double gm = OrbitData.parentGM;
        double period = oe1.getPeriod();//use period of source ship Math.Max(oe1.getPeriod(), oe2.getPeriod());
        if (first)
        {
            timeStep = period / 360;
            tra1 = oe1.tra;
            tra2 = oe2.tra;
        }
        var debugLine = transform.Find("debug").GetComponent<LineRenderer>();
        Vector3 minpos1 = new Vector3();
        Vector3 minpos2 = new Vector3();
        while (timeStep >= .01d && time < period && steps < 360)
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
            //Debug.Log(thisDiff);
            //output.Add("C," + tra1 + "," + tra2 + "," + thisDiff + "\n");
            if (thisDiff < prevDiff && forwardMode)
            {
                prevDiff = thisDiff;
                minpos1 = pos1;
                minpos2 = pos2;
            }
            time += timeStep;
            //else {
            if (false) {
                forwardMode = false;
                Debug.Log("time step halved!");
                timeStep /= 2;
                time -= timeStep;
            }
            steps++;
            debugLine.SetPosition(0, pos1);
            debugLine.SetPosition(1, pos2);
            intMarker1.transform.localPosition = minpos1;// Util.oe2r(OrbitData.parentGM, oe1);
            intMarker2.transform.localPosition = minpos2;// Util.oe2r(OrbitData.parentGM, oe2);
        } 
    }
    //find point of inclination, periapsis, apoapsis, etc
    //AN/DN - whatever points that are at y=0! :D
    //periapsis: lan + aop = tra of peri, plug that in to oe2r to get r
    //apoapsis: periapsis + 180

    public static void Savecsv()
    {
        string filePath = @"/temp/Saved_data.csv";

        int length = output.Count;
        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < length; index++)
            sb.Append(output[index]);

        File.WriteAllText(filePath, sb.ToString());
    }
}
