using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetIndicatorLogic : MonoBehaviour {
    public GameObject camera;
    public GameObject target;

    public GameObject topBound, bottomBound, leftBound, rightBound;
    GameObject canvas;
	// Use this for initialization
	void Start () {
        canvas = topBound.transform.parent.gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
		Vector3 v = target.transform.position - camera.transform.position;
        Vector3 d = Vector3.Project(v, camera.transform.forward);
        Vector3 projectedPoint = target.transform.position - d;
        //transform.position = 
        //if target not in view
        if (IsInViewFrustrum(target))
        {
            var distToCamera = canvas.transform.position - camera.transform.position;
            var targetIconPos = distToCamera.magnitude * (target.transform.position - camera.transform.position).normalized;
            transform.position = camera.transform.position + targetIconPos;
        } else
        {
            FindInterSection(projectedPoint);
        }
    }
    Plane[] planes;
    bool IsInViewFrustrum(GameObject target)
    {
        bool ret = false;
        planes = GeometryUtility.CalculateFrustumPlanes(camera.GetComponent<Camera>());
        Collider objCol = target.GetComponent<Collider>();
        return GeometryUtility.TestPlanesAABB(planes, objCol.bounds);
    }
    void FindInterSection(Vector3 point)
    {

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
            }
        }
        transform.position = hitPoint;
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
