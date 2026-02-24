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
        _currentEmotion = synaptikInput.emotionType;

        if (_alienAudioSO.AudioDataFromEmotion.TryGetValue(synaptikInput.emotionType, out AlienEmotionAudioData audioData))
        {
            if (audioData.Clips != null && audioData.Clips.Length > 0)
            {
                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                }
                
                AudioClip randomClip = audioData.Clips[UnityEngine.Random.Range(0, audioData.Clips.Length)];
                
                if (randomClip != null)
                {
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
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
