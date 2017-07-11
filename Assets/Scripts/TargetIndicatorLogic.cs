using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetIndicatorLogic : MonoBehaviour {
    public GameObject camera;
    public GameObject target;
    public GameObject marker;
    public GameObject lineTop, lineRight;

    public GameObject topBound, bottomBound, leftBound, rightBound;
    GameObject canvas;
    LineRenderer line;
    LineRenderer debugTop, debugRight;
	// Use this for initialization
	void Start () {
        line = GetComponent<LineRenderer>();
        canvas = topBound.transform.parent.gameObject;
        debugTop = lineTop.GetComponent<LineRenderer>();
        debugRight = lineRight.GetComponent<LineRenderer>();

	}
	
	// Update is called once per frame
	void Update () {
		Vector3 v = target.transform.position - camera.transform.position;
        Vector3 d = Vector3.Project(v, camera.transform.forward);
        Vector3 projectedPoint = target.transform.position - d;
        marker.transform.position = projectedPoint;
        line.SetPosition(0, camera.transform.position);
        line.SetPosition(1, projectedPoint);
        //transform.position = 
        FindInterSection(projectedPoint);
    }
    void FindInterSection(Vector3 point)
    {
        debugTop.SetPosition(0, Vector3.zero);
        debugRight.SetPosition(0, Vector3.zero);
        debugTop.SetPosition(1, canvas.transform.up);
        debugRight.SetPosition(1, canvas.transform.right);

        var topPlane = new Plane(canvas.transform.up, topBound.transform.position);
        var bottomPlane = new Plane(canvas.transform.up, bottomBound.transform.position);
        var leftPlane = new Plane(canvas.transform.right, leftBound.transform.position);
        var rightPlane = new Plane(canvas.transform.right, rightBound.transform.position);

        float[] dists = new float[4];
        Vector3[] hits = new Vector3[4];

        int i = 0;
        hits[i] = HitPlane(point, topPlane, out dists[i]);
        i++; hits[i] = HitPlane(point, bottomPlane, out dists[i]);
        i++; hits[i] = HitPlane(point, leftPlane, out dists[i]);
        i++; hits[i] = HitPlane(point, rightPlane, out dists[i]);

        float minDist = float.PositiveInfinity;
        Vector3 hitPoint = hits[0];
        for (int j = 0; j < 4; j++)
        {
            if (dists[j] < minDist && hits[j] != Vector3.zero)
            {
                hitPoint = hits[j];     //use the smallest distance not zero
                minDist = dists[j];
                Debug.Log("found min: " + j);
            }
        }
        Debug.Log("top: " + dists[0] + " bot: " + dists[1] + " left: " + dists[2] + " right: " + dists[3]);
        Debug.Log("top: " + hits[0] + " bot: " + hits[1] + " left: " + hits[2] + " right: " + hits[3]);
        marker.transform.position = hitPoint;
    }
    Vector3 HitPlane(Vector3 point, Plane plane, out float rayDistance)
    {
        Vector3 intersectPoint = Vector3.zero;
        var ray = new Ray(canvas.transform.position, point - canvas.transform.position);
        if (plane.Raycast(ray, out rayDistance))
        {
            intersectPoint = ray.GetPoint(rayDistance);
        }
        return intersectPoint;
    }
}
