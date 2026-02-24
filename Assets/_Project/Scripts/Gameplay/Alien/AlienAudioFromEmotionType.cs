using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public struct AlienEmotionAudioData
{
    public AudioClip Clip;
    [Min(0f)] public float Delay;
}

[CreateAssetMenu(fileName = "AlienAudioFromEmotion", menuName = "Synaptik/Alien/AlienAudio", order = 0)]
public class AlienAudioFromEmotionType : ScriptableObject
{
    [SerializedDictionary("EmotionType", "Audio Data")] 
    public SerializedDictionary<EmotionType, AlienEmotionAudioData> AudioDataFromEmotion = new();
}
