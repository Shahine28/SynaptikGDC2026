using UnityEngine;


[CreateAssetMenu(fileName = "Goal_DestroyItem", menuName = "Quest/Goal Destroy Item")]
public class Goal_DestroyItem : QuestGoal
{
    [SerializeField] private WorldEntityID _targetOwner;
    [SerializeField] private DestroyableItemID _targetItem;

    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnDestroyableItemDestroyed += OnItemDestroyed;
    }

    public override void Cleanup()
    {
        GameEvents.OnDestroyableItemDestroyed -= OnItemDestroyed;
    }

    private void OnItemDestroyed(WorldEntityID owner, DestroyableItemID itemDestroyableID)
    {
        if (owner == _targetOwner && itemDestroyableID == _targetItem)
        {
            Complete();
        }
    }
}

