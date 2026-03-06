using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    [Header("Overlap Settings")]
    [Tooltip("Si activé, coupe le son en cours pour jouer le nouveau immédiatement. Si désactivé, les sons sont mis en file d'attente et joués les uns après les autres.")]
    [SerializeField] private bool _cutPreviousSound = true;

    private SynaptikInput _currentInput;
    private Coroutine _queueCoroutine;
    private bool _canPlayAudio = false;
    private float _defaultVolume = 1f;
    private readonly Queue<(AudioClip clip, float volume, float delay)> _soundQueue = new Queue<(AudioClip, float, float)>();

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
            _defaultVolume = _audioSource.volume;
        }
        else
        {
            Debug.LogWarning("No AudioSource attached and none found on AlienEmotionAudioResponse!", this);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        _canPlayAudio = true;
    }

    public void OnEmotionChanged(SynaptikInput synaptikInput)
    {
        if (!_canPlayAudio || _alienAudioSO == null)
            return;
        
        if (_currentInput.emotionType == synaptikInput.emotionType && _currentInput.actionType == synaptikInput.actionType)
            return;

        _currentInput = synaptikInput;

        AlienEmotionAudioData audioData = default;
        bool hasFoundData = false;

        if (_alienAudioSO.AudioDataFromInput != null && _alienAudioSO.AudioDataFromInput.TryGetValue(synaptikInput, out var newAudioData))
        {
            audioData = newAudioData;
            hasFoundData = true;
        }
        else if (_alienAudioSO.AudioDataFromEmotion != null && _alienAudioSO.AudioDataFromEmotion.TryGetValue(synaptikInput.emotionType, out var oldAudioData))
        {
            audioData = oldAudioData;
            hasFoundData = true;
        }

        if (!hasFoundData || audioData.Clips == null || audioData.Clips.Length == 0)
            return;

        AudioClip randomClip = audioData.Clips[Random.Range(0, audioData.Clips.Length)];
        if (randomClip == null) return;

        float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

        if (_cutPreviousSound)
        {
            // Coupe l'ancien son et joue immédiatement
            if (_queueCoroutine != null)
            {
                StopCoroutine(_queueCoroutine);
                _queueCoroutine = null;
            }
            _soundQueue.Clear();

            if (audioData.Delay > 0f)
                _queueCoroutine = StartCoroutine(PlayWithDelayRoutine(randomClip, volume, audioData.Delay));
            else
                PlayAudio(randomClip, volume);
        }
        else
        {
            // Ajoute à la file d'attente
            _soundQueue.Enqueue((randomClip, volume, audioData.Delay));
            if (_queueCoroutine == null)
                _queueCoroutine = StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (_soundQueue.Count > 0)
        {
            var (clip, volume, delay) = _soundQueue.Dequeue();
            if (clip == null) continue;

            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            PlayAudio(clip, volume);

            // Attend la fin du clip avant de passer au suivant
            yield return new WaitForSeconds(clip.length);
        }

        _queueCoroutine = null;
    }

    private IEnumerator PlayWithDelayRoutine(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayAudio(clip, volume);
        _queueCoroutine = null;
    }

    private void PlayAudio(AudioClip clip, float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.clip = clip;
            _audioSource.volume = Mathf.Clamp01(_defaultVolume * volume);
            _audioSource.Play();
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, Mathf.Clamp01(volume));
        }
    }
}
