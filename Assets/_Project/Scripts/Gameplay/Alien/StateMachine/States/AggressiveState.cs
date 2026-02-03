using System;
using UnityEngine;


public class AggressiveState : State
{
    [SerializeField] private float _maxDistanceWithPlayer;
    [SerializeField] private float _hitZoneRadius;
    [SerializeField] private bool _showGizmos;
    
    public override StateID GetStateID()
    {
        return StateID.Aggressive;
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
    }

    public void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _hitZoneRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxDistanceWithPlayer);
    }
}
