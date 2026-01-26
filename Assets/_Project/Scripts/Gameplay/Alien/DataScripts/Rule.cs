using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public struct InterractionRule
{
    [SerializeField]
    private ActionType channel;

    [SerializeField]
    private EmotionType playerEmotion;

    [SerializeField]
    private string questId;

    [SerializeField]
    private string questStepId;

    [SerializeField]
    private int suspicionDelta;

    [SerializeField]
    private bool setNewEmotion;

    [SerializeField]
    private EmotionType newEmotion;

    public ActionType Channel => channel;
    public EmotionType PlayerEmotion => playerEmotion;
    public string QuestId => questId;
    public string QuestStepId => questStepId;
    public int SuspicionDelta => suspicionDelta;
    public bool SetNewEmotion => setNewEmotion;
    public EmotionType NewEmotion => newEmotion;
}

[Serializable]
public struct ItemRule
{
    [Header("Item Reaction")]
    [SerializeField]
    private string questId;

    [SerializeField]
    private string questStepId;

    [SerializeField]
    private string expectedItemId;

    [SerializeField]
    private int suspicionDelta;

    [SerializeField]
    private int expectedItemQuantity;

    [SerializeField]
    private bool setIfGoodItem;

    [SerializeField]
    private EmotionType newEmotionIfGoodItem;

    public string QuestId => questId;
    public string QuestStepId => questStepId;
    public string ExpectedItemId => expectedItemId;
    public int SuspicionDelta => suspicionDelta;
    public int ExpectedItemQuantity => expectedItemQuantity <= 0 ? 1 : expectedItemQuantity;
    public bool SetIfGoodItem => setIfGoodItem;
    public EmotionType NewEmotionIfGoodItem => newEmotionIfGoodItem;
}
