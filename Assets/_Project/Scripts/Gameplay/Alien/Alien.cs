using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

// using FMODUnity;


public class Alien : MonoBehaviour, IInteraction
{
    [Header("State Machine")] 
    [SerializeField, Required] private StateMachine _stateMachine;
    [SerializeField] private StateID _defaultStateID = StateID.Friendly;
    public StateID DefaultStateID => _defaultStateID;
    
    [Header("Movement")]
    [SerializeField, Required] private NavMeshAgent _navMeshAgent;
    private Coroutine _moveCoroutine;
    private Coroutine _followCoroutine;
    private Coroutine _fleeCoroutine;
        
    
    [Header("Alien Interaction Zone & Movement Zone")]
    [SerializeField, Required] private InteractionZone _interactionZone;
    public InteractionZone InteractionZone => _interactionZone;
    private bool _interactionZoneIsSet => _interactionZone != null;
    [SerializeField, ShowIf("_interactionZoneIsSet")] private float _interactionZoneRadius;
    
    [SerializeField, Required] private RoamZone _roamZone;
    public RoamZone RoamZone => _roamZone;
    private bool _roamZoneIsSet => _roamZone != null;
    [SerializeField, ShowIf("_roamZoneIsSet")] private float _roamZoneRadius;
    
    [Header("Animation")]
    [SerializeField, Required] private AlienAnimation _alienAnimation;
    
    [Header("EmotionColorVisual")]
    [SerializeField, Required] private AlienEmotionVisuals _alienEmotionColorVisuals;
    
    [Header("VFX")]
    [SerializeField] private ParticleSystem _alienVFX;
    
    public Action OnDestinationReachedAction;
    
    // [Header("Sound")]
    // [SerializeField] private VoicesModels _attributedVoice;

    

    
    private void OnValidate()
    {
        if (_roamZone)
        {
            _roamZone.SetRadius(_roamZoneRadius);
        }

        if (_interactionZone)
        {
            _interactionZone.SetRadius(_interactionZoneRadius);
        }
    }

    private void Awake()
    {
        if (_alienAnimation)
            _alienAnimation = GetComponent<AlienAnimation>();
        
        if (!_alienEmotionColorVisuals)
            _alienEmotionColorVisuals = GetComponent<AlienEmotionVisuals>();
    }

    private void Start()
    {
        if (_stateMachine == null)
        {
            Debug.LogError("No stateMachine assigned to NPC");
            return;
        }

        _stateMachine.Init(this);
        SynaptikInput synaptikInput =  new SynaptikInput
        {
            emotionType = _stateMachine.GetEmotionTypeFromStateId(_defaultStateID),
            actionType = ActionType.Action
        };
        _alienEmotionColorVisuals?.OnEmotionColorChanged(synaptikInput);
    }
    
    private void Update()
    {
        if (!_stateMachine) return;
        _stateMachine.StateMachineUpdate(Time.deltaTime);

        if (!_interactionZone || !_interactionZone.IsTargetInRange || !_stateMachine.GetCurrentState().LookAtTarget ||
            !_interactionZone.TargetToDetect) return;
        
        
        Vector3 direction = _interactionZone.TargetToDetect.transform.position - transform.position;

        direction.y = 0;

        if (direction == Vector3.zero) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
    
    public void Interact(SynaptikInput action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (_stateMachine)
            _stateMachine.UpdateState(action.emotionType);
        _alienEmotionColorVisuals.OnEmotionColorChanged(action);
    }
    
    
#region StateMachine
    public StateID GetCurrentStateID()
    {
        return _stateMachine.GetCurrentStateID();
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (_stateMachine.GetCurrentState().IsStatic) return; 
        
        destination = new Vector3(destination.x, 0, destination.z);

        // Empêche de se déplacer vers la même destination
        if (_navMeshAgent.hasPath && Vector3.Distance(_navMeshAgent.destination, destination) < 0.1f)
            return;
        
        destination = new Vector3(destination.x, 0, destination.z);
        _navMeshAgent.SetDestination(destination); 
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(CheckArrival(destination)); 
    }

    public void StopMoving()
    {
        if (_moveCoroutine != null) 
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        
        if (_followCoroutine != null)
        {
            StopCoroutine(_followCoroutine);
            _followCoroutine = null;
        }
        
        if (_fleeCoroutine != null)
        {
            StopCoroutine(_fleeCoroutine);
            _fleeCoroutine = null;
        }
        
        _navMeshAgent.ResetPath();
    }
    
    private IEnumerator CheckArrival(Vector3 destination)
    {
        int iterations = 0;
        while ((_navMeshAgent.pathPending 
                || _navMeshAgent.remainingDistance >= _navMeshAgent.stoppingDistance) && iterations++ < 10000 ) //On mets une sécurité pour éviter une boucle infini
        {
            yield return new WaitForEndOfFrame();
        }

        OnDestinationReached();
    }

    private void OnDestinationReached()
    {
        OnDestinationReachedAction?.Invoke();
    }

    public void Roam()
    {
        if (!_roamZone)
        {
            Debug.LogError("No roamZone assigned to Alien");
            return;
        }

        Vector3 randomPoint = _roamZone.GetRandomPointInZone();
        MoveTo(randomPoint);
    }
    
    public void StartFollowingTarget(Transform target)
    {
        if (_followCoroutine != null) return; // On est déjà en train de suivre la target
        StopMoving();
    
        _followCoroutine = StartCoroutine(FollowRoutine(target));
    }
    

    private IEnumerator FollowRoutine(Transform target)
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (target)
        {
            Vector3 targetPos = target.position;

            if (_roamZone && _stateMachine.GetCurrentState().AlwaysStayInRoamingZone)
            {
                targetPos = _roamZone.ClampPositionToZone(targetPos);
            }
            
            _navMeshAgent.SetDestination(targetPos);
            
            yield return wait;
        }
    }
    
    public void StartFleeingTarget(Transform target, float fleeDistance)
    {
        if (_fleeCoroutine != null) return;
        StopMoving(); 
        _fleeCoroutine = StartCoroutine(FleeRoutine(target, fleeDistance));
    }
    
    private IEnumerator FleeRoutine(Transform target, float fleeDistance)
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (target)
        {
            Vector3 directionAwayFromTarget = transform.position - target.position;
            Vector3 fleeDestination = transform.position + directionAwayFromTarget.normalized * fleeDistance;
            
            if (_roamZone && _stateMachine.GetCurrentState().AlwaysStayInRoamingZone)
            {
                fleeDestination = _roamZone.ClampPositionToZone(fleeDestination);
            }
            
            _navMeshAgent.SetDestination(fleeDestination);
        
            yield return wait;
        }
    }
    
#endregion

#region VFX
    public void PlayVFX()
    {
        if (_alienVFX)
        {
            Debug.Log("Play VFX");
            _alienVFX.Play();
        }
    }
    
    public void StopVFX()
    {
        if (_alienVFX)
        {
            _alienVFX.Stop();
        }
    }
    
    public void ClearVFX()
    {
        if (_alienVFX)
        {
            _alienVFX.Clear();
        }
    }
#endregion

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;
        if (!_roamZone) return;
        if (_roamZone.transform.position != transform.position)
        {
            _roamZone.transform.position = transform.position;
        }
    }
}

