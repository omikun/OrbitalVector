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
        if (GUILayout.Button("Build Object"))
        {
            pcp.CreatePlot();
        }
    }
}
public class PorkChopPlot : MonoBehaviour
{
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
    public GameObject trajectoryDeltaVTooltip;
    public GameObject shipDeltaVTooltip;
    public GameObject durationTooltip, startTimeTooltip;
    public GameObject startTimeIndicator, travelTimeIndicator;
    public GameObject selectorIndicator;
    float availDV = 5;
    double period;
    double mComputeTime = 0;
    OrbitalTools.OrbitalElements oe1, oe2;
    public PCPoint minDV;
    float mindv = Mathf.Infinity;
    double mMinDVStartTime, mMinDVTravelTime;

    Vector3d mInjectionVector, mMinRendezvousVector;
    double mStartTime, mTravelTime; //absolute time
    EventManager eventManager;
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

        var sprite = Sprite.Create(texture, new Rect(0, 0, PorkChopPlot.imgWidth, PorkChopPlot.imgHeight), new Vector2(0.5f, 0.5f), 40);
        renderer.sprite = sprite;
    }
    void Start()
    {
        CreatePlot();

        _thread = new Thread(Porkchop);
        _thread.Start();
        eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
    }

    bool GetOEs() //returns true valid src/tgt
    {
        var src = UXStateManager.GetSource();
        var tgt = UXStateManager.GetTarget();
        if (src == null || tgt == null)
        {
            Debug.Log("Something's not right, can't run prokchop: src: " + src + " tgt: " + tgt);
            return false;
        }
        oe1 = src.GetComponent<OrbitData>().GetOE();
        oe2 = tgt.GetComponent<OrbitData>().GetOE();
        return true;
    }
    // Update is called once per frame
    void Update()
    {

        //TODO should go in enable pork chop plot?
        if (triggerPork)
        {
            triggerPork = false;
            Debug.Log("Triggering porkchop!");
            if (!GetOEs()) { return; }

            _triggerPork = true;
        }

        //TODO every .2 seconds?
        if (porkDone)
        {
            texture.SetPixels(porkChopColors);
            texture.Apply();
            porkDone = false;
            Debug.Log("Period of ppc: " + period);
            PlotTrajectory(mMinDVStartTime, mMinDVTravelTime);
            //move selector
            MoveSelector(mMinDVStartTime, mMinDVTravelTime);
            mStartTime = mMinDVStartTime;
            mTravelTime = mMinDVTravelTime;
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
    public void _DragTravelTimeIndicator(RaycastHit hitInfo)
    {
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
    //update maneuver node w/ trajectory
    public void SelectedTrajectory(Vector2 coord)
    {
        //convert normalized coord to start times/transit times
        //[-.5,.5] = [0,period]
        Debug.Log("coord: " + coord.ToString());
        mStartTime = (coord.x + 0.5d) * period; //relative to compute time
        mTravelTime = (coord.y + 0.5d) * period;
        PlotTrajectory(mStartTime, mTravelTime);
    }
    float time2coord(double time)
    {
        return (float)(time / period - .5d);
    }
    void testMarker(string markername, double startTime)
    {
        //return;// don't show extra markers of start path
        Vector3d r1, v1;
        FindRV(oe1, startTime, out r1, out v1);

        var marker = GameObject.Find(markername);
        marker.transform.localPosition = r1.ToFloat() * HoloManager.SimZoomScale;
        Debug.Log(markername + " " + OVTools.FormatTime((float)startTime));
    }
    //times are relative
    bool PlotTrajectory(double relStartTime, double relTravelTime)
    {
        //if (!GetOEs()) { return false; } //now OEs are at current time

        var curStartTime = 0;//relStartTime + mComputeTime - eventManager.GetSimTime(); //relative to now
        Debug.Log(//"curStartTime: " + OVTools.FormatTime((float)curStartTime)
            " startTime: " + OVTools.FormatTime((float)relStartTime)
        + " travelTime: " + OVTools.FormatTime((float)relTravelTime));
        Debug.Log("injection time: " + (relStartTime+mComputeTime).ToString());
        //recompute trajectory w/ those times
        Vector3d initVel, finalVel;
        Vector3d r1, v1;
        Vector3d r2, v2;

        FindVel(relStartTime + mComputeTime, relTravelTime, out initVel, out finalVel, 
            out r1, out r2, out v1, out v2);
            
        OVDebug.projectedR1 = r1;
        Debug.Log("Injection pos: " + r1.ToString());
        mInjectionVector = initVel;// - v1;
        var rendezvousVector = finalVel - v2;
        
        //convert initial velocity to oe
        VectorD rv = Util.convertToRv(ref r1, ref initVel);
        var interceptOE = Util.rv2oe(OrbitData.parentGM, rv);
        //initialize/update maneuver node/orbit w/ oe
        ////tests/////////////////////////
        //testMarker("Marker1-2", startTime);
        //testMarker("Marker1-3", curStartTime + mComputeTime);
       // testMarker("Marker1-4", eventManager.GetSimTime());
       // testMarker("Marker1-5", startTime - eventManager.GetSimTime());
        
        var marker1 = GameObject.Find("Marker1");
        marker1.transform.localPosition = r1.ToFloat() * HoloManager.SimZoomScale;
        var marker2 = GameObject.Find("Marker2");
        marker2.transform.localPosition = r2.ToFloat() * HoloManager.SimZoomScale;
        var orbitManager = GameObject.Find("OrbitManager").GetComponent<Orbit>();
        orbitManager.updateInterceptLine(ref interceptOE, true);


        //display required deltaV for intercept
        //TODO based on mode: display required deltaV for rendezvous
        var tooltip = trajectoryDeltaVTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Req dV: " + OVTools.FormatDistance((float)(initVel - v1).magnitude);//.ToString("G2");
        tooltip.Reset();
        tooltip = shipDeltaVTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Ship dV: " + OVTools.FormatDistance(UXStateManager.GetSource().GetComponent<OrbitData>().GetDV());//.ToString("G4");
        tooltip.Reset();

        //display start time/travel time
        tooltip = startTimeTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Start Time: " + OVTools.FormatTime((float)(relStartTime + mComputeTime));//.ToString("G4");
        //tooltip.displayText = "Start Time: " + OVTools.FormatTime((float)curStartTime);//.ToString("G4");
        tooltip.Reset();
        tooltip = durationTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Duration: " + OVTools.FormatTime((float)relTravelTime);//.ToString("G4");
        tooltip.Reset();

        //set scrollers
        var localPos = startTimeIndicator.transform.localPosition;
        localPos.x = time2coord(relStartTime);
        startTimeIndicator.transform.localPosition = localPos;
        //
        localPos = travelTimeIndicator.transform.localPosition;
        localPos.y = time2coord(relTravelTime);
        travelTimeIndicator.transform.localPosition = localPos;
        //display time of arrival at intersect point
        //display start time at injection point
        return true;
    }
    public void TriggerIntercept()
    {
        if (mStartTime + mComputeTime <= eventManager.GetSimTime())
        {
            Debug.Log("Error: Intercept injection in the past");
            return;
        }
        Debug.Log("Trigger intercept event");
        var src = UXStateManager.GetSource();
        var tgt = UXStateManager.GetTarget();
        Debug.Log("InjVec: " + mInjectionVector.ToString());
        var e = new ManeuverEvent(src, tgt,
            (float)(mStartTime+mComputeTime), (float)mTravelTime,
            "intercepts", mInjectionVector);
        //Events.instance.Queue(e); //deprecated, should be made illegal
        eventManager.Queue(e);
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
        gameObject.SetActive(true);
        triggerPork = true;
        availDV = GetAvailableDV();
        Debug.Log("availDV: " + availDV);
        intercept = enableIntercept;
        Debug.Log("Enable porkchop, simtime: " + OVTools.FormatTime((float)eventManager.GetSimTime()));
    }

    //assumes time is absolute
    void FindRV(OrbitalElements oe, double absTime, out Vector3d r, out Vector3d v)
    {
        var tempOe = oe.CopyOE();
        //Note: SHOULD be oe.computeTime, not mComputeTime
        tempOe.tra = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe, absTime-oe.computeTime);
        OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe, out r, out v);
    }
    /*
     * takes relative absolute time startTime and relative travel time
     */
    void FindVel(double absStartTime, double relTravelTime, 
        out Vector3d initVel, out Vector3d finalVel,
        out Vector3d r1, out Vector3d r2,
        out Vector3d v1, out Vector3d v2)
    {
        //get position/velocity of source object at start time
        //and pos/vel of target object at end time
        FindRV(oe1, absStartTime, out r1, out v1);
        FindRV(oe2, absStartTime+relTravelTime, out r2, out v2);

        MuMech.LambertSolver.Solve(r1, r2, relTravelTime, OrbitData.parentGM, true, out initVel, out finalVel);
        //Debug.Log("initVel " + initVel.magnitude);
    }
    /**
     * Creates a 2D matrix of dV, relative to start time and travel time
     * start time relative to current sim time
     * travel time relative to start time
     * HOWEVER: time given to FindVel must be in relative time

     */
    void GeneratePorkChop()
    {
        Debug.Log("Running GeneratePorkChop()");
        float maxHue = 1.0f;
        float minHue = 360.0f;
        //find longest period
        var period1 = oe1.GetPeriod();
        var period2 = oe2.GetPeriod();
        Debug.Log("Period1: " + OVTools.FormatTime((float)period1));
        Debug.Log("Period2: " + OVTools.FormatTime((float)period2));
        period = System.Math.Max(period1, period2);

        //plot is always 1 period across and 1/2 period tall
        double startTimeInc = period / imgWidth;
        double travelTimeInc = (period ) / imgHeight;

        mComputeTime = eventManager.GetSimTime();
        Debug.Log("Compute time: " + mComputeTime);
        //starting time
        for (double x = 0; x < imgWidth; x++)
        {
            //travel time
            for (double y = 1; y <= imgHeight; y++)
            {
                var startTime = x * startTimeInc;
                var travelTime = y * travelTimeInc;

                Vector3d r1, r2, v1, v2;
                Vector3d injectionVector, rendezvousVector;
                FindVel(startTime+mComputeTime, travelTime, out injectionVector, out rendezvousVector, out r1, out r2, out v1, out v2);
                injectionVector -= v1;
                rendezvousVector -= v2;

                int index = (int)(y - 1) * (int)imgWidth + (int)x;
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
                    mMinDVStartTime = startTime;
                    mMinDVTravelTime = travelTime;
                    if (!intercept)
                    {
                        //record velocity match burn 
                        //burn = target.velocity - current.velocity
                        mMinRendezvousVector = rendezvousVector;
                    }
                }
            }//per row
        }//per column

        //convert to colors
        //mMinDVStartTime += mComputeTime;
        Debug.Log("Max hue: " + maxHue + " availDV: " + availDV);
        Debug.Log("MinDV StartTime (abs): " + OVTools.FormatTime((float)(mComputeTime + mMinDVStartTime)));

        for (int index = 0; index < imgWidth * imgHeight; index++)
        {
            if (false)//porkChopValues[index] > availDV)
            {
                porkChopColors[index] = Color.black;
            }
            else
            {
                float value = (porkChopValues[index] - mindv) / (maxHue - mindv); //black/white
                porkChopColors[index] = new Color(value, value, value);
                porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (porkChopValues[index]), 1f, 1.0f, 1f);
            }
        }
    }
    float GetAvailableDV()
    {
        var src = UXStateManager.GetSource();
        if (src == null)
        {
            Debug.Log("null src!");
            return -1;
        }
        InteractableShip iship = src.GetComponent<InteractableShip>();
        GameObject fs = iship.functionalShip;
        return fs.GetComponent<FunctionalShip>().GetDV();
        //return .5f;
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
