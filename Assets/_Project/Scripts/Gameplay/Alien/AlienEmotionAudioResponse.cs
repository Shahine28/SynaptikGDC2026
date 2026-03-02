using System;
using System.Collections;
using UnityEngine;

public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    private SynaptikInput _currentInput;
    private Coroutine _playCoroutine;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource == null)
            Debug.LogWarning("No AudioSource attached and none found on AlienEmotionAudioResponse!", this);
    }

    public void OnEmotionChanged(SynaptikInput synaptikInput)
    {
        if (_alienAudioSO == null) return;
        
        // Eviter de spammer le son si on a pas changé ni d'émotion, ni d'action
        if (_currentInput.emotionType == synaptikInput.emotionType && _currentInput.actionType == synaptikInput.actionType) return;
        
        Debug.Log($"[AudioResponse] Changer d'input de ({_currentInput.emotionType}, {_currentInput.actionType}) vers ({synaptikInput.emotionType}, {synaptikInput.actionType})");
        _currentInput = synaptikInput;

        AlienEmotionAudioData audioData = default;
        bool hasFoundData = false;

        // On cherche en priorité dans le nouveau dictionnaire (Emotion + Action)
        if (_alienAudioSO.AudioDataFromInput != null && _alienAudioSO.AudioDataFromInput.TryGetValue(synaptikInput, out var newAudioData))
        {
            audioData = newAudioData;
            hasFoundData = true;
        }
        // Sinon on fallback sur l'ancien dictionnaire (Emotion seule, pour rétrocompatibilité)
        else if (_alienAudioSO.AudioDataFromEmotion != null && _alienAudioSO.AudioDataFromEmotion.TryGetValue(synaptikInput.emotionType, out var oldAudioData))
        {
            audioData = oldAudioData;
            hasFoundData = true;
        }

        if (hasFoundData)
        {
            if (audioData.Clips != null && audioData.Clips.Length > 0)
            {
                Debug.Log($"[AudioResponse] On a trouvé {audioData.Clips.Length} sons pour l'input ({synaptikInput.emotionType}, {synaptikInput.actionType})");
                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                }
                
                AudioClip randomClip = audioData.Clips[UnityEngine.Random.Range(0, audioData.Clips.Length)];
                
                if (randomClip != null)
                {
                    // Rétrocompatibilité : si Volume est à 0 (cas des anciens SO non mis à jour) on le met à 1 par défaut
                    float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

                    Debug.Log($"[AudioResponse] Son choisi : {randomClip.name}. Délai : {audioData.Delay}s, Volume : {volume}");
                    if (audioData.Delay > 0f)
                    {
                        _playCoroutine = StartCoroutine(PlayWithDelayRoutine(randomClip, volume, audioData.Delay));
                    }
                    else
                    {
                        PlayAudio(randomClip, volume);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[AudioResponse] Aucune donnée audio trouvée pour l'input ({synaptikInput.emotionType}, {synaptikInput.actionType})", this);
        }
    }

    private IEnumerator PlayWithDelayRoutine(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayAudio(clip, volume);
        _playCoroutine = null;
    }

    private void PlayAudio(AudioClip clip, float volume)
    {
        Debug.Log($"[AudioResponse] Lecture de {clip.name} au volume {volume}...");
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip, volume);
            Debug.Log($"[AudioResponse] Joué via AudioSource !");
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            Debug.Log($"[AudioResponse] Joué via PlayClipAtPoint ! (Pas d'AudioSource trouvée)");
        }
    }
}
