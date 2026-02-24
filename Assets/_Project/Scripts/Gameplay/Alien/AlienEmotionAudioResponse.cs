using System;
using UnityEngine;

public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    private EmotionType _currentEmotion = EmotionType.None;

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

        if (_alienAudioSO.AudioClipFromEmotion.TryGetValue(synaptikInput.emotionType, out AudioClip clip))
        {
            if (clip != null)
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
    }
}
