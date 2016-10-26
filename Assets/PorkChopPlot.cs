using UnityEngine;
using System.Collections;
using System.Threading;

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
	    if (porkDone)
        {
            texture.SetPixels(porkChopColors);
            texture.Apply();
            porkDone = false;
        }
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
    void GeneratePorkChop()
    {
        double multiplier = 2d;
        double multiplier2 = 2d;
        float maxHue = 1.0f;
        float minHue = 360.0f;
        //starting time
        for (double x = 0; x < imgWidth; x++)
        {
            //travel time
            for (double y = 1; y <= imgHeight; y++)
            {
                var startTime = x * multiplier;
                var travelTime = y * multiplier2;
                var tra1 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe1, startTime);
                var tra2 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe2, startTime + travelTime);
                var tempOe1 = oe1.copyOE();
                var tempOe2 = oe2.copyOE();
                tempOe2.tra = tra2;
                tempOe1.tra = tra1;

                Vector3d r1, v1;
                OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe1, out r1, out v1);
                Vector3d r2, v2;
                OrbitalTools.Util.oe2rv(OrbitData.parentGM, tempOe2, out r2, out v2);

                Vector3d initVel, finalVel;
                MuMech.LambertSolver.Solve(r1, r2, y * multiplier2, OrbitData.parentGM, true, out initVel, out finalVel);
                //Debug.Log("initVel " + initVel.magnitude);

                int index = (int)(y-1) * (int)imgWidth + (int)x;
                float diffMag = 0;
                if (intercept) 
                    diffMag = (float)(initVel - v1).magnitude;
                else //rendezvous
                    diffMag = (float)(initVel - v1).magnitude + (float)(finalVel - v2).magnitude;
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
