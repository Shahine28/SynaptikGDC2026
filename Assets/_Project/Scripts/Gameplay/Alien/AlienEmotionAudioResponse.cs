using System;
using System.Collections;
using UnityEngine;

public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    private EmotionType _currentEmotion = EmotionType.None;
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
        
        // Eviter de spammer le son si l'émotion ne change pas
        if (_currentEmotion == synaptikInput.emotionType) return;
        
        Debug.Log($"[AudioResponse] Changer d'émotion de {_currentEmotion} vers {synaptikInput.emotionType}");
        _currentEmotion = synaptikInput.emotionType;

        if (_alienAudioSO.AudioDataFromEmotion.TryGetValue(synaptikInput.emotionType, out AlienEmotionAudioData audioData))
        {
            if (audioData.Clips != null && audioData.Clips.Length > 0)
            {
                Debug.Log($"[AudioResponse] On a trouvé {audioData.Clips.Length} sons pour l'émotion {synaptikInput.emotionType}");
                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                }
                
                AudioClip randomClip = audioData.Clips[UnityEngine.Random.Range(0, audioData.Clips.Length)];
                
                if (randomClip != null)
                {
                    Debug.Log($"[AudioResponse] Son choisi : {randomClip.name}. Délai : {audioData.Delay}s");
                    if (audioData.Delay > 0f)
                    {
                        _playCoroutine = StartCoroutine(PlayWithDelayRoutine(randomClip, audioData.Delay));
                    }
                    else
                    {
                        PlayAudio(randomClip);
                    }
                }
            }
        }
    }

    private IEnumerator PlayWithDelayRoutine(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayAudio(clip);
        _playCoroutine = null;
    }

    private void PlayAudio(AudioClip clip)
    {
        Debug.Log($"[AudioResponse] Lecture de {clip.name}...");
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
            Debug.Log($"[AudioResponse] Joué via AudioSource !");
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
            Debug.Log($"[AudioResponse] Joué via PlayClipAtPoint ! (Pas d'AudioSource trouvée)");
        }
    }
}
