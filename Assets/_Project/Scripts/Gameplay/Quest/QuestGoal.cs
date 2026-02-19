using System;
using NaughtyAttributes;
using UnityEngine;


public abstract class QuestGoal : ScriptableObject
{
    [TextArea] public string Description = "Description du goal";
    
    [SerializeField, ReadOnly] private bool _isMet;

    public bool IsMet 
    { 
        get => _isMet; 
        protected set => _isMet = value; 
    }
    
    public abstract void Initialize();
    
    public abstract void Cleanup();
    

    public event Action OnGoalMet;
    
    protected void Complete()
    {
        if (IsMet) return;
        IsMet = true;
        OnGoalMet?.Invoke();
    }
    
    protected void Uncomplete()
    {
        if (!IsMet) return;
        IsMet = false;
        OnGoalMet?.Invoke();
    }
}
