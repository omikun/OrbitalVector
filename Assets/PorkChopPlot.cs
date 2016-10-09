using UnityEngine;
using System.Collections;
using System.Threading;

public class PorkChopPlot : MonoBehaviour {
    // Use this for initialization
    public static bool triggerPork = false;
    bool _triggerPork = false;
    const int imgWidth = 40;
    const int imgHeight = 30;
    Color[] porkChopColors = new Color[imgWidth * imgHeight];
    float[] porkChopValues = new float[imgWidth * imgHeight];
    Texture2D texture;
    OrbitalTools.OrbitalElements oe1, oe2;
    Thread _thread;
    bool _threadRunning = true;

    void Start () {
        var renderer = GetComponent<SpriteRenderer>();
        texture = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
        /*
        for (int i = 0; i < imgWidth * imgHeight; i++)
        {
            colors[i] = (i < imgWidth * imgHeight / 2) ? Color.blue : Color.red;
        }
        texture.SetPixels(colors);
*/

        var textureWidth = imgWidth;
        var textureHeight = imgHeight;
        for (int i = 0; i < textureWidth; i++)
        {
            for (int j = 0; j < textureHeight; j++)
            {
                texture.SetPixel(i, j, MuMech.MuUtils.HSVtoRGB((360f / textureWidth) * i, (1.0f / j) * textureHeight, 1.0f, 1f));
            }
        }
        texture.Apply();
        var sprite = Sprite.Create(texture, new Rect(0,0,imgWidth, imgHeight), new Vector2(0.5f, 0.5f), 40);
        renderer.sprite = sprite;

        _thread = new Thread(porkchop);
        _thread.Start();
	}

    bool porkDone = false;
    // Update is called once per frame
    void Update() {

        if (triggerPork)
        {
            oe1 = GameObject.Find("ship_test1").GetComponent<OrbitData>().getOE();
            oe2 = GameObject.Find("ship_test2").GetComponent<OrbitData>().getOE();
            triggerPork = false;
            _triggerPork = true;
        }
	    if (porkDone)
        {
            texture.SetPixels(porkChopColors);
            texture.Apply();
            porkDone = false;
            //porkchop();
            //StartCoroutine("porkchop");

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
    void porkchop()
    {
        double multiplier = 1d;
        double multiplier2 = 1d;
        while (_threadRunning)
        {
            if (_triggerPork)
            {
                float maxHue = 1.0f;
                float minHue = 360.0f;
                //starting time
                for (double x = 0; x < imgWidth; x++)
                {
                    //travel time
                    for (double y = 1; y < imgHeight; y++)
                    {
                        var startTime = x * multiplier;
                        var travelTime = y * multiplier;
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
                        //Vector3d r1 = OrbitalTools.Util.oe2rd(OrbitData.parentGM, oe1);
                        //Vector3d r2 = OrbitalTools.Util.oe2rd(OrbitData.parentGM, oe2);

                        Vector3d initVel, finalVel;
                        MuMech.LambertSolver.Solve(r1, r2, y * multiplier2, OrbitData.parentGM, true, out initVel, out finalVel);
                        //Debug.Log("initVel " + initVel.magnitude);

                        int index = (int)(y) * (int)imgWidth + (int)x ;
                        var diffMag = (float)(initVel - v1).magnitude + (float)(finalVel - v2).magnitude;
                        //diffMag = (float)initVel.magnitude;
                        //Debug.Log("diffMag: " + diffMag);
                        //texture.SetPixel((int)i, (int)j, MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (float)initVel.magnitude, 1f, 1.0f, 1f));
                        porkChopValues[index] = diffMag;
                        maxHue = Mathf.Max(maxHue, diffMag);
                        minHue = Mathf.Min(minHue, diffMag);
                    }//per row
                }//per column
                //porkchop values computed
                //convert to colors
                Debug.Log("Max hue: " + maxHue);
                for (int index = 0; index < imgWidth*imgHeight; index++)
                {
                    var itIndex = (index < imgWidth) ? index + imgWidth : index;
                    porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (porkChopValues[itIndex]), 1f, 1.0f, 1f);
                    //Orbit.output.Add(porkChopValues[itIndex].ToString() + ",");
                    //if ((index+1) % (imgWidth) == 0)
                    //{
                    //    Orbit.output.Add("\n");
                    //}
                }
                //Orbit.Savecsv();
                _triggerPork = false;
                porkDone = true;
            }
        }
    }
}
