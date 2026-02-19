using System;
using UnityEngine;
using UnityEngine.Events;

// using FMODUnity;

public class DestroyableItem : MonoBehaviour, IInteraction
{ 
    [SerializeField] private DestroyableItemID _destroyableItemID;
    
    [SerializeField] private GameObject _itemToSpawnOnDestroy;
    
    [Tooltip("Si l'objet ne doit pas spawn au même endroit que le gameObject à détruire ou si le point de pivot du gameObject à spawn n'est pas bien placé")]
    [SerializeField] private Transform _spawnTransform;
    
    
    [SerializeField] private UnityEvent _onItemDestroyed;// Pour les GD
    private Action _onItemDestroyedAction; // Pour nous
    

    public void Interact(SynaptikInput action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (action is { actionType: ActionType.Action, emotionType: EmotionType.Aggressive })
        {
            if (_itemToSpawnOnDestroy)
            {
                if (_spawnTransform)
                    Instantiate(_itemToSpawnOnDestroy, _spawnTransform.position, _spawnTransform.rotation);
                else
                    Instantiate(_itemToSpawnOnDestroy, gameObject.transform.position, gameObject.transform.rotation);
            }

            if (playerInteraction && playerInteraction.TryGetComponent(out WorldEntity entity) && _destroyableItemID)
            {
                GameEvents.TriggerItemDestroyed(entity.EntityID, _destroyableItemID);
            }
            
            _onItemDestroyedAction?.Invoke();
            _onItemDestroyed?.Invoke();
            
            Destroy(gameObject);
        }
    }
    
    
}
