using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public struct AlienEmotionAudioData
{
    public AudioClip[] Clips;
    [Range(0f, 1f)] public float Volume;
    [Min(0f)] public float Delay;
}

[CreateAssetMenu(fileName = "AlienAudioFromEmotion", menuName = "Synaptik/Alien/AlienAudio", order = 0)]
public class AlienAudioFromEmotionType : ScriptableObject
{
    [Tooltip("Dictionnaire obsolète : basé uniquement sur l'émotion")]
    [SerializedDictionary("EmotionType", "Audio Data")] 
    public SerializedDictionary<EmotionType, AlienEmotionAudioData> AudioDataFromEmotion = new();

    [Tooltip("Nouveau dictionnaire : basé sur l'Emotion ET l'Action (Ex: Parler, Action)")]
    [SerializedDictionary("Input", "Audio Data")] 
    public SerializedDictionary<SynaptikInput, AlienEmotionAudioData> AudioDataFromInput = new();
}
