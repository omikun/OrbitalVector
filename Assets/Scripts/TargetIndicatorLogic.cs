using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public partial class TargetIndicatorLogic : MonoBehaviour {
    public GameObject camera;
    public GameObject target; //this needs to be set on each new object
    public GameObject center; //center of player (origin)

    public GameObject squareIcon;
    //screen space bounds for arrow icon
    public GameObject topBound, bottomBound, leftBound, rightBound;
    public GameObject arrowIcon;
    Plane[] planes;
    GameObject canvas;
    SpriteRenderer squareIconSR;

    public GameObject selectedTargetIcon;
    SpriteRenderer selectedIconSR;

    public GameObject lockedIcon;
    SpriteRenderer lockedIconSR;

    //icon for sphere UI
    public GameObject sphericalTargetIcon; //sti
    SpriteRenderer sphericalIconSR;
    //base if spherical target icon
    public GameObject stiBase;
    SpriteRenderer sphericalIconBaseSR;
    LineRenderer sphericalIconLR; //line extending from sti to stiBase

    public AudioClip lockedAudio;
    public AudioClip unlockedAudio; //falling out of fov
    public AudioClip lockingAudio;
    public AudioClip selectedAudio;
    public AudioClip inFOVAudio;
    AudioSource audio;
    void Start () {
        audio = GetComponent<AudioSource>();
        canvas = topBound.transform.parent.gameObject;
        squareIconSR = squareIcon.GetComponent<SpriteRenderer>();
        lockedIconSR = lockedIcon.GetComponent<SpriteRenderer>();
        selectedIconSR = selectedTargetIcon.GetComponent<SpriteRenderer>();
        sphericalIconLR = sphericalTargetIcon.GetComponent<LineRenderer>();
        sphericalIconSR = sphericalTargetIcon.GetComponent<SpriteRenderer>();
        sphericalIconBaseSR = stiBase.GetComponent<SpriteRenderer>();
        NextState = InitialState;
    }

    void SetSphericalTargetIcon()
    {
        var basePos = target.transform.position.normalized * 2;
        var pos = basePos * 1.1f;
        sphericalTargetIcon.transform.position = pos;
        sphericalIconLR.SetPosition(0, pos);
        sphericalIconLR.SetPosition(1, basePos);
        stiBase.transform.position = basePos;
        stiBase.transform.forward = pos;
    }
    // Update is called once per frame
    float oldAngle = 0;
    //know when a target is first locked
    float BeginTargetLockTime = 0;
    public float TimeToLock = 2f;

    void FixedUpdate ()
    {
        SetSphericalTargetIcon();
        SetSquareAndArrowIcon();

    }

    private void SetSquareAndArrowIcon()
    {
        Vector3 v = target.transform.position - camera.transform.position;
        Vector3 d = Vector3.Project(v, camera.transform.forward);
        Vector3 projectedPoint = target.transform.position - d;
        if (IsInViewFrustrum(target))
        {
            squareIconSR.enabled = true;
            arrowIcon.SetActive(false);
            var distToCamera = canvas.transform.position - camera.transform.position;
            var targetIconPos = distToCamera.magnitude * (target.transform.position - camera.transform.position).normalized;
            squareIcon.transform.position = camera.transform.position + targetIconPos;
            TargetLockFSM();
        }
        else
        {
            selectedIconSR.enabled = false;
            squareIconSR.enabled = false;
            arrowIcon.SetActive(true);
            FindInterSection(projectedPoint);

            var angle = AngleSigned(transform.up, projectedPoint, transform.forward);
            var diffAngle = angle - oldAngle;
            oldAngle = angle;
            var localRot = arrowIcon.transform.localRotation;
            arrowIcon.transform.localRotation *= Quaternion.Euler(0, 0, diffAngle);
        }
    }


    bool IsInFOV(GameObject target)
    {
        float minAngle = 20;
        var targetDir = target.transform.position - camera.transform.position;
        var angle = Vector3.Angle(targetDir, camera.transform.forward);
        return (angle < minAngle);
    }
    float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
    bool IsInViewFrustrum(GameObject target)
    {
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