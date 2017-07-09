using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRenderTransform : MonoBehaviour {
    //render space bounds
    public float min = 1000;
    public float max = 2000;
    //real space bounds
    public float rMin = 1000;
    public float rMax = 1000 * 1000 * 1000;
    float rMaxSqr, rMinSqr;
    public float EarthRadius = 12742 * 1000/2;
    public GameObject focus; //this is the source of the transform
    //assume UXManager.GetSource is the focus
	// Use this for initialization
	void Start () {
		rMaxSqr = rMax * rMax;
        rMinSqr = rMin * rMin;
        transform.localScale = new Vector3(EarthRadius, EarthRadius, EarthRadius);
        UXStateManager.SelectUnit(focus);
	}

    // Update is called once per frame
    void Update() {
        bool realScale = true;
        if (realScale)
        {
            GetRealScale();
        } else
        {
            GetApparentScale();
        }
    }

    void GetRealScale()
    {
        var focus = UXStateManager.GetSource();
        if (focus == null)
            return;
        var focusPos = focus.GetComponent<OrbitData>().getRFloat();
        transform.position = -focusPos;
    }
    void GetApparentScale()
    {
        //need to get distance to focus
        focus = UXStateManager.GetSource();
        if (focus == null)
            return;
        var focusPos = focus.GetComponent<OrbitData>().getRFloat();
        var newDist = ConvertToRenderSpace(focusPos);
        transform.position = -focusPos.normalized * newDist;

        //get scale of earth at distance
        var actualDist = focusPos.magnitude;
        var frac = newDist / actualDist;
        var scale = frac * EarthRadius;
        transform.localScale = Vector3.one * scale;
        if (false)//DelayBy(delay))
        {
            Debug.Log("actualDist: " + actualDist + " frac: " + frac + " newDist: " + newDist + " scale: " + scale);
        }
	}
    float delay = 1;
    float lastTime = 0;
    bool DelayBy(float delay)
    {
        if (Time.time - delay > lastTime)
        {
            lastTime = Time.time;
            return true;
        }
        return false;
    }

    float ConvertToRenderSpace(Vector3 offset)
    {
        var dist = offset.sqrMagnitude;
        var index = (dist - rMinSqr) / (rMaxSqr - rMinSqr);
        var newDist = min + index * (max - min);
        return newDist;
    }
}
