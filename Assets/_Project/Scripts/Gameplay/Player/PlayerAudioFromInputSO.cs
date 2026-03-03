using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public struct PlayerInteractionAudioData
{
    public AudioClip[] Clips;
    [Range(0f, 1f)] public float Volume;
    [Min(0f)] public float Delay;
}

[CreateAssetMenu(fileName = "PlayerInteractionAudio", menuName = "Synaptik/Player/PlayerInteractionAudio", order = 0)]
public class PlayerAudioFromInputSO : ScriptableObject
{
    [Tooltip("Dictionnaire : basé sur l'Emotion ET l'Action (Ex: Parler, Action)")]
    [SerializedDictionary("Input", "Audio Data")] 
    public SerializedDictionary<SynaptikInput, PlayerInteractionAudioData> AudioDataFromInput = new();
}
