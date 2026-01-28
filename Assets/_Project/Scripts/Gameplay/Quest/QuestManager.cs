using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;


public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance {get; private set;}
    
    [SerializeField, Expandable] private List<QuestData> _quests;

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach (var quest in _quests)
        {
            quest.Initialize();
            quest.OnQuestCompleted += HandleQuestComplete;

        }
    }

    private void HandleQuestComplete(QuestData quest)
    {
        Debug.Log($"QUÊTE TERMINÉE : {quest.Title}");
        quest.OnQuestCompleted -= HandleQuestComplete;
    }
}
