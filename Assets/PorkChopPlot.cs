using UnityEngine;
using System.Collections;
using System.Threading;
using OrbitalTools;

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

    double period;
    OrbitalTools.OrbitalElements oe1, oe2;

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
                return;
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
        }
    }

    //update maneuver node w/ trajectory
    public void SelectedTrajectory(Vector3 coord)
    {
        //convert normalized coord to start times/transit times
        //[-.5,.5] = [0,period]
        double startTime = (coord.x + 0.5d) * period;
        double travelTime = (coord.y + 0.5d) * period;
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
            var injectionVector = initVel - v1;
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
        //display time of arrival at intersect point
        //display time of arrival at intersect point
        //display start time at start node
        //display required deltaV for intercept
        //based on mode: display required deltaV for rendezvous
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
