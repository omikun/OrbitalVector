using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetIndicatorLogic : MonoBehaviour {
    public GameObject camera;
    public GameObject target; //this needs to be set on each new object
    public GameObject center; //center of player (origin)

    public GameObject squareIcon;
    //screen space bounds for arrow icon
    public GameObject topBound, bottomBound, leftBound, rightBound;
    public GameObject arrowIcon;
    Plane[] planes;
    GameObject canvas;
    SpriteRenderer sr;

    //icon for sphere UI
    public GameObject sphericalTargetIcon; //sti
    //base if spherical target icon
    public GameObject stiBase;
    LineRenderer stiLR; //line extending from sti to stiBase

	void Start () {
        canvas = topBound.transform.parent.gameObject;
        sr = squareIcon.GetComponent<SpriteRenderer>();
        stiLR = sphericalTargetIcon.GetComponent<LineRenderer>();
	}
    void SetSphericalTargetIcon()
    {
        var basePos = target.transform.position.normalized * 2;
        var pos = basePos * 1.1f;
        sphericalTargetIcon.transform.position = pos;
        stiLR.SetPosition(0, pos);
        stiLR.SetPosition(1, basePos);
        stiBase.transform.position = basePos;
        stiBase.transform.forward = pos;

    }
    // Update is called once per frame
    float oldAngle = 0;
	void FixedUpdate () {

        //target = UXStateManager.GetTarget();
        if (target == null)
        {
            sr.enabled = false;
            return;
        }
        sr.enabled = true;
        SetSphericalTargetIcon();
		Vector3 v = target.transform.position - camera.transform.position;
        Vector3 d = Vector3.Project(v, camera.transform.forward);
        Vector3 projectedPoint = target.transform.position - d;
        if (IsInViewFrustrum(target))
        {
            sr.enabled = true;
            arrowIcon.SetActive(false);
            var distToCamera = canvas.transform.position - camera.transform.position;
            var targetIconPos = distToCamera.magnitude * (target.transform.position - camera.transform.position).normalized;
            squareIcon.transform.position = camera.transform.position + targetIconPos;
        } else
        {
            sr.enabled = false;
            arrowIcon.SetActive(true);
            FindInterSection(projectedPoint);
            if (false) //TODO simpler method, but can't get it to work atm
            {
                //arrowIcon.transform.up = (projectedPoint).normalized;
                //arrowIcon.transform.up = (target.transform.position - transform.position).normalized;
                //arrowIcon.transform.forward = camera.transform.position;
                //var angle = Vector3.Angle(transform.up, projectedPoint);
                arrowIcon.transform.rotation.SetLookRotation(camera.transform.forward, projectedPoint - transform.position);
                //forward.transform.localPosition = camera.transform.forward.normalized * 18;
                //up.transform.localPosition = (projectedPoint - transform.position).normalized * 18;
            }
            else
            {
                var angle = AngleSigned(transform.up, projectedPoint, transform.forward);
                var diffAngle = angle - oldAngle;
                oldAngle = angle;
                var localRot = arrowIcon.transform.localRotation;
                arrowIcon.transform.localRotation *= Quaternion.Euler(0, 0, diffAngle);

                //arrowIcon.transform.Rotate(camera.transform.forward.normalized, diffAngle);

                //arrowIcon.transform.rotation.SetLookRotation(camera.transform.position, projectedPoint);
                //arrowIcon.transform.rotation.SetFromToRotation(Vector3.up, projectedPoint.normalized);
                //Logdump.Log("angle: " + diffAngle);
            }
        }
    }
    float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
    bool IsInViewFrustrum(GameObject target)
    {
        bool ret = false;
        planes = GeometryUtility.CalculateFrustumPlanes(camera.GetComponent<Camera>());
        Collider objCol = target.GetComponent<Collider>();
        return GeometryUtility.TestPlanesAABB(planes, objCol.bounds);
    }
    //find bounds of the screen
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
        squareIcon.transform.position = hitPoint;
    }
    Vector3 HitPlane(Vector3 point, Plane plane, out float rayDistance)
    {
        Vector3 intersectPoint = Vector3.zero;
        var ray = new Ray(canvas.transform.position, point);// - canvas.transform.position);
        if (plane.Raycast(ray, out rayDistance))
        {
            intersectPoint = ray.GetPoint(rayDistance);
        }
        return intersectPoint;
    }
}
