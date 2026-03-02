using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(fileName = "AlienDialogueSymbolBySynaptikInput", menuName = "Synaptik/Alien/AlienDialogueSymbol")]
public class AlienDialogueSymbolBySynaptikInput : ScriptableObject
{
    public SerializedDictionary<EmotionType, AlienDialogueAndTrust> SymbolFromEmotion = new()
    {
        { EmotionType.Aggressive, new AlienDialogueAndTrust("⚡", 0) },
        { EmotionType.Friendly, new AlienDialogueAndTrust("❤️",0) },
        { EmotionType.Curious, new AlienDialogueAndTrust("❓",0) },
        { EmotionType.Fearful, new AlienDialogueAndTrust("😱",0) }
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
            sb.Append(emotionSymbol.Symbol);
        
        return sb.ToString();
    }

    public int GetMissTrustModifier(EmotionType emotionType)
    {
        return SymbolFromEmotion.TryGetValue(emotionType, out var emotionSymbol) 
            ? emotionSymbol.MisstrustModifier 
            : 0;
    }

    [Serializable]
    public struct AlienDialogueAndTrust
    {
        public string Symbol;
        [Range(-100f, 100f)] public int MisstrustModifier;

        public AlienDialogueAndTrust(string s, int i)
        {
            Symbol = s;
            MisstrustModifier = i;
        }
    }
}
