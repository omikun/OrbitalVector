using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
[CustomEditor(typeof(PlayerManager))]
public class ShipPhysicsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawDefaultInspector();
        PlayerManager script = (PlayerManager)target;
        if (GUILayout.Button("Fire Missile"))
        {
            script.FireMissile();
        }
    }
}

