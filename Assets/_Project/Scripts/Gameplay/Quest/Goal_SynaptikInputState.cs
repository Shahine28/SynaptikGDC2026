using UnityEngine;


[CreateAssetMenu(fileName = "Goal_SynaptikInputState", menuName = "Quest/ Goal Synaptik Input State")]
public class Goal_SynaptikInputState : QuestGoal
{
    [SerializeField] private WorldEntityID _targetEntity;
    [SerializeField] private SynaptikInput _requiredSynaptikInput;
    
    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnSynaptikInputChanged += OnSynaptikInputChanged;
    }

    public override void Cleanup()
    {
        GameEvents.OnSynaptikInputChanged -= OnSynaptikInputChanged;
    }

    private void OnSynaptikInputChanged(WorldEntityID entity, SynaptikInput synaptikInput)
    {
        if (entity == _targetEntity && synaptikInput.actionType == _requiredSynaptikInput.actionType && synaptikInput.emotionType == _requiredSynaptikInput.emotionType)
        {
            Complete();
        }
        else
        {
            Uncomplete();
        }
    }
}
