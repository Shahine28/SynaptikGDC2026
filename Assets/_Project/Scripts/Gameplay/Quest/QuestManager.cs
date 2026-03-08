using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;


public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance {get; private set;}
    
    [SerializeField, SerializedDictionary("Quest", "UnityActionOnQuestCompleted")]
    public SerializedDictionary<QuestData, UnityEvent> _unityActionsFromQuests = new();

    [SerializeField] public UnityEvent OnAllQuestsCompleted;
    
    public List<QuestData> Quests => _unityActionsFromQuests.Keys.ToList();
    
    private int completedQuests = 0;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach (var quest in _unityActionsFromQuests.Keys)
        {
            quest.Initialize();
            quest.OnQuestCompleted += HandleQuestComplete;
        }
    }

    private void HandleQuestComplete(QuestData quest)
    {
        Debug.Log($"QUÊTE TERMINÉE : {quest.Title}");
        quest.OnQuestCompleted -= HandleQuestComplete;
        _unityActionsFromQuests[quest]?.Invoke();
        completedQuests++;

        if (completedQuests >= _unityActionsFromQuests.Count)
        {
            OnAllQuestsCompleted?.Invoke();
        }
    }
}
