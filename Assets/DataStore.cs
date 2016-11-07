using UnityEngine;
using System.Collections;

public static class DataStore { 

    //public static GameObject userSelection; //TODO: make this an array or linked list
    public static bool simpleMovement = false;
}

public enum SelectStates
{
    SELECT_SOURCE,
    SELECT_TARGET
}
public enum UXStates
{
    HIDDEN,
    IDLE,
    MOVE,
    ATTACK,
    SCAN,
    JAM,
    CONFIG
}
public static class UXStateManager
{
    public static GameObject selectionIcon;
    public static GameObject targetIcon;
    static SelectStates selectState;
    static UXStates uxState;
    static GameObject selectedSource, selectedTarget;
    public static void EnableTargetSelection()
    {
        selectState = SelectStates.SELECT_TARGET;
        Debug.Log("Now selecting target");
    }
    public static void SelectUnit(GameObject unit)
    {
        switch (selectState)
        {
            case SelectStates.SELECT_SOURCE:
                selectedSource = unit;
                selectionIcon.transform.parent = unit.transform;
                selectionIcon.transform.localPosition = Vector3.zero;
                selectionIcon.SetActive(true);
                uxState = UXStates.IDLE;
                ClearTarget();
                Debug.Log("Selected source");
                break;
            case SelectStates.SELECT_TARGET:
                //can't select same target as source
                if (selectedSource == unit)
                {
                    Debug.Log("Can't select same target as source");
                    break;
                }
                Debug.Log("Selected target");
                selectedTarget = unit;
                targetIcon.transform.parent = unit.transform;
                targetIcon.transform.localPosition = Vector3.zero;
                targetIcon.SetActive(true);
                //TODO this shouldn't happen until menu state change (cancel/escape from current mode)
//TODO reset porkchop plot and intercept line on tgt or src change
                break;
        }
    }
    public static void ClearSource() { selectedSource = null; uxState = UXStates.HIDDEN; }
    public static void ClearTarget() { selectedTarget = null; }
    public static GameObject GetSource()
    {
        return selectedSource;
    }
    public static GameObject GetTarget()
    {
        if (selectedSource == null)
            Debug.Log("No target set");
        return selectedTarget;
    }
    public static UXStates getUXState() { return uxState; }

    static void ShowMenu()
    {
        switch (uxState)
        {
            case UXStates.IDLE:
                //show main menu
                //spawn menu in front of player
                break;
            case UXStates.MOVE:
                break;
            case UXStates.ATTACK:
                break;
            case UXStates.SCAN:
                break;
            case UXStates.JAM:
                break;
            case UXStates.CONFIG:
                break;
        }
    }
    static void Update()
    {
        
    }
}
