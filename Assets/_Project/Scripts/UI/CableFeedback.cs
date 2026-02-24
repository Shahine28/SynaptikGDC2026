using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public sealed class CableFeedback : MonoBehaviour
{
    [Header("Output References")]
    [SerializeField] private Image outputLeft;
    [SerializeField] private Image outputRight;

    [Header("Action Colors")]
    [SerializeField] private Color defaultActionColor = Color.white;
    
    [Header("Emotion Colors")]
    [SerializeField] private Color defaultEmotionColor = Color.white;
    
    [Header("Binding")]
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    [SerializeField, Required] private AlienColorFromEmotionType alienColorFromEmotion;

    void OnEnable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput += HandleSynaptikInput;
    }
    
    void OnDisable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput -= HandleSynaptikInput;
    }

    private void HandleSynaptikInput(SynaptikInput synaptikInput)
    {
        if (outputLeft)
        {
            outputLeft.color = synaptikInput.actionType == ActionType.None 
                ? defaultActionColor : alienColorFromEmotion.AlienColorFromAction[synaptikInput.actionType];
        }

        if (outputRight)
        {
            outputRight.color = synaptikInput.emotionType == EmotionType.None
                ? defaultEmotionColor : alienColorFromEmotion.AlienColorFromEmotion[synaptikInput.emotionType];
        }
    }
}
