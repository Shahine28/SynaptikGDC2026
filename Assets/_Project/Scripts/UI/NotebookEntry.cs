using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class NotebookEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private Toggle notebookToggle;

    public void Initialize(QuestData quest)
    {
        if (titleText)
        {
            titleText.text = quest.Title;
        }

        if (descriptionText)
        {
            descriptionText.text = quest.DisplayDescription;
        }

        SetToggle(quest.IsCompleted);
    }

    
    public void SetToggle(bool isOn)
    {
        if (!notebookToggle)
            return;

        notebookToggle.isOn = isOn;
    }
}
