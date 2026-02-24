using System;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private ZoneID _zoneID;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out WorldEntity worldEntity))
        {
            // Debug.Log("Player entered the angry zone!");
            GameEvents.TriggerZoneEntered(worldEntity.EntityID, _zoneID);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out WorldEntity worldEntity))
        {
            // Debug.Log("Player exited the angry zone!");
            GameEvents.TriggerZoneExited(worldEntity.EntityID, _zoneID);
            
        }
    }
}
