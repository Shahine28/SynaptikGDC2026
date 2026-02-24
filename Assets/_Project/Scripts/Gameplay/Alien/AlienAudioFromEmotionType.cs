using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "AlienAudioFromEmotion", menuName = "Synaptik/Alien/AlienAudio", order = 0)]
public class AlienAudioFromEmotionType : ScriptableObject
{
    [SerializedDictionary("EmotionType", "AudioClip")] 
    public SerializedDictionary<EmotionType, AudioClip> AudioClipFromEmotion = new();
}
