using UnityEngine;

[CreateAssetMenu(fileName = "Goal_AnimationPlayed", menuName = "Quest/Goal Animation Played")]
public class Goal_AnimationPlayed : QuestGoal
{
    [SerializeField] private WorldEntityID _targetEntity;

    [Tooltip("Nom de l'action (ex: 'Slipped', 'Vomit', 'Dance')")]
    [SerializeField] private string _requiredActionName;

    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnAnimationAction += OnAnimCheck;
    }

    public override void Cleanup()
    {
        GameEvents.OnAnimationAction -= OnAnimCheck;
    }

    private void OnAnimCheck(WorldEntityID entity, string actionName)
    {
        if (entity == _targetEntity && actionName == _requiredActionName)
        {
            Complete();
        }
        else
        {
            Uncomplete();
        }
    }
}
