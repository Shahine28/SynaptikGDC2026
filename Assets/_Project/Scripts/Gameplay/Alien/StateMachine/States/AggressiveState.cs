using System;
using System.Linq;
using System.Numerics;
using NaughtyAttributes;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


public class AggressiveState : State
{
    [Header("Hit Zone")]
    [SerializeField, Required] private InteractionZone _alienHitZone;
    private bool _isHitZoneSet => _alienHitZone != null;
    [SerializeField, ShowIf("_isHitZoneSet")] private float _hitZoneRadius;
    
    private bool _hasHitPlayer;
    private bool _hasTryToHitPlayer;
    

    private void OnValidate()
    {
        if (!_alienHitZone) return;
        _alienHitZone.SetRadius(_hitZoneRadius);
    }
    public override StateID GetStateID()
    {
        return StateID.Aggressive;
    }

    public override void StateInit(StateMachine stateMachine)
    {
        base.StateInit(stateMachine);
    }

    public override void StateEnter(StateID PreviousStateID) 
    {
        base.StateEnter(PreviousStateID);
        _alienHitZone.OnPlayerEnter.AddListener(OnPlayerEnterHitZone);
        _alienHitZone.OnPlayerExit.AddListener(OnPlayerExitHitZone);
        if (_alienHitZone.IsTargetInRange)
        {
            Punch();
        }
        else
        {
            CheckMovement();
        }
        
    }

    public override void StateExit(StateID NextStateID)
    {
        base.StateExit(NextStateID);
        _alienHitZone.OnPlayerEnter.RemoveListener(OnPlayerEnterHitZone);
        _alienHitZone.OnPlayerExit.RemoveListener(OnPlayerExitHitZone);
        
        StopAllCoroutines();
        _alien.StopMoving();
    }

    public override void StateUpdate(float deltaTime)
    {
        base.StateUpdate(deltaTime);
        if (_isStatic) return;
        CheckMovement();
    }

    public override void CheckMovement()
    {
        if (_isStatic) return;
        
        if (!_alien.InteractionZone.IsTargetInRange)
        {
            if (_hasHitPlayer)
            {
                if (_isRoaming) return;
                StopAllCoroutines();
                _alien.StopMoving();
                StartRoaming();
                return;
            }
            
            if (_isPlayerFarEnough && !_isRoaming)
            {
                _hasTryToHitPlayer = false;
                _hasHitPlayer = false;
                StopAllCoroutines();
                _alien.StopMoving();
                StartRoaming();
            }
            else if (!_isPlayerFarEnough && _alien.CurrentMovementMode != Alien.MovementMode.Follow)
            {
                StopAllCoroutines();
                _alien.StopMoving();
                StopRoaming();
                _alien.StartFollowingTarget(_alien.InteractionZone.TargetToDetect.transform);
            }
        }
    }


    private void OnPlayerEnterHitZone()
    {
        StopAllCoroutines();
        _alien.StopMoving();
        StopRoaming();
        Punch();
    }

    private void OnPlayerExitHitZone()
    {
        
    }
    
    
    private void Punch()
    {
        _hasTryToHitPlayer = true;
        _hasHitPlayer = false;
        _alien.AlienAnimation.PlayPunch();
    }
    
    public void OnPlayerHit()
    {
        _hasHitPlayer = true;
        StopAllCoroutines();
        _alien.StopMoving();
        StartRoaming();
    }

    public void OnPunchCompleted()
    {   
        _hasTryToHitPlayer = false;
        if (_hasHitPlayer) return;
        CheckMovement();
    }
    
    protected override void OnPlayerEnterTalkZone()
    {
        if (_alien.CurrentMovementMode == Alien.MovementMode.Follow || _hasHitPlayer) return;
        StopAllCoroutines();
        StopRoaming();
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        if (_hasHitPlayer) return;
        CheckMovement();
    }
    
    protected override void OnRoamDestinationReached()
    {
        if (_isStatic) return;
        base.OnRoamDestinationReached();
    }
}
