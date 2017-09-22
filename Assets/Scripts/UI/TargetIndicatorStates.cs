using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public partial class TargetIndicatorLogic
{
    //state delegates in FSM
    /// <summary>
    /// unselected -> selected
    /// selected -> locking
    /// locking -> unlocked OR locked
    /// locked -> unlocked
    /// unlocked -> selected
    /// any state -> unselected
    /// any state -> gone -> end
    /// start up/end -> InitialState
    /// </summary>
    Action NextState = null;
    void InitialState() //assumes prev target was destroyed and is coming from EndState
    {
        //squareIconSR.enabled = true; this is determined by squareIcon function
        sphericalIconSR.enabled = true;
        sphericalIconBaseSR.enabled = true;
        sphericalIconLR.enabled = true;

        var isSelected = (target == UXStateManager.GetTarget());
        if (isSelected)
            NextState = SelectedState;
        else
            NextState = UnselectedState;

        selectedIconSR.enabled = isSelected;
        lockedIconSR.enabled = isSelected;
    }
    void GoneState() //target is destroyed; clean up icons
    {
        NextState = EndState;
        squareIconSR.enabled = false;
        lockedIconSR.enabled = false;
        selectedIconSR.enabled = false;
        sphericalIconSR.enabled = false;
        sphericalIconLR.enabled = false;
        sphericalIconBaseSR.enabled = false;
        audio.clip = TargetDestroyedAudio;
        audio.loop = false;
        audio.Play();
    }
    void EndState()
    {
        return;
    }

    void TargetLockFSM()
    {
        if (NextState == EndState) return;
        if (target == null) NextState = GoneState;
        NextState();
    }

    void UnselectedState()
    { 
        if (target == UXStateManager.GetTarget())
        {
            NextState = SelectedState;
            selectedIconSR.enabled = true;
            Debug.Log("state: selected");
        } else
        {
            //steady state for unselected
        }
    }
    void SelectedState()
    {
        if (IsInFOV(target)) //transition to lockingstate
        {
            NextState = LockingState;
            BeginTargetLockTime = Time.time;
            lockedIconSR.enabled = true;

            audio.loop = true;
            audio.clip = lockingAudio;
            audio.Play();

            Debug.Log(name + "state: locking!");
        } else
        {
            //regular selected state animation
        }
    }

    void LockingState()
    {
        if (TryUnlockState()) { }
        else if (Time.time > BeginTargetLockTime + 2f) //transition to LOCKED!
        {
            lockedIconSR.enabled = true;
            audio.clip = lockedAudio;
            audio.loop = false;
            audio.Play();

            NextState = LockedState;
            Debug.Log(name + "state: locked");
        } else {
            //move targetLockIcon from center screen to targetIcon position
            var startingPos = camera.transform.position;
            var endingPos = startingPos + startingPos.magnitude / 2 * (target.transform.position - camera.transform.position).normalized;
            var t = Mathf.Min(1, (Time.time - BeginTargetLockTime) / TimeToLock);
            lockedIcon.transform.position = Vector3.Lerp(startingPos, endingPos, t);
        }
    }

    void LockedState()
    {
        if (TryUnlockState()) { }
        else {
            var startingPos = camera.transform.position;
            var endingPos = startingPos + startingPos.magnitude / 2 * (target.transform.position - camera.transform.position).normalized;
            lockedIcon.transform.position = endingPos;
        }
    }

    bool TryUnlockState()
    {
        if (!IsInFOV(target)) //kick out of LockingState
        {
            audio.clip = unlockedAudio;
            audio.loop = false;
            audio.Play();

            lockedIconSR.enabled = false;
            NextState = SelectedState; //could jump straight to unselected, but next update will do that if need be
            Debug.Log("state: selected");
            return true;
        }
        return false;
    }
}
