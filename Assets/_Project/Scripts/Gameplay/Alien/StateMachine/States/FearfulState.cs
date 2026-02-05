using UnityEngine;


public class FearfulState : State
{
    [SerializeField] private float _fleeDistance = 10f;
    private bool _isFleeing;
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
        if (!_alien.InteractionZone.IsTargetInRange && _alien.CurrentMovementMode != Alien.MovementMode.Flee && !_isPlayerFarEnough)
        {
            _alien.StartFleeingTarget(_alien.InteractionZone.TargetToDetect.transform, _fleeDistance);  
        }
        else if (!_alien.InteractionZone.IsTargetInRange && _alien.CurrentMovementMode == Alien.MovementMode.Flee &&  _isPlayerFarEnough)
        {
            _alien.StopMoving();
            StartRoaming();
        }
    }
    
    protected override void OnRoamDestinationReached()
    {
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        base.OnRoamDestinationReached();
    }

    protected override void OnFleeDestinationReached()
    {
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        base.OnFleeDestinationReached();
        CheckMovement();
    }
    
    protected override void OnPlayerEnterTalkZone()
    {
        if (_isStatic) return;
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        CheckMovement();
    }
    
    
    

}
