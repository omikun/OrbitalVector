using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroMaker : MonoBehaviour {
    public GameObject linePrefab;
    List<LineRenderer> lines;
    int segments = 64;
	// Use this for initialization
	void Start () {
        var line = linePrefab.GetComponent<LineRenderer>();
        line.SetVertexCount(segments+1);
        drawCircle(line);
	}
    [ContextMenu("Draw line!")]
	void drawCircle(LineRenderer line)
    {
        float semiMajor = 2;
        float semiMinor = 2;
        Vector3 pos;
        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.PI * i / segments * 2;
            pos.x = (Mathf.Cos(rad) * semiMajor);
            pos.z = (Mathf.Sin(rad) * semiMinor);
            pos.y = (0);
            //Debug.Log("i: " + i + " rad: " + rad + " pos: " + pos);
            line.SetPosition(i, pos);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
