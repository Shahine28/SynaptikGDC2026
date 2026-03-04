using System;
using System.Collections;
using UnityEngine;

public class AlienEmotionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AlienAudioFromEmotionType _alienAudioSO;
    [SerializeField] private AudioSource _audioSource;

    [Header("Overlap Settings")]
    [Tooltip("Si activé, coupe le son en cours pour jouer le nouveau. Si désactivé, ignore le nouveau son si un autre est déjà en train de jouer.")]
    [SerializeField] private bool _cutPreviousSound = true;
    [Tooltip("Délai minimum (en secondes) à attendre entre deux sons quand Cut Previous Sound est désactivé.")]
    [SerializeField] private float _spamCooldown = 0.5f;

    private SynaptikInput _currentInput;
    private Coroutine _playCoroutine;
    private bool _canPlayAudio = false;
    private float _nextAllowedPlayTime = 0f;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
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
            
        if (!_cutPreviousSound)
        {
            bool isPlayingAudio = _audioSource != null && _audioSource.isPlaying;
            bool isWaitingForDelay = _playCoroutine != null;
            if (isPlayingAudio || isWaitingForDelay || Time.time < _nextAllowedPlayTime)
            {
                return;
            }
        }
        
        Debug.Log($"[AudioResponse] Changer d'input de ({_currentInput.emotionType}, {_currentInput.actionType}) vers ({synaptikInput.emotionType}, {synaptikInput.actionType})");
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
                    float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

                    if (!_cutPreviousSound)
                    {
                        _nextAllowedPlayTime = Time.time + randomClip.length + audioData.Delay + _spamCooldown;
                    }

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
            if (_cutPreviousSound)
            {
                _audioSource.Stop();
            }
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.Play();
            Debug.Log($"[AudioResponse] Joué via AudioSource !");
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            Debug.Log($"[AudioResponse] Joué via PlayClipAtPoint ! (Pas d'AudioSource trouvée)");
        }
    }
}
