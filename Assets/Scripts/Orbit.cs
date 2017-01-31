using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OrbitalTools;
using System;
 using System.Text;
 using System.IO;
using UnityEditor;

public static class OVDebug {
    public static Vector3d projectedR1;
}
public class ManeuverEvent : GameEvent
{
    public ManeuverEvent (GameObject src, GameObject tgt, float timeInFuture, string action, Vector3d v) : base(src, tgt, timeInFuture, action)
    {
        velocity = v;
    }
    public Vector3d velocity;
    public GameObject projectile;
}
public class Orbit : MonoBehaviour
{
    public int segments = 360;
    List<LineRenderer> lines = new List<LineRenderer>();
    LineRenderer interceptLine;
    public GameObject display;
    private Text textRef;
    public static Vector3 accelVector;
    public GameObject OrbitRenderer;
    public static List<String> output = new List<String>();

    Vector3 apo, peri;
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

    public void CreateNewShip()
    {
        //get ship template
        var temp = GameObject.Find("ship_test1");
        //instantiate new one
        var randomPos = new Vector3(4, 0, 0);
        GameObject newShip = (GameObject)Instantiate(temp, randomPos, temp.transform.rotation);
        newShip.transform.parent = transform.parent;
        newShip.transform.localPosition = randomPos;
        newShip.transform.localScale = new Vector3(.1f, .1f, .1f);
        Debug.Log("newly created ship position: " + newShip.transform.position);
        Debug.Log("newly created ship localposition: " + newShip.transform.localPosition);
        newShip.GetComponent<OrbitData>().Init();
        AddOrbitRenderer(newShip);
        //TODO orbit renderer not fixed to holoroot
        //newShip.GetComponent<OrbitData>().Init();
    }
    void OnEnable()
    {
        Events.instance.AddListener<ManeuverEvent>(OnManeuverEvent);
    }
    void OnDisable()
    {
        Events.instance.RemoveListener<ManeuverEvent>(OnManeuverEvent);
    }
    void OnManeuverEvent(ManeuverEvent e)
    {
        //coroutine execute in x time?
        Debug.Log("ManeuverEvent fired");
        //StartCoroutine(AdjustOrbit(e));
        AdjustOrbit(e);
        
    }
    //specifies dv at this time
    //IEnumerator AdjustOrbit(ManeuverEvent e)
    void AdjustOrbit(ManeuverEvent e)
    {
        /*
        float time = (float)e.GetTime();

        while (time > 5)
        {
            Debug.Log("Waiting for " + time + " seconds");
            yield return new WaitForSeconds(5);
            time -= 5;
        }
        Debug.Log("Waiting for " + time + " seconds");
        yield return new WaitForSeconds(time);
        */
        Debug.Log("Name of source: " + e.GetSource().name);
        Debug.Log("Firing! " + OVTools.FormatDistance((float)e.velocity.magnitude) + "/s");
        Debug.Log("injection vector: " + e.velocity.ToString());
        Debug.Log("Injection time: " + eventManager.GetSimTime());

        var odata = e.GetSource().GetComponent<OrbitData>();
        Debug.Log("Injection pos: " + odata.getR().ToString());
        var diff = odata.getR() - OVDebug.projectedR1;
        Debug.Log("Difference: " + OVTools.FormatDistance((float)diff.magnitude));
        if (odata == null)
        {
            Debug.Log("no orbital data from ship!!");
        }

        odata.rv[3] = e.velocity.x;
        odata.rv[4] = e.velocity.y;
        odata.rv[5] = e.velocity.z;

        //update oe
        var oe = Util.rv2oe(OrbitData.parentGM, odata.rv);
        odata.SetOE(oe);

        Debug.Log("InjVec: " + e.velocity);
        //disable intercept line render
        interceptLine.enabled = false;
        var marker1 = GameObject.Find("Marker1");
        marker1.transform.localPosition = Vector3.zero;
        var marker2 = GameObject.Find("Marker2");
        marker2.transform.localPosition = Vector3.zero;
    }
    void drawOrbitalPath(LineRenderer line, float tra, float a, float e, Quaternion rot)
    {
        //inputs
        tra = (tra >= Mathf.PI) ? tra - 2 * Mathf.PI : tra;
        float theta = (a > 0) ? -Mathf.PI : tra;
        Vector3 pos = Vector3.zero;
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
            line.SetPosition(i, rot * pos);//hacky
            if (i == 0) { apo = rot * pos; }
            if (i == segments/2) { peri = rot * pos; }
            //if (drawPathCount == 0)
             //   Debug.Log("draw path count");
        }

    }

    EventManager eventManager;
    void Start()
    {
        textRef = display.GetComponent<Text>();
        accelVector = Vector3.zero;
        //HACK FIXME workaround line.SetPosition is relative to parent translation, but absolute to world initially
        //one time offset relative to intiial parent position
        foreach (GameObject ship in GameObject.FindGameObjectsWithTag("ship"))
        {
            AddOrbitRenderer(ship);
        }
        //just for intercept orbit renderer
        var newObj = Instantiate(OrbitRenderer);
        newObj.transform.parent = transform;
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = transform.localScale;
        interceptLine = newObj.GetComponent<LineRenderer>();
        interceptLine.SetVertexCount(segments+1);
        interceptLine.material = new Material(Shader.Find("Particles/Additive"));
        interceptLine.SetColors(Color.white, Color.red);
        interceptLine.enabled = false;
        //InvokeRepeating("prepFindIntercept", 5.0f, 2f); //debug

        eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
    }
    void AddOrbitRenderer(GameObject ship)
    {
        var newObj = Instantiate(OrbitRenderer);
        //TODO need to match localPosition in newly instantiated ship with ship that comes at startup
        newObj.transform.parent = transform;
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = transform.localScale;
        var newLineRenderer = newObj.GetComponent<LineRenderer>();
        newLineRenderer.SetVertexCount(segments+1);
        lines.Add(newLineRenderer);
    }
    void FindRV(OrbitalElements oe, double time, out Vector3d r, out Vector3d v)
    {
        var tempOe = oe.CopyOE();
        tempOe.tra = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe, time);
        OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe, out r, out v);
    }
    
    // Update is called once per frame
    bool first = true;
    void FixedUpdate()
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

            var simdeltatime = HoloManager.SimTimeScale /2 * Time.fixedDeltaTime;
            bool integration = false;
            if (integration)
            {
                odata.rv = Util.rungeKutta4(0, simdeltatime, odata.rv, odata.params_);

                odata.params_[4] = 0;
                odata.params_[5] = 0;
                odata.params_[6] = 0;

                odata.SetOE(Util.rv2oe(OrbitData.parentGM, odata.rv));

            } else {
                Vector3d r1, v1;
                //var newOE = Util.rv2oe(OrbitData.parentGM, odata.rv);
                FindRV(odata.getOE(), eventManager.GetSimTime() - odata.GetOETime(), out r1, out v1);
                odata.rv = Util.convertToRv(ref r1, ref v1);
            }

            var oe = odata.getOE();
            DrawOrbit(lines[count], ref oe);

            if (first)
            {
                var period = oe.GetPeriod();
                oe.print();
                Debug.Log("alt: " + OVTools.FormatDistance((float)odata.getR().magnitude)
                    + " Period: " + OVTools.FormatTime((float)period));
            }
    
            //draw icons for selected orbits
            if (ship == UXStateManager.GetSource())
            {
                var iconApo = GameObject.Find("iconApo");
                iconApo.transform.localPosition = apo;
                var iconPeri = GameObject.Find("iconPeri");
                iconPeri.transform.localPosition = peri;
            }
            count++;
        }
        first = false;
    }

    public void updateInterceptLine(ref OrbitalElements oe, bool enable)
    {
        interceptLine.enabled = enable;
        DrawOrbit(interceptLine, ref oe);
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

        var simSMA = (float)oe.sma*(float)HoloManager.SimZoomScale;
        //Debug.Log("SMA: " + oe.sma + "sim: " + simSMA);
        drawOrbitalPath(line, (float)oe.tra, simSMA, (float)oe.ecc, rotq);
    }

    [CustomEditor(typeof(Orbit))]
    public class ObjectBuilderEditor : Editor{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Orbit or = (Orbit)target;
            if(GUILayout.Button("Draw Orbits"))
            {
                or.Start();
                or.FixedUpdate();
            }
        }
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
        double period = oe1.GetPeriod();//use period of source ship Math.Max(oe1.getPeriod(), oe2.getPeriod());
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
            var tempoe1 = oe1.CopyOE();
            var tempoe2 = oe2.CopyOE();
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
