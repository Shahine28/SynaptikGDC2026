using UnityEngine;


public class FearfulState : State
{
    [SerializeField] private float _fleeDistance = 10f;
    public override StateID GetStateID()
    {
        return StateID.Fearful;
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
        _alien.StopMoving();
    }

    public override void StateUpdate(float deltaTime)
    {
        base.StateUpdate(deltaTime);
        CheckMovement();
    }

    public override void CheckMovement()
    {
        if (_isStatic) return;

        if (_alien.InteractionZone.IsTargetInRange) return;
        
        
        if (_isPlayerFarEnough && !_isRoaming)
        {
            _alien.StopMoving();
            StartRoaming();
        }
        else if (!_isPlayerFarEnough && _alien.CurrentMovementMode != Alien.MovementMode.Flee)
        {
            _alien.StopMoving();
            StopRoaming();
            _alien.StartFleeingTarget(_alien.InteractionZone.TargetToDetect.transform, _fleeDistance);  
        }
    }
    
    protected override void OnRoamDestinationReached()
    {
        if (_isStatic) return;
        base.OnRoamDestinationReached();
    }

    protected override void OnFleeDestinationReached()
    {
        if (_isStatic) return;
        base.OnFleeDestinationReached();
        CheckMovement();
    }
    
    protected override void OnPlayerEnterTalkZone()
    {
        if (_isStatic) return;
        StopRoaming();
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        if (_isStatic) return;
        CheckMovement();
    }

}
