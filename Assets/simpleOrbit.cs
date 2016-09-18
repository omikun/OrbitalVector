using UnityEngine;
using System.Collections;

public class simpleOrbit : MonoBehaviour {

    public Color color;
    LineRenderer myLine;

	// Use this for initialization
	void Start () {
        myLine = GetComponent<LineRenderer>();
        var start = transform.position;
        color = Color.green;
    }
    public static int lengthOfLineRenderer = 40;

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
        myLine.SetPosition(0, transform.position + Orbit.getGlobalVel());
        myLine.SetPosition(1, transform.position);
        //ever 30 frames
        if (frameCount++ > 200)
        {
            updatePath();
            frameCount = 0;
        }
    }
    void updatePath() { 
        //shift elements front to back, push current position to first element
        int i = lengthOfLineRenderer - 1;
        while (i > 1)
        {
            tempLine[i] = tempLine[i - 1];
            i--;
        }
        tempLine[1] = transform.position;
        //copy over
        for (i=2; i<tempLine.Length; i++)
        {
            myLine.SetPosition(i, tempLine[i]);
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
