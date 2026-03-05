using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Rigidbody _rb;

    [Header("Speed Settings")]
    [SerializeField, Min(0f)] protected float _maxReportedSpeed = 8f;
    [SerializeField, Range(0f, 0.5f)] protected float _speedDampTime = 0.1f;
    [SerializeField] private float _baseAnimationSpeed = 3f;

    [Header("Animator Parameter Names")]
    [SerializeField] protected string _paramSpeed = "Speed";
    [SerializeField] protected string _paramHitTrig = "Punch";
    [SerializeField] protected string _paramAnimationSpeed = "LocomotionMultiplier";

    [SerializeField, SerializedDictionary("Emotion", "Parameter Name")]
    protected SerializedDictionary<EmotionType, string> _paramEmotions = new()
    {
        {EmotionType.Friendly, "IsHappy" },
        {EmotionType.Aggressive, "IsAngry" },
        {EmotionType.Fearful, "IsAfraid" },
        {EmotionType.Curious, "IsCurious" },
    };

    private Dictionary<EmotionType, int> _hashEmotions;
    
    protected int _hashSpeed;
    protected int _hashAnimationSpeed;
    private int _hashHitTrig;

    [Header("Punch Event & Area")]
    [SerializeField] protected UnityEvent OnPunchEvent;
    [SerializeField] protected UnityEvent OnPunchCompletedEvent;
    public UnityEvent OnHitByPunch;
    [SerializeField] protected Transform _punchSocket;
    [SerializeField] protected float _punchArea = 2.0f;
    protected readonly Collider[] _punchCollider = new Collider[10];
    protected int hitByPunchCount = 0;
    

    protected virtual void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected virtual void Awake()
    {
        if (!_animator)
            Debug.LogError($"{GetType().Name}: pas d'Animator assigné !", this);

        _hashSpeed = Animator.StringToHash(_paramSpeed);
        _hashHitTrig = Animator.StringToHash(_paramHitTrig);
        
        _hashAnimationSpeed = Animator.StringToHash(_paramAnimationSpeed);
        
        _hashEmotions = new Dictionary<EmotionType, int>();
        

        foreach (var entry in _paramEmotions)
        {
            int hash = Animator.StringToHash(entry.Value);
            _hashEmotions.Add(entry.Key, hash);
        }

        if (!_rb)
            _rb = GetComponent<Rigidbody>();
        
    }

    protected virtual void Update()
    {
        float speed = 0f;
        if (_rb)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            speed = v.magnitude;
        }

        speed = Mathf.Min(speed, _maxReportedSpeed);
        float normalized = _maxReportedSpeed > 0.0001f ? speed / _maxReportedSpeed : 0f;

        float multiplier = 1f;
        if (normalized >= 0.1f)
        {
            multiplier = normalized * _baseAnimationSpeed;
        }
        
        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);
        _animator.SetFloat(_hashAnimationSpeed, multiplier);

    }
    

    public void SetEmotion(EmotionType emotion)
    {
        foreach (var entry in _hashEmotions)
        {
            int hash = _hashEmotions[entry.Key];
            if (hash == -1) continue;
            _animator.SetBool(hash, entry.Key == emotion);
        }
    }

    public void UnsetEmotion(EmotionType emotion)
    {
        if (_hashEmotions.TryGetValue(emotion, out int hash))
        {
            if (hash != -1)
            {
                _animator.SetBool(hash, false);
            }
        };
    }

    public void ClearAllEmotions()
    {
        foreach (var entry in _hashEmotions)
        {
            UnsetEmotion(entry.Key);
        }
    }

    public void PlayPunch()
    {
        _animator.SetTrigger(_hashHitTrig);
        if (TryGetComponent(out WorldEntity worldEntity))
        {
            GameEvents.TriggerAnimationAction(worldEntity.EntityID, "Punch");
        }
    }
    
    public virtual void OnPunch()
    {
        OnPunchEvent?.Invoke();
        hitByPunchCount = Physics.OverlapSphereNonAlloc(_punchSocket.position, _punchArea,  _punchCollider);
    }

    public virtual void OnPunchCompleted()
    {
        OnPunchCompletedEvent?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_punchSocket.position, _punchArea);
    }
}
