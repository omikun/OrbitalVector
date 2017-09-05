using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Component of camera (or enemy ship?)
public class TargetRadar : MonoBehaviour {
    public float fov = 20;
    GameObject[] targets; //targets that are in range and targetable
    public GameObject targetIndicatorPrefab;
    List<GameObject> stis = new List<GameObject>();

    float[] angles;
    float lastTime;
    float delay = .5f;
    // Use this for initialization
    void Start () {
        lastTime = Time.time;
        Initialize();
        targets = GameObject.FindGameObjectsWithTag("ship");
        foreach(var target in targets)
        {
            Debug.Log("found target: " + target.name + " dist: " + target.transform.position.magnitude);
            UXStateManager.AddTarget(target);
            var temp = Instantiate(targetIndicatorPrefab);
            temp.transform.parent = transform; //camera.transform
            temp.GetComponent<TargetIndicatorLogic>().target = target;
            stis.Add(temp);
        }
        angles = new float[targets.Length];
        //get list of targetable gameobjects
        Debug.Log("num targets: " + targets.Length);
	}
	
    void Initialize() {
        //fov = Mathf.Atan2(5f, 2f);
    }
    // Update is called once per frame

    bool printDebugFlag = true;
    void Update() {
        //var target = IsInFOV();
        //DebugPrint(target);
        //if ( target )
        //    UXStateManager.SelectTarget(target);
    }
    //find next smallest dist that is larger than prevMinDist
    GameObject FindClosestTarget(GameObject ret=null, float prevMinDist=0f)
    {
        //GameObject ret = null;
        var minDist = float.PositiveInfinity;
        foreach (var target in targets)
        {
            var dist = target.transform.position.magnitude;
            if (dist < minDist && prevMinDist < dist)
            {
                minDist = dist;
                ret = target;
            }
        }
        return ret;
    }
    public void SelectNextTarget()
    {
        //sort targets by distance
        //get currently selected target
        var target = UXStateManager.GetTarget();
        GameObject nextTarget = null;
        if (target != null)
        {
            nextTarget = FindClosestTarget(target, target.transform.position.magnitude);
        }
        if (target == null || target == nextTarget)
        {
            //pick closest target
            var unit = FindClosestTarget();
            UXStateManager.SelectTarget(unit);
        } else
        {
            //pick next closest target
            UXStateManager.SelectTarget(nextTarget);
        }
    }
    GameObject IsInFOV()
    {
        GameObject selectedTarget = null;
        //if a targetable game object is within fov (really fov/2) of camera.forward
        float minAngle = fov;
        int i = 0;
        foreach (var target in targets)
        {
            if (target == null)
                continue; //TODO remove destroyed targets from list, or rebuild list?
            var targetDir = target.transform.position - transform.position;
            var angle = Vector3.Angle(targetDir, transform.forward);
            if (angle < minAngle)
            {
                minAngle = angle;
                selectedTarget = target;
            }
            angles[i++] = angle;
        }
        return selectedTarget;
    }
    void DebugPrint()
    { 
        if (printDebugFlag)
        {
            StringBuilder debugString = new StringBuilder("angle: ");
            foreach(var angle in angles)
            {
                debugString.Append(angle.ToString());
                debugString.Append(" ");
            }
            Debug.Log(debugString);

            //if (selectedTarget) Debug.Log("target: " + selectedTarget.name + " minAngle: " + minAngle);
            printDebugFlag = false;
        }
        //print every delay
        if (Time.time > lastTime + delay)
        {
            lastTime = Time.time;
            printDebugFlag = true;
        }

    }
}
