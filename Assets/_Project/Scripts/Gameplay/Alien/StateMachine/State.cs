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
    
    

    public virtual StateID GetStateID() //retourne le StateID associé à cet état
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
        _alien.OnDestinationReachedAction += OnDestinationReached;
        _alien.InteractionZone.OnPlayerEnterAction += OnPlayerEnterTalkZone;
        _alien.InteractionZone.OnPlayerExitAction += OnPlayerExitTalkZone;
    }
    

    public virtual void StateExit(StateID NextStateID)
    {
        _alien.OnDestinationReachedAction -=  OnDestinationReached;
        _alien.InteractionZone.OnPlayerEnterAction -= OnPlayerEnterTalkZone;
        _alien.InteractionZone.OnPlayerExitAction -= OnPlayerExitTalkZone;
    }

    public virtual void StateUpdate(float deltaTime)
    {
        
    }
    
    protected IEnumerator WaitBeforeRoam()
    {
        yield return new WaitForSeconds(_timeBetweenRoam);
        _isRoaming = true;
        _alien.Roam();
    }
    
    protected virtual void OnPlayerEnterTalkZone()
    {
        
    }
    
    protected virtual void  OnPlayerExitTalkZone()
    {
        
    }
    
    protected virtual void OnDestinationReached()
    {
        _isRoaming = false;
    }
}
