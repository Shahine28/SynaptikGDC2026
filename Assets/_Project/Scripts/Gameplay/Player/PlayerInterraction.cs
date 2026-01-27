using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


[Serializable]
struct ComboSymbolDefinition
{
    public string Symbols;
    public float Duration;
}


public class PlayerInteraction : MonoBehaviour
{
    private const string LogPrefix = "[PlayerInteraction]";

    [Header("Pickup/Drop Settings")]
    [SerializeField] private Transform handSocket;

    [SerializeField] private float pickupRadius = 1.2f;

    [SerializeField] private LayerMask pickupMask = ~0;

    [SerializeField] private float dropForwardSpeed;

    private HoldableItem _heldItem;
    private string _heldItemId;

    [Header("Interaction Settings")]
    [SerializeField] private Transform aimZone;

    [SerializeField] private float interactRadius = 2f;

    [SerializeField] private float interactHalfFov = 45f;

    [SerializeField] private LayerMask interactMask;

    [Header("Combo Feedback")]
    [SerializeField] private float defaultComboBubbleDuration = 1.75f;
    

    [SerializedDictionary("Synaptik Inputs", "Symbol Definition"), SerializeField]
    private SerializedDictionary<SynaptikInput, ComboSymbolDefinition> comboLookup = new();
    private PlayerComboBubble comboBubble;
    private bool isInInteractionZone;

    private static readonly Collider[] overlap = new Collider[64];
    
    private static readonly Dictionary<EmotionType, string> DefaultEmotionSymbols = new()
    {
        { EmotionType.Aggressive, "⚡" },
        { EmotionType.Friendly, "❤️" },
        { EmotionType.Curious, "❓" },
        { EmotionType.Fearful, "😱" }
    };

    private static readonly Dictionary<ActionType, string> DefaultBehaviorSymbols = new()
    {
        { ActionType.Word, "💬" },
        { ActionType.Action, "✋" }
    };
    
    
    [Header("Player Animation")]
    [SerializeField] private PlayerAnimation _playerAnimation;
    
    
    [Header("Angry Zone")]
    public string AngryQuestAlienId;

    [HideInInspector]
    public Alien AngryQuestAlien;
    

    private void Reset()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
        if (!_playerAnimation) Debug.LogWarning("PlayerInteraction: pas de PlayerAnimation assigné !", this);
    }
    public event Action<bool> InteractionZoneChanged;

    private void Awake()
    {
        comboBubble = GetComponent<PlayerComboBubble>() ?? gameObject.AddComponent<PlayerComboBubble>();
        if (!_playerAnimation)
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            if (!_playerAnimation)
                Debug.LogWarning("PlayerInteraction: pas de PlayerAnimation assigné !", this);
        }

        Debug.Log($"{LogPrefix} '{name}' prêt ({comboLookup.Count} combos).");
    }

    private void Start()
    {

    }

    private void OnDestroy()
    {
        if (InputsDetection.Instance)
        {
            Debug.Log($"{LogPrefix} Désabonné des combos d'InputsDetection.");
        }
    }

    private void OnValidate()
    {
        RebuildComboLookup();
    }

    private void Update()
    {
        // UpdateInteractionZoneState();
    }

    private void RebuildComboLookup()
    {
        comboLookup.Clear();
        // if (comboSymbolDefinitions == null)
        // {
        //     Debug.LogWarning($"{LogPrefix} Aucun symbole de combo configuré.");
        //     return;
        // }
        //
        // foreach (var definition in comboSymbolDefinitions)
        // {
        //     if (definition.Behavior == Behavior.None || definition.Emotion == Emotion.None)
        //         continue;
        //
        //     var key = new ComboKey(definition.Emotion, definition.Behavior);
        //     comboLookup[key] = definition;
        // }
        //
        // Debug.Log($"{LogPrefix} Table de combos reconstruite ({comboLookup.Count} entrées).");
    }
    
    private void HandleEmotion(EmotionType emotion, bool keyReleased)
    {
        if (!keyReleased)
        {
            _playerAnimation?.SetEmotion(emotion);
        }
        else
        {
            _playerAnimation?.UnsetEmotion(emotion);
        }
    }
    private void HandleEmotionAction(EmotionType emotion, ActionType behavior)
    {
        ShowComboFeedback(emotion, behavior);
        

        // if (TryFindInteractionTarget(out var interactable))
        // {
        //     Debug.Log($"{LogPrefix} Combo {emotion}/{behavior} → interactable '{interactable}'.");
        //     interactable.Interact(new ActionValues(emotion, behavior), _heldItem, this);
        // }
        // else if (emotion == Emotion.Friendly && behavior == Behavior.Action && _heldItem)
        // {
        //     DropItem();
        // }
        
        if (emotion == EmotionType.Aggressive)
        {
            if (behavior == ActionType.Action)
                _playerAnimation?.PlayPunch();
            else if (behavior == ActionType.Word && AngryQuestAlien)
            {
                GameManager.Instance.SetMissionFinished(AngryQuestAlienId, AngryQuestAlien.Definition);
                AngryQuestAlien.PlayVFX();
                Debug.Log("Quête de l'Angry Zone complétée !");
            }
        }
    }

    public void PickUp()
    {
        var origin = handSocket ? handSocket.position : transform.position;
        var count = Physics.OverlapSphereNonAlloc(origin, pickupRadius, overlap, pickupMask, QueryTriggerInteraction.Ignore);
        if (count <= 0)
        {
            return;
        }

        HoldableItem bestCandidate = null;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < count; i++)
        {
            var collider = overlap[i];
            if (!collider || !collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            var holdable = collider.GetComponentInParent<HoldableItem>();
            if (!holdable || !holdable.CanBePicked || holdable == _heldItem)
            {
                continue;
            }

            var distance = (holdable.transform.position - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCandidate = holdable;
            }
        }

        if (!bestCandidate)
            return;

        if (_heldItem)
        {
            var velocity = dropForwardSpeed > 0f ? transform.forward * dropForwardSpeed : Vector3.zero;
            _heldItem.Drop(velocity);
            _heldItem = null;
        }

        bestCandidate.Pick(handSocket ? handSocket : transform);
        _heldItem = bestCandidate;
        _heldItemId = _heldItem.ItemId;
        _playerAnimation?.OnPickedUpItem();
        Debug.Log($"{LogPrefix} Objet '{_heldItem.name}' ramassé (ID: {_heldItemId}).");
    }

    public void DropItem(bool destroyItem = false)
    {
        if (!_heldItem)
        {
            Debug.Log($"{LogPrefix} Aucun objet à déposer.");
            return;
        }

        if (destroyItem)
        {
            _heldItem.SetAtSpawn();
            Debug.Log($"{LogPrefix} Objet '{_heldItemId}' Reset at spawn.");
            
            _heldItem = null;
            _heldItemId = null;
            _playerAnimation?.OnDroppedItem();
            return;
        }

        var origin = aimZone ? aimZone : transform;
        var alien = TargetingUtil.FindAlienInFront(origin, interactRadius, interactHalfFov, interactMask);

        var gaveItem = false;
        if (alien && alien.IsWithinReceiveRadius(origin.position))
        {
            gaveItem = alien.TryReceiveItem(_heldItemId);
            Debug.Log($"{LogPrefix} Don de '{_heldItemId}' à '{alien.name}' → succès={gaveItem}.");
        }

        if (gaveItem)
        {
            Destroy(_heldItem.gameObject);
        }
        else
        {
            var velocity = dropForwardSpeed > 0f ? transform.forward * dropForwardSpeed : Vector3.zero;
            _heldItem.Drop(velocity);
        }

        _heldItem = null;
        _heldItemId = null;
        _playerAnimation?.OnDroppedItem();
    }

    public void OnDrawGizmos()
    {
        if (handSocket != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handSocket.position, pickupRadius);
        }

        if (aimZone != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(aimZone.position, interactRadius);
            Gizmos.DrawLine(aimZone.position, aimZone.position + Quaternion.Euler(0f, interactHalfFov, 0f) * aimZone.forward * interactRadius);
            Gizmos.DrawLine(aimZone.position, aimZone.position + Quaternion.Euler(0f, -interactHalfFov, 0f) * aimZone.forward * interactRadius);
        }
    }

    private void ShowComboFeedback(EmotionType emotion, ActionType behavior)
    {
        // if (!comboBubble || emotion == Emotion.None || behavior == Behavior.None)
        // {
        //     return;
        // }
        //
        // if (comboLookup.Count == 0)
        // {
        //     RebuildComboLookup();
        //     if (comboLookup.Count == 0)
        //     {
        //         Debug.LogWarning($"{LogPrefix} Aucun combo disponible pour l'affichage de feedback.");
        //     }
        // }
        //
        //
        // if (comboLookup.TryGetValue(key, out var definition) && !string.IsNullOrWhiteSpace(definition.Symbols))
        // {
        //     var duration = definition.Duration > 0f ? definition.Duration : defaultComboBubbleDuration;
        //     // comboBubble.Show(definition.Emotion, definition.Symbols, duration);
        //
        //     return;
        // }
        //
        // if (DefaultBehaviorSymbols.TryGetValue(behavior, out var behaviorSymbol) &&
        //     DefaultEmotionSymbols.TryGetValue(emotion, out var emotionSymbol))
        // {
        //     comboBubble.Show(emotion, behaviorSymbol + emotionSymbol, defaultComboBubbleDuration);
        // }
        // else
        // {
        //     Debug.LogWarning($"{LogPrefix} Impossible de trouver un feedback pour le combo {behavior}/{emotion}.");
        // }
    }

    // private bool TryFindInteractionTarget(out IInteraction interaction)
    // {
    //     var origin = aimZone ? aimZone : transform;
    //     interaction = TargetingUtil.FindInteractionInFront(origin, interactRadius, interactHalfFov, interactMask);
    //     return interaction != null;
    // }
    //
    // private void UpdateInteractionZoneState()
    // {
    //     var hasInteraction = TryFindInteractionTarget(out _);
    //     if (hasInteraction == isInInteractionZone)
    //     {
    //         return;
    //     }
    //
    //     isInInteractionZone = hasInteraction;
    //     InteractionZoneChanged?.Invoke(isInInteractionZone);
    // }
}
