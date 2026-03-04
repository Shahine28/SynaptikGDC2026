using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;


[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Info")]
    public string Title;
    [TextArea] public string DisplayDescription;

    [Range(-100, 100)] public float MisstrustModifier;
    [Header("Logic")]
    [Expandable]
    public List<QuestGoal> Goals;

    
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestUpdated;
    
    private bool _isCompleted;
    public bool IsCompleted => _isCompleted;

    public void Initialize()
    {
        _isCompleted = false;
        foreach (var goal in Goals)
        {
            goal.Initialize();
            goal.OnGoalMet += CheckCompletion;
        }
    }

    public void Cleanup()
    {
        foreach (var goal in Goals)
        {
            goal.Cleanup();
            goal.OnGoalMet -= CheckCompletion;
        }
    }

    private void CheckCompletion()
    {
        OnQuestUpdated?.Invoke(this);
        if (Goals.All(g => g.IsMet))
        {
            _isCompleted = true;
            OnQuestCompleted?.Invoke(this);
            MistrustManager.Instance?.AddMistrust(MisstrustModifier);
            Cleanup(); 
        }
    }
}
