using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Component of camera (or enemy ship?)
public class TargetRadar : MonoBehaviour {
    public float fov = 20;
    GameObject[] targets; //targets that are in range and targetable

    float[] angles;
    float lastTime;
    float delay = .5f;
    // Use this for initialization
    void Start () {
        lastTime = Time.time;
        Initialize();
        targets = GameObject.FindGameObjectsWithTag("ship");
        angles = new float[targets.Length];
        //get list of targetable gameobjects
        Debug.Log("num targets: " + targets.Length);
	}
	
    void Initialize() {
        //fov = Mathf.Atan2(5f, 2f);
    }
    // Update is called once per frame

    bool printDebugFlag = true;
	void Update () {
        //if a targetable game object is within fov (really fov/2) of camera.forward
        GameObject selectedTarget = null;
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
        if (printDebugFlag)
        {
            StringBuilder debugString = new StringBuilder("angle: ");
            foreach(var angle in angles)
            {
                debugString.Append(angle.ToString());
                debugString.Append(" ");
            }
            //Debug.Log(debugString);
            if (selectedTarget)
                Debug.Log("target: " + selectedTarget.name + " minAngle: " + minAngle);
            printDebugFlag = false;
        }
        //print every delay
        if (Time.time > lastTime + delay)
        {
            lastTime = Time.time;
            printDebugFlag = true;
        }
        if (selectedTarget)
            UXStateManager.SelectTarget(selectedTarget);

    }
}
