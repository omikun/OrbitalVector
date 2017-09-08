using System.Collections;
using System.Collections.Generic;
using System;
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

    public GameObject selectedTargetIcon;
    SpriteRenderer stsr;

    public GameObject lockedIcon;
    SpriteRenderer lsr;

    //icon for sphere UI
    public GameObject sphericalTargetIcon; //sti
    SpriteRenderer ssr;
    //base if spherical target icon
    public GameObject stiBase;
    SpriteRenderer sbsr;
    LineRenderer stiLR; //line extending from sti to stiBase

    public AudioClip lockedAudio;
    public AudioClip unlockedAudio; //falling out of fov
    public AudioClip lockingAudio;
    public AudioClip selectedAudio;
    public AudioClip inFOVAudio;
    AudioSource audio;
	void Start () {
        audio = GetComponent<AudioSource>();
        canvas = topBound.transform.parent.gameObject;
        sr = squareIcon.GetComponent<SpriteRenderer>();
        lsr = lockedIcon.GetComponent<SpriteRenderer>();
        stsr = selectedTargetIcon.GetComponent<SpriteRenderer>();
        stiLR = sphericalTargetIcon.GetComponent<LineRenderer>();
        ssr = sphericalTargetIcon.GetComponent<SpriteRenderer>();
        sbsr = stiBase.GetComponent<SpriteRenderer>();
        NextState = UnselectedState;
	}

    /// <summary>
    /// unselected -> selected
    /// selected -> locking
    /// locking -> selected OR unselected OR locked
    /// locked -> selected OR unselected
    /// </summary>
    //state delegates in FSM
    Action NextState = null;
    void InitialState()
    {

    }
    void GoneState()
    {
        NextState = EndState;
    }
    void EndState()
    {
        return;
    }
    void UnselectedState()
    {
        if (target == UXStateManager.GetTarget())
        {
            NextState = SelectedState;
            Debug.Log("state: selected");
        } else
        {
        }
    }
    float BeginLocking = 0;
    void SelectedState()
    {
        if (IsInFOV(target))
        {
            if (target != UXStateManager.GetTarget())
            {
                NextState = UnselectedState;
                Debug.Log("state: UNselected");
            }
            else
            {
                //locking
                BeginLocking = Time.time;
                NextState = LockingState;
                Debug.Log(name + "state: locking!");
            }
        } else
        {
            //regular selected state animation
        }
    }
    void LockingState()
    {
        if (Time.time > BeginLocking + 2f)
        {
            NextState = LockedState;
            Debug.Log(name + "state: locked");
        }
        if (!IsInFOV(target))
        {
            NextState = SelectedState;
            Debug.Log("state: selected");
        }
        if (target != UXStateManager.GetTarget()) {
            NextState = UnselectedState;
            Debug.Log("state: UNselected");
        }
    }
    void LockedState()
    {
        if (!IsInFOV(target))
        {
            NextState = SelectedState;
            Debug.Log("state: selected");
        }
        if (target != UXStateManager.GetTarget()) {
            NextState = UnselectedState;
            Debug.Log("state: UNselected");
        }      
    }

    /// //////////////////////////////////////////////////////////////////////

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
    //know when a target is first locked
    float BeginTargetLockTime = 0;
    float TimeToLock = 2f;
    bool TargetLocked = false;
    bool BeginTargetLock = false;


	void FixedUpdate ()
    {
        NextState();
        //target = UXStateManager.GetTarget();
        if (target == null)
        {
            sr.enabled = false;
            lsr.enabled = false;
            stsr.enabled = false;
            ssr.enabled = false;
            stiLR.enabled = false;
            sbsr.enabled = false;
            return;
        }
        ssr.enabled = true;
        sbsr.enabled = true;
        stiLR.enabled = true;
        stsr.enabled = (target == UXStateManager.GetTarget());
        lsr.enabled = (target == UXStateManager.GetTarget());

        LockAnimation();

        sr.enabled = true;
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
            sr.enabled = true;
            arrowIcon.SetActive(false);
            var distToCamera = canvas.transform.position - camera.transform.position;
            var targetIconPos = distToCamera.magnitude * (target.transform.position - camera.transform.position).normalized;
            squareIcon.transform.position = camera.transform.position + targetIconPos;
        }
        else
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

    //only perform lock if in fov and target is selected (stsr.enabled)
    private void LockAnimation()
    {
        if (lsr.enabled && IsInFOV(target))
        {
            if (stsr.enabled)
            {
                if (TargetLocked == true)
                {
                    var startingPos = camera.transform.position;
                    var endingPos = startingPos + startingPos.magnitude / 2 * (target.transform.position - camera.transform.position).normalized;
                    lockedIcon.transform.position = endingPos;
                }
                else if (BeginTargetLock == false)
                {
                    BeginTargetLock = true;
                    BeginTargetLockTime = Time.time;
                    lsr.enabled = true;
                }
                else
                {
                    //move targetLockIcon from center screen to targetIcon position
                    var startingPos = camera.transform.position;
                    var endingPos = startingPos + startingPos.magnitude / 2 * (target.transform.position - camera.transform.position).normalized;
                    var t = Mathf.Min(1, (Time.time - BeginTargetLockTime) / TimeToLock);
                    lockedIcon.transform.position = Vector3.Lerp(startingPos, endingPos, t);
                    if (t == 1)
                    {
                        TargetLocked = true;
                        lsr.enabled = TargetLocked;
                        audio.clip = lockedAudio;
                        audio.loop = false;
                        audio.Play();
                    }
                    else
                    {
                        if (audio.clip != lockingAudio)
                        {
                            audio.loop = true;
                            audio.clip = lockingAudio;
                            audio.Play();
                        }
                    }
                }
            }
        }
        else
        {
            if (audio.clip == lockingAudio)
            {
                audio.clip = unlockedAudio;
                audio.loop = false;
                audio.Play();
            }
            TargetLocked = false;
            BeginTargetLock = false;
            stsr.enabled = false;
            lsr.enabled = false;
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
