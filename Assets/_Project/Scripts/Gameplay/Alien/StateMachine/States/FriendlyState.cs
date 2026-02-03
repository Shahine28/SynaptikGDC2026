using System.Collections;
using UnityEngine;


public class FriendlyState : State
{
    public override StateID GetStateID()
    {
        return StateID.Friendly;
    }

    public override void StateInit(StateMachine stateMachine)
    {
        base.StateInit(stateMachine);
    }

    public override void StateEnter(StateID PreviousStateID) // Je laisse ces fonctions en virtual et pas abstract pour pouvoir faire des modifcations générales sur ces fonctions
    {
        base.StateEnter(PreviousStateID);
        
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        _alien.Roam();
    }
    
    public override void StateExit(StateID NextStateID)
    {
        base.StateExit(NextStateID);
        
        _alien.StopMoving();
    }

    public override void StateUpdate(float deltaTime)
    {
        base.StateUpdate(deltaTime);
    }
    
    protected override void OnDestinationReached()
    {
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        StopAllCoroutines();
        StartCoroutine(WaitBeforeRoam());
    }

    
    protected override void OnPlayerEnterTalkZone()
    {
        if (_isStatic) return;
        _isRoaming = false;
        StopAllCoroutines();
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        if (_isStatic) return;
        StartCoroutine(WaitBeforeRoam());
    }
}
