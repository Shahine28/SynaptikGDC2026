using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

// using FMODUnity;

public class DestroyableItem : MonoBehaviour, IInteraction
{ 
    [SerializeField] private DestroyableItemID _destroyableItemID;
    
    [SerializeField] private GameObject _itemToSpawnOnDestroy;
    
    [Tooltip("Si l'objet ne doit pas spawn au même endroit que le gameObject à détruire ou si le point de pivot du gameObject à spawn n'est pas bien placé")]
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private bool _destroyOnInteract = true;
    
    [SerializeField] private UnityEvent _onItemDestroyed;// Pour les GD
    [SerializeField, Tooltip("Appelé quand Action Type = Action et EmotionType = Agressive")] private UnityEvent _onItemInteracted;
    private Action _onItemDestroyedAction; // Pour nous
    
    
    private PlayerInteraction _lastPlayerInteraction;

    public void Interact(SynaptikInput action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (action is { actionType: ActionType.Action, emotionType: EmotionType.Aggressive })
        {
            _lastPlayerInteraction = playerInteraction;
            _onItemInteracted?.Invoke();
            if (_destroyOnInteract)
            {
                SimulateDestroy();
            }
        }
    }

    [Button("destroy")]
    public void SimulateDestroy()
    {
        if (_itemToSpawnOnDestroy)
        {
            if (_spawnTransform)
                Instantiate(_itemToSpawnOnDestroy, _spawnTransform.position, _spawnTransform.rotation);
            else
                Instantiate(_itemToSpawnOnDestroy, gameObject.transform.position, gameObject.transform.rotation);
        }

        if (_lastPlayerInteraction && _lastPlayerInteraction.TryGetComponent(out WorldEntity entity) && _destroyableItemID)
        {
            GameEvents.TriggerItemDestroyed(entity.EntityID, _destroyableItemID);
        }
        _onItemDestroyedAction?.Invoke();
        _onItemDestroyed?.Invoke();
            
        Destroy(gameObject);
    }
    
    
}
