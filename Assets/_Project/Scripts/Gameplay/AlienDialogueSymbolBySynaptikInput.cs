using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(fileName = "AlienDialogueSymbolBySynaptikInput", menuName = "Synaptik/Alien/AlienDialogueSymbol")]
public class AlienDialogueSymbolBySynaptikInput : ScriptableObject
{
    public SerializedDictionary<EmotionType, string> SymbolFromEmotion = new()
    {
        { EmotionType.Aggressive, "⚡" },
        { EmotionType.Friendly, "❤️" },
        { EmotionType.Curious, "❓" },
        { EmotionType.Fearful, "😱" }
    };

    public SerializedDictionary<ActionType, string> SymbolFromAction = new()
    {
        { ActionType.Word, "💬" },
        { ActionType.Action, "✋" }
    };

    public string GetDialogue(SynaptikInput input)
    {
        var sb = new System.Text.StringBuilder();
        
        if (SymbolFromAction.TryGetValue(input.actionType, out var actionSymbol))
            sb.Append(actionSymbol);
        
        if (SymbolFromEmotion.TryGetValue(input.emotionType, out var emotionSymbol))
            sb.Append(emotionSymbol);
        
        return sb.ToString();
    }
}
