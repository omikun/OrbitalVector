using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
[CustomEditor(typeof(ShipPhysics))]
public class ShipPhysicsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawDefaultInspector();
        ShipPhysics script = (ShipPhysics)target;
        if (GUILayout.Button("Fire Missile"))
        {
            script.FireMissile();
        }
    }
}

