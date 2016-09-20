using UnityEngine;
using System.Collections;
using OrbitalTools;
//using System.Threading;

public class simpleOrbit : MonoBehaviour {

    public Color color;
    LineRenderer myLine;

    bool _threadRunning;
    //Thread _thread;

	// Use this for initialization
	void Start () {
        myLine = GetComponent<LineRenderer>();
        var start = transform.position;
        color = Color.green;
        for (int i=0; i<lengthOfLineRenderer; i++)
        {
            myLine.SetPosition(i, transform.position);
        }


        //_thread = new Thread(rk4);
        //_thread.Start();
        _threadRunning = true;
    }
    public static int lengthOfLineRenderer = 360;

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

    Vector3[] tempLine = new Vector3[lengthOfLineRenderer];
    // Update is called once per frame
    int frameCount = 0;
    void Update() {
        //transform.RotateAround(Vector3.zero, Vector3.up,  20*Time.deltaTime);

#if false
        //example
        float t = Time.time;
        int i = 0;
        float scale = 10;
        Vector3 pos = transform.position;
        while (i < lengthOfLineRenderer) {
            pos = RotateAround(Vector3.zero, Vector3.up, i * scale);
            myLine.SetPosition(i, pos);
            i++;
        }
#endif
        myLine.SetColors(Color.green, Color.red);
        myLine.SetPosition(0, transform.position + Orbit.getGlobalVel());
        myLine.SetPosition(1, transform.position);
        //ever 30 frames
        if (frameCount++ > 20)
        {
            updatePath();
            frameCount = 0;
        }
    }
    void updatePath() {
        //shift elements front to back, push current position to first element
        int i = lengthOfLineRenderer - 1;
#if true
        while (i > 1)
        {
            tempLine[i] = tempLine[i - 1];
            i--;
        }
        tempLine[1] = transform.position;
#else
        rk4();
#endif
        //copy over
        for (i=2; i<tempLine.Length; i++)
        {
            myLine.SetPosition(i, tempLine[i]);
        }


    }
    void OnDisable()
    {
        //_thread.Join();
    }
    void rk4() {
        //while(_threadRunning)
        {
            VectorD rv = Util.convertToRv(ref Orbit.Global.pos, ref Orbit.Global.vel);
            double[] accel = new double[3];
            accel[0] = Orbit.accelVector.x;
            accel[1] = Orbit.accelVector.y;
            accel[2] = Orbit.accelVector.z;
            VectorD params_ = Util.convertToParams(new double[3], Orbit.parentGM, accel);

            //just to update Orbit.cs for OE path
            rv = Util.rungeKutta4(0, .0001d, rv, params_);
            Orbit.Global.pos[0] = rv[0];
            Orbit.Global.pos[1] = rv[1];
            Orbit.Global.pos[2] = rv[2];
            Orbit.Global.vel[0] = rv[3];
            Orbit.Global.vel[1] = rv[4];
            Orbit.Global.vel[2] = rv[5];
            //now calculate the rk4 path
            Vector3 position, velocity;

            for (int i = 2; i < lengthOfLineRenderer; i++)
            {
                rv = Util.rungeKutta4(0, 0.1d, rv, params_);
                position.x = (float)rv[0];
                position.y = (float)rv[1];
                position.z = (float)rv[2];
                velocity.x = (float)rv[3];
                velocity.y = (float)rv[4];
                velocity.z = (float)rv[5];
                tempLine[i] = position;
            }
            /*
            //myLine.transform.position = transform.position;
            //lr.SetColors(color, color);
            myLine.SetWidth(0.1f, 0.1f);
            var tempPos = transform.position;
            for (int i=0; i < 10; i++)
            {
                transform2.RotateAround(Vector3.zero, Vector3.up, i);
                myLine.SetPosition(i, transform2.position);
            }
            transform.position = tempPos;
            */
        }
    }
}
