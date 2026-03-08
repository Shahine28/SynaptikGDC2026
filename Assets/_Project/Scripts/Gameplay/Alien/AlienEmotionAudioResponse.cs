using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    [Header("Overlap Settings")]
    [Tooltip("Cut = coupe le son en cours. Queue = file d'attente. Simultaneous = joue tous les sons en même temps.")]
    [SerializeField] private SoundOverlapMode _overlapMode = SoundOverlapMode.Cut;

    private SynaptikInput _currentInput;
    private Coroutine _queueCoroutine;
    private bool _canPlayAudio = false;
    private float _defaultVolume = 1f;
    private readonly Queue<(AudioClip clip, float volume, float delay)> _soundQueue = new Queue<(AudioClip, float, float)>();
    private AudioSource _dedicatedSource;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
            _defaultVolume = _audioSource.volume;
            
            _dedicatedSource = gameObject.AddComponent<AudioSource>();
            _dedicatedSource.hideFlags = HideFlags.HideInInspector;
            _dedicatedSource.playOnAwake = false;
            _dedicatedSource.spatialBlend = _audioSource.spatialBlend;
            _dedicatedSource.minDistance = _audioSource.minDistance;
            _dedicatedSource.maxDistance = _audioSource.maxDistance;
            _dedicatedSource.rolloffMode = _audioSource.rolloffMode;
            _dedicatedSource.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
        }
        else
        {
            Debug.LogWarning("No AudioSource attached and none found on AlienEmotionAudioResponse!", this);
            _defaultVolume = 1f;
            _dedicatedSource = gameObject.AddComponent<AudioSource>();
            _dedicatedSource.hideFlags = HideFlags.HideInInspector;
            _dedicatedSource.playOnAwake = false;
            _dedicatedSource.spatialBlend = 1f;
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
        if (randomClip == null)
            return;

        float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

        switch (_overlapMode)
        {
            case SoundOverlapMode.Cut:
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
                
                break;

            case SoundOverlapMode.Queue:
                _soundQueue.Enqueue((randomClip, volume, audioData.Delay));
                if (_queueCoroutine == null)
                    _queueCoroutine = StartCoroutine(ProcessQueueRoutine());
                break;

            case SoundOverlapMode.Simultaneous:
                if (audioData.Delay > 0f)
                    StartCoroutine(PlaySimultaneousWithDelay(randomClip, volume, audioData.Delay));
                else
                    PlaySimultaneous(randomClip, volume);
                
                break;
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (_soundQueue.Count > 0)
        {
            var (clip, volume, delay) = _soundQueue.Dequeue();
            if (!clip)
                continue;

            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            PlayAudio(clip, volume);
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

    private IEnumerator PlaySimultaneousWithDelay(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySimultaneous(clip, volume);
    }

    private void PlayAudio(AudioClip clip, float volume)
    {
        _dedicatedSource.Stop();
        _dedicatedSource.clip = clip;
        _dedicatedSource.volume = Mathf.Clamp01(_defaultVolume * volume);
        _dedicatedSource.Play();
    }

    private void PlaySimultaneous(AudioClip clip, float volume)
    {
        _dedicatedSource.volume = _defaultVolume;
        _dedicatedSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
