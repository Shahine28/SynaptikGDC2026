using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    [SerializeField, ReadOnly] private SynaptikInput _currentSynaptikInput;
    public SynaptikInput CurrentSynaptikInput => _currentSynaptikInput;
    
    [Header("Movement Input")]
    [SerializeField, ReadOnly] private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;
    
    [Header("Action Type")]
    [SerializeField, ReadOnly] private bool _isTalkInput;
    public bool IsTalkInput => _isTalkInput;

    [SerializeField, ReadOnly] private bool _isActionInput;
    public bool IsActionInput => _isActionInput;

    private List<ActionType> _activeActions = new List<ActionType>();

    [Header("Emotion Type")] 
    [SerializeField, ReadOnly] private bool _isFriendlyInput;
    [SerializeField, ReadOnly] private bool _isAgressiveInput;
    [SerializeField, ReadOnly] private bool _isFearfulInput;
    [SerializeField, ReadOnly] private bool _isCuriousInput;
    private List<EmotionType> _activeEmotions = new List<EmotionType>();

    
    public bool HasActionPressed => _activeActions.Count > 0;
    public bool HasEmotionPressed => _activeEmotions.Count > 0;
    
    public event Action<SynaptikInput> OnSynaptikInput;
    
    public event Action<bool> OnActionTriggered;
    public event Action<ActionType> OnActionTypeInput;
    public event Action<EmotionType> OnEmotionTypeInput;

    [SerializeField] private float _inputBufferCooldown = 0.5f;

    private Coroutine _actionBufferCoroutine;
    private Coroutine _talkBufferCoroutine;
    private Coroutine _friendlyBufferCoroutine;
    private Coroutine _agressiveBufferCoroutine;
    private Coroutine _fearfullBufferCoroutine;
    private Coroutine _curiousBufferCoroutine;

    void UpdateCurrentSynaptikInput()
    {
        SynaptikInput newInput;
        
        newInput.emotionType = _activeEmotions.Count > 0 ? _activeEmotions[0] : EmotionType.None;
        newInput.actionType = _activeActions.Count > 0 ? _activeActions[0] : ActionType.None;
        
        OnActionTriggered?.Invoke(_activeActions.Count > 1);
        

        if (_currentSynaptikInput.emotionType == newInput.emotionType &&
            _currentSynaptikInput.actionType == newInput.actionType) return;

        if (_currentSynaptikInput.actionType != newInput.actionType)
        {
            OnActionTypeInput?.Invoke(newInput.actionType);
        }

        if (_currentSynaptikInput.emotionType != newInput.emotionType)
        {
            OnEmotionTypeInput?.Invoke(newInput.emotionType);
        }
        
        
        _currentSynaptikInput = newInput;
        OnSynaptikInput?.Invoke(_currentSynaptikInput);
        if (TryGetComponent(out WorldEntity worldEntity))
        {
            GameEvents.TriggerSynaptikInputChange(worldEntity.EntityID, _currentSynaptikInput);
        }
    }
    
    private void UpdateEmotionState(EmotionType emotion, bool isPressed)
    {
        if (isPressed)
        {
            if (!_activeEmotions.Contains(emotion))
                _activeEmotions.Add(emotion);
        }
        else
        {
            _activeEmotions.Remove(emotion);
        }
        
        UpdateCurrentSynaptikInput();
    }
    
    private void UpdateActionState(ActionType action, bool isPressed)
    {
        if (isPressed)
        {
            if (!_activeActions.Contains(action))
                _activeActions.Add(action);
        }
        else
        {
            _activeActions.Remove(action);
        }

        UpdateCurrentSynaptikInput();
    }
    
    // Bindings des inputs
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnTalkInput(InputAction.CallbackContext context)
    {
        _isTalkInput = context.ReadValueAsButton();
        if (_talkBufferCoroutine  != null)
            StopCoroutine(_talkBufferCoroutine);
        _talkBufferCoroutine = StartCoroutine(InputBufferAction(ActionType.Word, _isTalkInput));
        //UpdateActionState(ActionType.Word, _isTalkInput);
    }

    public void OnActionInput(InputAction.CallbackContext context)
    {
        _isActionInput = context.ReadValueAsButton();
        if (_actionBufferCoroutine != null)
            StopCoroutine(_actionBufferCoroutine);
        _actionBufferCoroutine = StartCoroutine(InputBufferAction(ActionType.Action, _isActionInput));
        //UpdateActionState(ActionType.Action, _isActionInput);
    }

    public void OnFriendlyInput(InputAction.CallbackContext context)
    {
        _isFriendlyInput = context.ReadValueAsButton();
        if (_friendlyBufferCoroutine != null)
            StopCoroutine(_friendlyBufferCoroutine);
        _friendlyBufferCoroutine = StartCoroutine(InputBufferEmotion(EmotionType.Friendly, _isFriendlyInput));
        //UpdateEmotionState(EmotionType.Friendly, _isFriendlyInput);
    }

    public void OnAgressiveInput(InputAction.CallbackContext context)
    {
        _isAgressiveInput = context.ReadValueAsButton();
        if (_agressiveBufferCoroutine!= null)
            StopCoroutine(_agressiveBufferCoroutine);
        _agressiveBufferCoroutine = StartCoroutine(InputBufferEmotion(EmotionType.Aggressive, _isAgressiveInput));
        //UpdateEmotionState(EmotionType.Aggressive, _isAgressiveInput);
    }

    public void OnFearfulInput(InputAction.CallbackContext context)
    {
        _isFearfulInput = context.ReadValueAsButton();
        if (_fearfullBufferCoroutine != null)
            StopCoroutine(_fearfullBufferCoroutine);
        _fearfullBufferCoroutine = StartCoroutine(InputBufferEmotion(EmotionType.Fearful, _isFearfulInput));
        //UpdateEmotionState(EmotionType.Fearful, _isFearfulInput);

    }

    public void OnCuriousInput(InputAction.CallbackContext context)
    {
        _isCuriousInput = context.ReadValueAsButton();
        if (_curiousBufferCoroutine != null)
            StopCoroutine(_curiousBufferCoroutine);
        _curiousBufferCoroutine = StartCoroutine(InputBufferEmotion(EmotionType.Curious, _isCuriousInput));
        //UpdateEmotionState(EmotionType.Curious, _isCuriousInput);
    }

    public IEnumerator InputBufferAction(ActionType type, bool IsPressed)
    {
        yield return new WaitForSeconds(_inputBufferCooldown);
        UpdateActionState(type, IsPressed);
    }

    public IEnumerator InputBufferEmotion(EmotionType type, bool IsPressed)
    {
        yield return new WaitForSeconds(_inputBufferCooldown);
        UpdateEmotionState(type, IsPressed);
    }
}
