using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class NoteBook : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField, Required] private Transform _noteboolEntryContainer;
    [SerializeField, Required] private NotebookEntry _notebookEntryPrefab;

    [Header("Quest Manager")]
    [SerializeField, Required] private QuestManager _questManager;
    
    private Dictionary<QuestData, NotebookEntry> _notebookEntryFromQuestData = new();
    
    [SerializeField, Required] SmoothListAnimator _smoothListAnimator;


    private void Start()
    {
        if (!_noteboolEntryContainer)
        {
            Debug.LogWarning("NoteBook: Note Container is missing");
            return;
        }

        if (!_notebookEntryPrefab)
        {
            Debug.LogWarning("NoteBook: NotebookEntryPrefab is missing");
            return;
        }

        foreach (QuestData quest in _questManager.Quests)
        {
            NotebookEntry notebookEntry = Instantiate(_notebookEntryPrefab, _noteboolEntryContainer);
            notebookEntry.Initialize(quest);
            quest.OnQuestCompleted += OnQuestCompleted;
            _notebookEntryFromQuestData.Add(quest, notebookEntry);
        }
    }

    private void OnEnable()
    {
        foreach (var variable in _notebookEntryFromQuestData)
        {
            variable.Key.OnQuestCompleted += OnQuestCompleted;
        }
    }

    private void OnDisable()
    {
        foreach (var variable in _notebookEntryFromQuestData)
        {
            variable.Key.OnQuestCompleted -= OnQuestCompleted;
        }
    }


    private void OnQuestCompleted(QuestData data)
    {
        _notebookEntryFromQuestData[data].SetToggle(data.IsCompleted);
        if (data.IsCompleted)
        {
            // _notebookEntryFromQuestData[data].transform.SetAsLastSibling();
            _smoothListAnimator?.MoveQuestToBottom(_notebookEntryFromQuestData[data].GetComponent<RectTransform>());
        }
    }
}
