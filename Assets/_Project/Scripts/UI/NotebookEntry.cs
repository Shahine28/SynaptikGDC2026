using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class NotebookEntry : MonoBehaviour
{
    [Header("Color")] 
    [SerializeField] private Color _validateColor = Color.green; 
    
    
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
        titleText.color = isOn ? _validateColor : Color.black;
        descriptionText.color = isOn ? _validateColor : Color.black;
    }
}
