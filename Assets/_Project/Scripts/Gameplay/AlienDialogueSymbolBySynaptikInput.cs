using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(fileName = "AlienDialogueSymbolBySynaptikInput", menuName = "Synaptik/Alien/AlienDialogueSymbol")]
public class AlienDialogueSymbolBySynaptikInput : ScriptableObject
{
    public SerializedDictionary<SynaptikInput, AlienDialogueAndTrust> SymbolFromSynaptikInput =  new SerializedDictionary<SynaptikInput, AlienDialogueAndTrust>();
    
    
    public string GetDialogue(SynaptikInput input)
    {
        var sb = new System.Text.StringBuilder();
        
        if (SymbolFromSynaptikInput.TryGetValue(input, out var emotionSymbol))
            sb.Append(emotionSymbol.Symbol);
        
        return sb.ToString();
    }

    public int GetMissTrustModifier(SynaptikInput input)
    {
        return SymbolFromSynaptikInput.TryGetValue(input, out var emotionSymbol) 
            ? emotionSymbol.MisstrustModifier 
            : 0;
    }

    [Serializable]
    public class AlienDialogueAndTrust
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
