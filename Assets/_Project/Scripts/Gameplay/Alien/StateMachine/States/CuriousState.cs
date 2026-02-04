using UnityEngine;


public class CuriousState : State
{
    private bool _isFollowingPlayer;
    
    public override StateID GetStateID()
    {
        return StateID.Curious;
    }

    public override void StateInit(StateMachine stateMachine)
    {
        base.StateInit(stateMachine);
    }

    public override void StateEnter(StateID PreviousStateID) // Je laisse ces fonctions en virtual et pas abstract pour pouvoir faire des modifcations générales sur ces fonctions
    {
        base.StateEnter(PreviousStateID);
        CheckMovement();
    }

    public override void StateExit(StateID NextStateID)
    {
        base.StateExit(NextStateID);
        StopRoaming();
        StopAllCoroutines();
        _alien.StopMoving();
    }

    public override void StateUpdate(float deltaTime)
    {
        base.StateUpdate(deltaTime);
        CheckMovement();
    }

    private void CheckMovement()
    {
        if (_isStatic || _isRoaming || _isFollowingPlayer) return;
        if (!_isPlayerFarEnough)
        {
            _alien.StartFollowingTarget(_alien.InteractionZone.TargetToDetect.transform);
        }
        else
        {
            StartRoaming();
        }
    }
    
    
    protected override void OnRoamDestinationReached()
    {
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        base.OnRoamDestinationReached();
    }

    protected override void OnFollowDestinationReached()
    {
        base.OnFollowDestinationReached();
        _isFollowingPlayer = false;
    }


    protected override void OnPlayerEnterTalkZone()
    {
        if (_isStatic) return;
        StopAllCoroutines();
        if (_isRoaming) StopRoaming();
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        CheckMovement();
    }
}
