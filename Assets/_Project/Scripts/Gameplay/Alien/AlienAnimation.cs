using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class AlienAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramPukeTrig = "Puke";
    private int _hashPukeTrig;

    private CharacterController _controller;
    private Vector3 _lastPosition;

    [SerializeField, Required] private WorldEntity _punchTaget;
    
    
    protected override void Awake()
    {
        base.Awake();
        _hashPukeTrig = Animator.StringToHash(_paramPukeTrig);

        _controller = GetComponent<CharacterController>();
        _lastPosition = transform.position;
    }

    protected override void Update()
    {
        Vector3 delta = transform.position - _lastPosition;
        delta.y = 0f; // ignore les mouvements verticaux

        float speed = delta.magnitude / Time.deltaTime;
        speed = Mathf.Min(speed, _maxReportedSpeed);
        float normalized = _maxReportedSpeed > 0.0001f ? speed / _maxReportedSpeed : 0f;

        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);

        _lastPosition = transform.position;
    }

    public override void OnPunch()
    {
        base.OnPunch();
        int hitCount = _punchCollider.Count(x => x != null);
        for (int i = 0; i < hitCount; i++)
        {
            if (_punchCollider[i].TryGetComponent(out WorldEntity worldEntity))
            {
                if (worldEntity.EntityID == _punchTaget.EntityID)
                {
                    if (_punchCollider[i].TryGetComponent(out PlayerInputSystem playerInputSystem))
                    {
                        if (playerInputSystem.CurrentSynaptikInput is { emotionType: EmotionType.Fearful, actionType: ActionType.Action })
                        {
                            Debug.Log("Player punched but crouched");
                            break;
                        }
                    }
                    OnHitByPunch?.Invoke();
                    Debug.Log("Target punched");
                    break;
                }
            }
        }
    }

    public void PlayPuke()
    {
        if (TryGetComponent(out WorldEntity worldEntity))
        {
            GameEvents.TriggerAnimationAction(worldEntity.EntityID, "Puke");
        }
        _animator.SetTrigger(_hashPukeTrig);
    } 
}