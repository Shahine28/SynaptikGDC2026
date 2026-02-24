using UnityEngine;

[CreateAssetMenu(fileName = "Goal_EntityInZone", menuName = "Quest/Goal Entity In Zone", order = 0)]
public class Goal_EntityInZone : QuestGoal
{

    [SerializeField] private WorldEntityID _targetEntity;
    [SerializeField] private ZoneID _targetZoneID; 

    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnZoneEntered += OnZoneEntered;
        GameEvents.OnZoneExited += OnZoneExited;
    }

    public override void Cleanup()
    {
        GameEvents.OnZoneEntered -= OnZoneEntered;
        GameEvents.OnZoneExited -= OnZoneExited;
    }

    private void OnZoneEntered(WorldEntityID entity, ZoneID zoneID)
    {
        if (entity == _targetEntity && zoneID == _targetZoneID)
        {
            Complete();
        }
    }
    
    private void OnZoneExited(WorldEntityID entity, ZoneID zoneID)
    {
        if (entity == _targetEntity && zoneID == _targetZoneID)
        {
            Uncomplete();
        }
    }
}
