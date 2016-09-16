using UnityEngine;
using System.Collections;

public class simpleOrbit : MonoBehaviour {

    public Color color;
    public Transform transform2;
    LineRenderer myLine;

	// Use this for initialization
	void Start () {
        myLine = GetComponent<LineRenderer>();
        var start = transform.position;
        color = Color.green;
        transform2 = transform;
    }
    public int lengthOfLineRenderer = 20;

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

    // Update is called once per frame
    void Update () {
	    //transform.RotateAround(Vector3.zero, Vector3.up,  20*Time.deltaTime);

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
