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
    
    private bool _isTargetInHitZone;
    
    private bool _hasHitPlayer;

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
        if (_isStatic || _alien.InteractionZone.IsTargetInRange) return;
        
        if (!_isTargetInHitZone && _isPlayerFarEnough)
        {
            StartRoaming();
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
    }
    
    private void OnPlayerEnterHitZone()
    {
        _isTargetInHitZone = true;
        _alien.StopMoving();
        _hasHitPlayer = false;
        _alien.AlienAnimation.PlayPunch();
    }

    private void OnPlayerExitHitZone()
    {
        _isTargetInHitZone = false;
    }
    
    public void OnPlayerPunched()
    {
        _hasHitPlayer = true;
    }

    public void OnPunchCompleted()
    {
        if (_hasHitPlayer)
        {
            StartRoaming();
        }
        else
        {
            _alien.StartFollowingTarget(_alien.InteractionZone.TargetToDetect.transform);
        }
        _hasHitPlayer = false;
    }
    
    protected override void OnPlayerEnterTalkZone()
    {
        StopRoaming();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        _alien.StopMoving();
        if (_isPlayerFarEnough)
        {
            StartRoaming();
        }
        else
        {
            _alien.StartFollowingTarget(_alien.InteractionZone.TargetToDetect.transform);
        }
    }
    
    protected override void OnRoamDestinationReached()
    {
        if (_isStatic || _alien.InteractionZone.IsTargetInRange || _isTargetInHitZone) return;
        base.OnRoamDestinationReached();
    }
}
