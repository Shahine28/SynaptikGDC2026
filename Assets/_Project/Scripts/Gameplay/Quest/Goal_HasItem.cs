using UnityEngine;


[CreateAssetMenu(fileName = "Goal_HasItem", menuName = "Quest/Goal Has Item")]
public class Goal_HasItem : QuestGoal
{
    [SerializeField] private WorldEntityID _targetOwner;
    [SerializeField] private ItemID _targetItem;

    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnInventoryChanged += OnInventoryCheck;
    }

    public override void Cleanup()
    {
        GameEvents.OnInventoryChanged -= OnInventoryCheck;
    }

    private void OnInventoryCheck(WorldEntityID owner, ItemID item, bool added)
    {
        if (added && owner == _targetOwner && item == _targetItem)
        {
            Complete();
        }
    }
}

