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
        double multiplier = 10;
        double multiplier2 = 40;
        while (_threadRunning)
        {
            if (_triggerPork)
            {
                //starting time
                for (double startTime = 0; startTime < imgWidth; startTime++)
                {
                    //jounrey time, broken into multiple blocks for perf reasons
                    int maxBlock = 30;
                    for (double block = 0; block < maxBlock; block++)
                    {
                        double blockStart = block / maxBlock * imgHeight;
                        blockStart = (blockStart == 0) ? 1 : blockStart;
                        double blockLimit = (block + 1) / maxBlock * imgHeight;
                        for (double travelTime = blockStart; travelTime < blockLimit; travelTime++)
                        {
                            var tra1 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe1, startTime * multiplier);
                            var tra2 = OrbitalTools.Program.anomalyAfterTime(OrbitData.parentGM, oe2, travelTime * multiplier2);
                            oe2.tra = tra2;
                            Vector3d r1 = OrbitalTools.Util.oe2rd(OrbitData.parentGM, oe1);
                            Vector3d r2 = OrbitalTools.Util.oe2rd(OrbitData.parentGM, oe2);

                            Vector3d initVel, finalVel;
                            MuMech.LambertSolver.Solve(r1, r2, travelTime * multiplier2, OrbitData.parentGM, true, out initVel, out finalVel);
                            //Debug.Log("initVel " + initVel.magnitude);

                            var i = (float)startTime;
                            var j = (float)travelTime;
                            int maxHue = 2;
                                int index = (int)j * (int)imgWidth + (int)i;
                            //texture.SetPixel((int)i, (int)j, MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (float)initVel.magnitude, 1f, 1.0f, 1f));
                                porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (float)initVel.magnitude, 1f, 1.0f, 1f);
                            if (j == 1)
                            {
                                j = 0;
                                //texture.SetPixel((int)i, (int)j, MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (float)initVel.magnitude, 1f, 1.0f, 1f));
                                porkChopColors[index] = MuMech.MuUtils.HSVtoRGB((360f / maxHue) * (float)initVel.magnitude, 1f, 1.0f, 1f);
                            }
                        }
                    }
                }
                _triggerPork = false;
                porkDone = true;
            }
        }
    }
}
