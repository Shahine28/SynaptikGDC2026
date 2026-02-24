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
            if (audioData.Clip != null)
            {
                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                }
                
                if (audioData.Delay > 0f)
                {
                    _playCoroutine = StartCoroutine(PlayWithDelayRoutine(audioData.Clip, audioData.Delay));
                }
                else
                {
                    PlayAudio(audioData.Clip);
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
