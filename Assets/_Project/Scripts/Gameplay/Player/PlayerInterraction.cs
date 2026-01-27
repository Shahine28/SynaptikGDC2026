using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using ExternPropertyAttributes;
using UnityEngine;



public class PlayerInteraction : MonoBehaviour
{
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    
    [Header("Pickup/Drop Settings")]
    [SerializeField] private Transform _handSocket;
    [SerializeField] private LayerMask _pickupMask;

    private HoldableItem _heldItem;

    [Header("Interaction Settings")] 
    [SerializeField] private Transform _interactionTransform;
    [SerializeField] private Vector3 _interactionZoneSize;
    [SerializeField] private LayerMask _interactMask;


    [Header("Dialogues")] 
    [SerializeField, Required] private AlienDialogueSymbolBySynaptikInput _dialogueSymbolBySynaptikInput;
    
    private readonly Collider[] _pickableHitBuffer = new Collider[10];
    private readonly Collider[] _interactableHitBuffer = new Collider[10];

    public event Action OnItemPickedUp;
    public event Action OnItemDropped;
    public event Action<SynaptikInput, HoldableItem> OnSynaptikInterraction;
    
    void Awake()
    {
        if (!_playerInputSystem)
            _playerInputSystem = GetComponent<PlayerInputSystem>();
    }

    void OnEnable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput += HandleSynaptikInput;
    }

    void OnDisable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput -= HandleSynaptikInput;
    }

    private void HandleSynaptikInput(SynaptikInput synaptikInput)
    {
        if (synaptikInput.actionType == ActionType.None || synaptikInput.emotionType == EmotionType.None) return;

        SpeechBubbleManager.Instance?.SpawnBubble(transform, synaptikInput, _dialogueSymbolBySynaptikInput?.GetDialogue(synaptikInput));
        if (!_heldItem && synaptikInput is { actionType: ActionType.Action, emotionType: EmotionType.Curious })
        {
            if (TryFindClosest(_pickupMask, _pickableHitBuffer, out HoldableItem item))
            {
                if (item.TryPick(_handSocket))
                {
                    _heldItem = item;
                    OnItemPickedUp?.Invoke();
                }
            }
            return;
        }
        
        if (TryFindClosest(_interactMask, _interactableHitBuffer, out IInteraction interactable))
        {
            interactable.Interact(synaptikInput, _heldItem, this);
            OnSynaptikInterraction?.Invoke(synaptikInput, _heldItem);
        }
        else
        {
            if (_heldItem == null
                || synaptikInput is not { actionType: ActionType.Action, emotionType: EmotionType.Friendly }) return;
            
            
            if (_heldItem.TryDrop())
            {
                _heldItem = null;
                OnItemDropped?.Invoke();
            }
        }
    }
    
    private bool TryFindClosest<T>(LayerMask mask, Collider[] buffer, out T result) where T : class
    {
        result = null;
        
        int hitCount = Physics.OverlapBoxNonAlloc(
            _interactionTransform.position, 
            _interactionZoneSize * 0.5f, 
            buffer, 
            _interactionTransform.rotation, 
            mask
        );

        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (buffer[i].TryGetComponent(out T component))
            {
                float distance = Vector3.Distance(_interactionTransform.position, buffer[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    result = component;
                }
            }
        }

        return result != null;
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_interactionTransform.position, _interactionZoneSize);
        
    }
}  

[Serializable]
struct SymbolAndDuration
{
    public string Symbol;
    public float Duration;
}
