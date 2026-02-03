using UnityEngine;


public class CuriousState : State
{
    [SerializeField] private float _maxDistanceWithPlayer;
    [SerializeField] private bool _showGizmos;
    
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
    }

    public override void StateExit(StateID NextStateID)
    {
        base.StateExit(NextStateID);
    }

    public override void StateUpdate(float deltaTime)
    {
        base.StateUpdate(deltaTime);
        if (_isStatic || _alien.InteractionZone.IsTargetInRange || _isRoaming) return;
        float distance = Vector3.Distance(_alien.InteractionZone.TargetToDetect.transform.position, transform.position);
        if (distance < _maxDistanceWithPlayer)
        {
            _alien.StartFollowingTarget(_alien.InteractionZone.TargetToDetect.transform);
        }
        else
        {
            StartCoroutine(WaitBeforeRoam());
        }
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
        StopAllCoroutines();
        _alien.StopMoving();
    }
    
    protected override void OnPlayerExitTalkZone()
    {
        if (_isStatic) return;
        StopAllCoroutines();
        _alien.StopMoving();
    }
    
    public void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxDistanceWithPlayer);
    }
}
