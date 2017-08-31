using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroMaker : MonoBehaviour {
    public GameObject linePrefab;
    public GameObject linePrefabMini1;
    public GameObject linePrefabMini2;
    public GameObject linePrefabPitch;
    public GameObject linePrefabRoll;
    List<LineRenderer> lines;
    int segments = 64;
	// Use this for initialization
	void Start () {
        var line = linePrefab.GetComponent<LineRenderer>();
        line.numPositions = (segments+1);
        drawCircle(2f, 0f, line);

        line = linePrefabMini1.GetComponent<LineRenderer>();
        line.numPositions = (segments+1);
        drawCircle(1f, 1.71f, line);

        line = linePrefabMini2.GetComponent<LineRenderer>();
        line.numPositions = (segments+1);
        drawCircle(1f, -1.71f, line);

        line = linePrefabPitch.GetComponent<LineRenderer>();
        line.numPositions = (segments+1);
        drawRollCircle(2f, 0f, line);

        line = linePrefabRoll.GetComponent<LineRenderer>();
        line.numPositions = (segments+1);
        drawPitchCircle(2f, 0f, line);
	}
    [ContextMenu("Draw line!")]
	void drawCircle(float radius, float height, LineRenderer line)
    {
        float semiMajor = radius;
        float semiMinor = radius;
        Vector3 pos;
        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.PI * i / segments * 2;
            pos.x = (Mathf.Cos(rad) * semiMajor);
            pos.z = (Mathf.Sin(rad) * semiMinor);
            pos.y = (height);
            //Debug.Log("i: " + i + " rad: " + rad + " pos: " + pos);
            line.SetPosition(i, pos);
        }
    }
    void drawPitchCircle(float radius, float height, LineRenderer line)
    {
        float semiMajor = radius;
        float semiMinor = radius;
        Vector3 pos;
        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.PI * i / segments * 2;
            pos.z = (Mathf.Cos(rad) * semiMajor);
            pos.x = (height);
            pos.y = (Mathf.Sin(rad) * semiMinor);
            //Debug.Log("i: " + i + " rad: " + rad + " pos: " + pos);
            line.SetPosition(i, pos);
        }
    }
    void drawRollCircle(float radius, float height, LineRenderer line)
    {
        float semiMajor = radius;
        float semiMinor = radius;
        Vector3 pos;
        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.PI * i / segments * 2;
            pos.x = (Mathf.Cos(rad) * semiMajor);
            pos.y = (Mathf.Sin(rad) * semiMinor);
            pos.z = (height);
            //Debug.Log("i: " + i + " rad: " + rad + " pos: " + pos);
            line.SetPosition(i, pos);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
