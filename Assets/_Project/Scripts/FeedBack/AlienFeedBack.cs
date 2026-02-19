using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct FeedBack
{
    public EmotionType emotion;
    public Color emotionColor;
    public string talkingReaction;
}

public sealed class AlienFeedBack : MonoBehaviour
{
    [Header("Feedbacks")]
    [SerializeField]
    private List<FeedBack> feedbackList = new();

    private readonly Dictionary<EmotionType, Color> feedbackColors = new();
    private readonly Dictionary<EmotionType, string> feedbackTalking = new();

    private void Start()
    {
        foreach (var feedback in feedbackList)
        {
            feedbackColors[feedback.emotion] = feedback.emotionColor;
            feedbackTalking[feedback.emotion] = feedback.talkingReaction;
        }
    }

    private void ActionFeedback(IAlienReaction alien, EmotionType emotion, ActionType behavior)
    {
        if (alien == null)
        {
            return;
        }

        if (feedbackColors.TryGetValue(emotion, out var color))
        {
            alien.FeedbackColor(color);
        }

        if (behavior == ActionType.Word && feedbackTalking.TryGetValue(emotion, out var reaction))
        {
            alien.FeedbackTalking(reaction);
        }
    }
}
