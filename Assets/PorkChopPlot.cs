using UnityEngine;
using System.Collections;
using System.Threading;
using OrbitalTools;
public class PCPoint
    {
        public float dv = Mathf.Infinity;
        public double startTime, travelTime;
        PCPoint() { dv = Mathf.Infinity; }
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
    void CreatePlot()
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
            if (src == null || tgt == null)
            {
                Debug.Log("src: " + src + " tgt: " + tgt);
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

    Vector3d injectionVector;
    double startTime; //absolute time
    //update maneuver node w/ trajectory
    public void SelectedTrajectory(Vector2 coord)
    {
        //convert normalized coord to start times/transit times
        //[-.5,.5] = [0,period]
        Debug.Log("coord: " + coord);
        startTime = (coord.x + 0.5d) * period; //relative to compute time
        double travelTime = (coord.y + 0.5d) * period;
        PlotTrajectory(startTime, travelTime);
    }
    void PlotTrajectory(double startTime, double travelTime)
    {
        var timeSinceCompute = Time.time + computeTime;
        var curStartTime = startTime - timeSinceCompute; //relative to now
        Debug.Log("startTime: " + curStartTime.ToString("G3") + " travelTime: " + travelTime.ToString("G3"));
        //recompute trajectory w/ those times
        Vector3d initVel, finalVel;
        Vector3d r1, v1;
        Vector3d r2, v2;
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
        tooltip.displayText = "Ship dV: " + UXStateManager.GetSource().GetComponent<OrbitData>().GetDV().ToString("G2");
        tooltip.Reset();

        //display start time/travel time
        tooltip = startTimeTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Start Time: " + (curStartTime).ToString("G2");
        tooltip.Reset();
        tooltip = durationTooltip.GetComponent<Tooltip>();
        tooltip.displayText = "Duration: " + travelTime.ToString("G2");
        tooltip.Reset();

        //display time of arrival at intersect point
        //display start time at injection point
    }
    public void TriggerIntercept()
    {
        if (startTime <= Time.time)
        {
            Debug.Log("Error: Intercept injection in the past");
            return;
        }
        var src = UXStateManager.GetSource();
        Debug.Log("InjVec: " + injectionVector);
        Events.instance.Raise(new ManeuverEvent(injectionVector, (startTime-Time.time+computeTime), src));
    }

    void OnDisable()
    {
        if (_threadRunning)
        {
            _threadRunning = false;
            _thread.Join();
        }
    }
    public void EnablePorkChop(bool enableIntercept)
    {
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
            porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (porkChopValues[index]), 1f, 1.0f, 1f);
        }
    }
    void Porkchop()
    {
        while (_threadRunning)
        {
            if (_triggerPork)
            {
                _triggerPork = false;

                GeneratePorkChop();
                porkDone = true;
            }
        }
    }
}
