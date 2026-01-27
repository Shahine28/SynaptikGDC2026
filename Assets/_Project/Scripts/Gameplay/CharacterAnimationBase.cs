using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Rigidbody _rb;

    [Header("Speed Settings")]
    [SerializeField, Min(0f)] protected float _maxReportedSpeed = 8f;
    [SerializeField, Range(0f, 0.5f)] protected float _speedDampTime = 0.1f;

    [Header("Animator Parameter Names")]
    [SerializeField] protected string _paramSpeed = "Speed";
    [SerializeField] protected string _paramHitTrig = "Punch";

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
    private int _hashHitTrig;

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
        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);
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
    
    protected void PlayPunch() => _animator.SetTrigger(_hashHitTrig);
}
