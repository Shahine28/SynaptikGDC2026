using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(fileName = "AlienColorFromEmotion", menuName = "Synaptik/Alien/AlienColor", order = 0)]
public class AlienColorFromEmotionType : ScriptableObject
{
    [SerializedDictionary("EmotionType", "AlienColor")] 
    public SerializedDictionary<EmotionType, Color> AlienColorFromEmotion = new();
    
    [SerializedDictionary("ActionType", "AlienColor")]
    public SerializedDictionary<ActionType, Color> AlienColorFromAction = new();
}
