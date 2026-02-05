using System.Collections;
using UnityEngine;

public abstract class State : MonoBehaviour // Cela va être la classe de base pour les états : Disponible, En cours de tâche, Fatigué et le bonus Contrôllé par Joueur. Mais elle ne peut pas être utilisé directement on doit en hérité
{
    protected StateMachine _stateMachine;
    protected Alien _alien;
    [SerializeField] protected bool _isStatic;
    public bool IsStatic => _isStatic;

    [SerializeField] protected bool _lookAtTarget = true;
    public bool LookAtTarget => _lookAtTarget;

    [SerializeField] protected bool _alwaysStayInRoamingZone;
    public bool AlwaysStayInRoamingZone => _alwaysStayInRoamingZone;
    
    [SerializeField] protected float _timeBetweenRoam = 1.0f;
    protected bool _isRoaming;
    
    [SerializeField] protected float _maxDistanceWithPlayer = 5f;
    [SerializeField] private bool _showGizmos;
    protected bool _isPlayerFarEnough 
    {
        get 
        {
            if (!_alien || !_alien.InteractionZone || !_alien.InteractionZone.TargetToDetect) 
                return false;
            
            float distance = Vector3.Distance(transform.position, _alien.InteractionZone.TargetToDetect.transform.position);
            
            return distance > _maxDistanceWithPlayer;
        }
    }

    public virtual void CheckMovement()
    {
        
    }

    public virtual StateID GetStateID()
    {
        return StateID.None;
    }

    public virtual void StateInit(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _alien = _stateMachine.GetAlien();
        Debug.Log("Init State" + GetStateID());
    }

    public virtual void StateEnter(StateID PreviousStateID) // Je laisse ces fonctions en virtual et pas abstract pour pouvoir faire des modifcations générales sur ces fonctions
    {
        _alien.OnRoamingDestinationReachedAction += OnRoamDestinationReached;
        _alien.OnFollowDestinationReachedAction += OnFollowDestinationReached;
        _alien.OnFleeDestinationReachedAction += OnFleeDestinationReached;
        _alien.InteractionZone.OnPlayerEnter.AddListener(OnPlayerEnterTalkZone);
        _alien.InteractionZone.OnPlayerExit.AddListener(OnPlayerExitTalkZone);
    }
    

    public virtual void StateExit(StateID NextStateID)
    {
        _alien.OnRoamingDestinationReachedAction -=  OnRoamDestinationReached;
        _alien.OnFollowDestinationReachedAction -= OnFollowDestinationReached;
        _alien.OnFleeDestinationReachedAction -= OnFleeDestinationReached;
        _alien.InteractionZone.OnPlayerEnter.RemoveListener(OnPlayerEnterTalkZone);
        _alien.InteractionZone.OnPlayerExit.RemoveListener(OnPlayerExitTalkZone);
    }

    public virtual void StateUpdate(float deltaTime)
    {
        
    }

    protected void StartRoaming()
    {
        StopAllCoroutines();
        StartCoroutine(WaitBeforeRoam());
        _isRoaming = true;
    }

    protected void StopRoaming()
    {
        _isRoaming = false;
        _alien.StopMoving();
    }
    
    protected IEnumerator WaitBeforeRoam()
    {
        yield return new WaitForSeconds(_timeBetweenRoam);
        _alien.Roam();
    }
    
    protected virtual void OnPlayerEnterTalkZone()
    {
        
    }
    
    protected virtual void  OnPlayerExitTalkZone()
    {
        
    }
    
    protected virtual void OnRoamDestinationReached()
    {
        if (_isRoaming)
        {
            StartCoroutine(WaitBeforeRoam());
        }
    }

    protected virtual void OnFollowDestinationReached()
    {
        
    }

    protected virtual void OnFleeDestinationReached()
    {
        
    }
    
    public void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxDistanceWithPlayer);
    }
}
