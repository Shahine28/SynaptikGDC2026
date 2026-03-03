using UnityEngine;


[CreateAssetMenu(fileName = "Goal_EmotionState", menuName = "Quest/Goal Emotion State")]
public class Goal_EmotionState : QuestGoal
{
    [SerializeField] private WorldEntityID _targetEntity;
    [SerializeField] private EmotionType _requiredEmotion;

    public override void Initialize()
    {
        IsMet = false;
        GameEvents.OnEmotionChanged += OnEmotionChanged;
    }

    public override void Cleanup()
    {
        GameEvents.OnEmotionChanged -= OnEmotionChanged;
    }

    private void OnEmotionChanged(WorldEntityID entity, EmotionType emotion)
    {
        if (entity == _targetEntity && emotion == _requiredEmotion)
        {
            Complete();
        }
        else
        {
            Uncomplete();
        }
    }
}
