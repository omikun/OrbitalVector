using UnityEngine;
using System.Collections;
using System.Threading;
using OrbitalTools;
using UnityEditor;

public class PCPoint
    {
        public float dv = Mathf.Infinity;
        public double startTime, travelTime;
        PCPoint() { dv = Mathf.Infinity; }
    }
    
[CustomEditor(typeof(PorkChopPlot))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PorkChopPlot pcp = (PorkChopPlot)target;
        if(GUILayout.Button("Build Object"))
        {
            pcp.CreatePlot();
        }
    }
}
public class PorkChopPlot : MonoBehaviour {
    const int imgWidth = 40;
    const int imgHeight = 30;
    Color[] porkChopColors = new Color[imgWidth * imgHeight];
    float[] porkChopValues = new float[imgWidth * imgHeight];

    Texture2D texture;
    Thread _thread;
    bool _threadRunning = true; //always running until shut down TODO consider making this thread sleep when not triggered
    bool _triggerPork = false; //internal flag, gets oe1, oe2 before triggering porkchop thread
    public static bool triggerPork = false; //external flag to run porkchop plot
    bool porkDone = false;
    bool intercept = true;
    double computeTime;
    public GameObject trajectoryDeltaVTooltip;
    public GameObject shipDeltaVTooltip;
    public GameObject durationTooltip, startTimeTooltip;
    public GameObject startTimeIndicator, travelTimeIndicator;
    public GameObject selectorIndicator;

    double period;
    OrbitalTools.OrbitalElements oe1, oe2;
    public PCPoint minDV;
    float mindv = Mathf.Infinity;
    double mindvStarTime, mindvTravelTime;

    void InitTexture2D(Texture2D texture)
    {
        for (int i = 0; i < imgWidth; i++)
        {
            for (int j = 0; j < imgHeight; j++)
            {
                texture.SetPixel(i, j, MuMech.MuUtils.HSVtoRGB((360f / imgWidth) * i, (1.0f / j) * imgHeight, 1.0f, 1f));
            }
        }
        texture.Apply();
    }
    public void CreatePlot()
    {
        var renderer = GetComponent<SpriteRenderer>();
        texture = new Texture2D(PorkChopPlot.imgWidth, PorkChopPlot.imgHeight, TextureFormat.RGB24, false);

        InitTexture2D(texture);
        
        var sprite = Sprite.Create(texture, new Rect(0,0, PorkChopPlot.imgWidth, PorkChopPlot.imgHeight), new Vector2(0.5f, 0.5f), 40);
        renderer.sprite = sprite;
    }
    void Start () {
        CreatePlot();

        _thread = new Thread(Porkchop);
        _thread.Start();
	}

    // Update is called once per frame
    void Update() {

        //TODO should go in enable pork chop plot?
        if (triggerPork)
        {
            triggerPork = false;
            var src = UXStateManager.GetSource();
            var tgt = UXStateManager.GetTarget();
            Debug.Log("Triggering porkchop!");
            if (src == null || tgt == null)
            {
                Debug.Log("Something's not right, can't run prokchop: src: " + src + " tgt: " + tgt);
                return;
            }
            computeTime = Time.time;
            oe1 = src.GetComponent<OrbitData>().getOE();
            oe2 = tgt.GetComponent<OrbitData>().getOE();
            _triggerPork = true;
        } 

        //TODO every .2 seconds?
	    if (porkDone)
        {
            texture.SetPixels(porkChopColors);
            texture.Apply();
            porkDone = false;
            Debug.Log("Period of ppc: " + period);
            Debug.Log("mindv: " + mindv);
            PlotTrajectory(mindvStarTime, mindvTravelTime);
            //move selector
            MoveSelector(mindvStarTime, mindvTravelTime);
        }
    }
    void MoveSelector(double startTime, double travelTime)
    {
        GameObject selector = GameObject.Find("selector");
        var width = (float)(startTime / period - .5d);
        var height = (float)(travelTime / period - .5d);
        Debug.Log("width: " + width + " height: " + height);
        var newLocation = new Vector3(width, height, selector.transform.localPosition.z);
        Debug.Log("selector pos: " + newLocation.ToString());
        selector.transform.localPosition = newLocation;
    }
    public void DragStartTimeIndicator()
    {
        //mouse only
        //get mouse position
        Debug.Log("dragging start time indicator");
        var camera = GameObject.Find("Camera");
        var cam = camera.GetComponent<Camera>();
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo);
        _DragStartTimeIndicator(hitInfo);
    }
    public void _DragStartTimeIndicator(RaycastHit hitInfo)
    {
        Debug.Log("loc: " + hitInfo.point.ToString());
        //move localPosition.x
        var oldLocalPosition = startTimeIndicator.transform.localPosition;
        startTimeIndicator.transform.position = hitInfo.point;
        var newLocalPosition = oldLocalPosition;
        var localx = startTimeIndicator.transform.localPosition.x;
        if (localx < .5f && localx > -.5)
            newLocalPosition.x = localx;
        startTimeIndicator.transform.localPosition = newLocalPosition;
        var localy = travelTimeIndicator.transform.localPosition.y;
        localx = newLocalPosition.x;

        SelectedTrajectory(new Vector2(localx, localy));
        var pos = selectorIndicator.transform.localPosition;
        pos.x = localx;
        pos.y = localy;
        selectorIndicator.transform.localPosition = pos;
    }
    public void DragTravelTimeIndicator()
    {
        //mouse only
        //get mouse position
        Debug.Log("dragging travel time indicator3");
        var camera = GameObject.Find("Camera");
        var cam = camera.GetComponent<Camera>();
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hitInfo);
        _DragTravelTimeIndicator(hitInfo);
    }
    public void _DragTravelTimeIndicator(RaycastHit hitInfo) { 
        Debug.Log("loc: " + hitInfo.point.ToString());
        //move localPosition.y
        var oldLocalPosition = travelTimeIndicator.transform.localPosition;
        travelTimeIndicator.transform.position = hitInfo.point;
        var newLocalPosition = oldLocalPosition;
        var localy = travelTimeIndicator.transform.localPosition.y;
        if (localy < .35f && localy > -.35)
            newLocalPosition.y = localy;
        travelTimeIndicator.transform.localPosition = newLocalPosition;
        var localx = startTimeIndicator.transform.localPosition.x;
        localy = newLocalPosition.y;

        SelectedTrajectory(new Vector2(localx, localy));
        var pos = selectorIndicator.transform.localPosition;
        pos.x = localx;
        pos.y = localy;
        selectorIndicator.transform.localPosition = pos;
    }
    Vector3d injectionVector;
    double startTime; //absolute time
    //update maneuver node w/ trajectory
    public void SelectedTrajectory(Vector2 coord)
    {
        //convert normalized coord to start times/transit times
        //[-.5,.5] = [0,period]
        Debug.Log("coord: " + coord.ToString());
        startTime = (coord.x + 0.5d) * period; //relative to compute time
        double travelTime = (coord.y + 0.5d) * period;
        PlotTrajectory(startTime, travelTime);
    }
    float time2coord(double time)
    {
        return (float)(time / period - .5d);
    }
    void PlotTrajectory(double startTime, double travelTime)
    {
        var timeSinceCompute = Time.time - computeTime;
        var curStartTime = startTime - timeSinceCompute; //relative to now
        Debug.Log("startTime: " + curStartTime.ToString("G3") + " travelTime: " + travelTime.ToString("G3"));
        //recompute trajectory w/ those times
        Vector3d initVel, finalVel;
        Vector3d r1, v1;
        Vector3d r2, v2;
        if (oe1 == null || oe2 == null) {
            Debug.Log("OE1 or 2 not initialized");
            return;
        }
        {
            var tempOe1 = oe1.copyOE();
            var tempOe2 = oe2.copyOE();
            var tra1 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe1, startTime);
            var tra2 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe2, startTime + travelTime);
            tempOe2.tra = tra2;
            tempOe1.tra = tra1;

            OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe1, out r1, out v1);
            OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe2, out r2, out v2);

            MuMech.LambertSolver.Solve(r1, r2, travelTime, OrbitData.parentGM, true, out initVel, out finalVel);
            injectionVector = initVel;// - v1;
            var rendezvousVector = finalVel - v2;
        }
        //convert initial velocity to oe
        VectorD rv = Util.convertToRv(ref r1, ref initVel);
        var interceptOE = Util.rv2oe(OrbitData.parentGM, rv);
        //initialize/update maneuver node/orbit w/ oe
        var marker1 = GameObject.Find("Marker1");
        marker1.transform.localPosition = r1.ToFloat();
        var marker2 = GameObject.Find("Marker2");
        marker2.transform.localPosition = r2.ToFloat();
        var orbitManager = GameObject.Find("OrbitManager").GetComponent<Orbit>();
        orbitManager.updateInterceptLine(ref interceptOE, true);


        //display required deltaV for intercept
        //TODO based on mode: display required deltaV for rendezvous
        var tooltip = trajectoryDeltaVTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Req dV: " + (initVel-v1).magnitude.ToString("G2");
        tooltip.Reset();
        tooltip = shipDeltaVTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Ship dV: " + UXStateManager.GetSource().GetComponent<OrbitData>().GetDV().ToString("G4");
        tooltip.Reset();

        //display start time/travel time
        tooltip = startTimeTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Start Time: " + (curStartTime).ToString("G4");
        tooltip.Reset();
        tooltip = durationTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Duration: " + travelTime.ToString("G4");
        tooltip.Reset();

        //set scrollers
        var localPos = startTimeIndicator.transform.localPosition;
        localPos.x = time2coord(startTime);
        startTimeIndicator.transform.localPosition = localPos;
//
        localPos = travelTimeIndicator.transform.localPosition;
        localPos.y = time2coord(travelTime);
        travelTimeIndicator.transform.localPosition = localPos;
        //display time of arrival at intersect point
        //display start time at injection point
    }
    public void TriggerIntercept()
    {
        if (computeTime+startTime <= Time.time)
        {
            Debug.Log("Error: Intercept injection in the past");
            return;
        }
        Debug.Log("Trigger intercept event");
        var src = UXStateManager.GetSource();
        var tgt = UXStateManager.GetTarget();
        Debug.Log("InjVec: " + injectionVector);
        var e = new ManeuverEvent(src, tgt, (float)(startTime-Time.time+computeTime), "intercepts", injectionVector);
        Events.instance.Queue(e);
    }

    void OnDestroy()
    {
        if (_threadRunning)
        {
            _threadRunning = false;
            _thread.Join();
        }
    }
    public void EnablePorkChop(bool enableIntercept)
    {
        Debug.Log("Enabled porkchop");
        triggerPork = true;
        intercept = enableIntercept;
    }

    void FindVel(double startTime, double travelTime, out Vector3d injectionVector, out Vector3d rendezvousVector)
    {
        var tempOe1 = oe1.copyOE();
        var tempOe2 = oe2.copyOE();
        var tra1 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe1, startTime);
        var tra2 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe2, startTime + travelTime);
        tempOe2.tra = tra2;
        tempOe1.tra = tra1;

        Vector3d r1, v1;
        OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe1, out r1, out v1);
        Vector3d r2, v2;
        OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe2, out r2, out v2);

        Vector3d initVel, finalVel;
        MuMech.LambertSolver.Solve(r1, r2, travelTime, OrbitData.parentGM, true, out initVel, out finalVel);
        //Debug.Log("initVel " + initVel.magnitude);
        injectionVector = initVel - v1;
        rendezvousVector = finalVel - v2;
    }
    void GeneratePorkChop()
    {
        Debug.Log("Running GeneratePorkChop()");
        float maxHue = 1.0f;
        float minHue = 360.0f;
        //find longest period
        var period1 = oe1.getPeriod();
        var period2 = oe2.getPeriod();
        period = System.Math.Max(period1, period2);

        //plot is always 1 period across and 1/2 period tall
        double startTimeInc = period / imgWidth;
        double travelTimeInc = (period / 2) / imgHeight;

        //starting time
        for (double x = 0; x < imgWidth; x++)
        {
            //travel time
            for (double y = 1; y <= imgHeight; y++)
            {
                var startTime = x * startTimeInc;
                var travelTime = y * travelTimeInc;

                Vector3d injectionVector, rendezvousVector;
                FindVel(startTime, travelTime, out injectionVector, out rendezvousVector);

                int index = (int)(y-1) * (int)imgWidth + (int)x;
                float diffMag = 0;
                if (intercept) 
                    diffMag = (float)injectionVector.magnitude;
                else //rendezvous
                    diffMag = (float)injectionVector.magnitude + (float)rendezvousVector.magnitude;
                porkChopValues[index] = diffMag;
                maxHue = Mathf.Max(maxHue, diffMag);
                minHue = Mathf.Min(minHue, diffMag);
                if (diffMag < mindv)
                {
                    mindv = diffMag;
                    //Debug.Log("new minimum dv found!");
                    //minDV.dv = diffMag;
                    //minDV.startTime = startTime;
                    //minDV.travelTime = travelTime;
                    mindvStarTime = startTime;
                    mindvTravelTime = travelTime;
                }
            }//per row
        }//per column

        //convert to colors
        Debug.Log("Max hue: " + maxHue);

        for (int index = 0; index < imgWidth * imgHeight; index++)
        {
            //porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (porkChopValues[index]), 1f, 1.0f, 1f);
            float value = (porkChopValues[index] - mindv) / (maxHue - mindv);
            porkChopColors[index] = new Color(value, value, value);
        }
    }
    void Porkchop()
    {
        Debug.Log("Porkchop thread started!");
        while (_threadRunning)
        {
            if (_triggerPork)
            {
                _triggerPork = false;
                Debug.Log("in separate thread, detected _triggerPork");
                GeneratePorkChop();
                porkDone = true;
            }
        }
    }
}
