using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputSystem))]
public class PlayerInteractionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerAudioFromInputSO _playerAudioSO;
    [SerializeField] private AudioSource _audioSource;

    [Header("Overlap Settings")]
    [Tooltip("Si activé, coupe le son en cours pour jouer le nouveau. Si désactivé, ignore le nouveau son si un autre est déjà en train de jouer.")]
    [SerializeField] private bool _cutPreviousSound = true;
    [Tooltip("Délai minimum (en secondes) à attendre entre deux sons quand Cut Previous Sound est désactivé.")]
    [SerializeField] private float _spamCooldown = 0.5f;

    private PlayerInputSystem _playerInputSystem;
    private SynaptikInput _currentInput;
    private Coroutine _playCoroutine;
    private bool _canPlayAudio = false;
    private float _nextAllowedPlayTime = 0f;
    private float _defaultVolume = 1f;

    private void Awake()
    {
        _playerInputSystem = GetComponent<PlayerInputSystem>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
            _defaultVolume = _audioSource.volume;
        }
        else
        {
            Debug.LogWarning("No AudioSource attached and none found on PlayerInteractionAudioResponse!", this);
        }
    }

    private void OnEnable()
    {
        if (_playerInputSystem != null)
        {
            _playerInputSystem.OnSynaptikInput += OnPlayerInput;
        }
    }

    private void OnDisable()
    {
        if (_playerInputSystem != null)
        {
            _playerInputSystem.OnSynaptikInput -= OnPlayerInput;
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        _canPlayAudio = true;
    }

    private void OnPlayerInput(SynaptikInput synaptikInput)
    {
        if (!_canPlayAudio || _playerAudioSO == null || synaptikInput.actionType == ActionType.None || synaptikInput.emotionType == EmotionType.None)
            return;

        if (!_cutPreviousSound)
        {
            bool isPlayingAudio = _audioSource != null && _audioSource.isPlaying;
            bool isWaitingForDelay = _playCoroutine != null;
            if (isPlayingAudio || isWaitingForDelay || Time.time < _nextAllowedPlayTime)
            {
                // Anti-spam en cours
                return;
            }
        }
        
        _currentInput = synaptikInput;

        if (_playerAudioSO.AudioDataFromInput != null && _playerAudioSO.AudioDataFromInput.TryGetValue(synaptikInput, out PlayerInteractionAudioData audioData))
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
                    float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

                    if (!_cutPreviousSound)
                    {
                        // On calcule le temps auquel le prochain son sera autorisé
                        _nextAllowedPlayTime = Time.time + randomClip.length + audioData.Delay + _spamCooldown;
                    }

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
        {
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
        if (_audioSource != null)
        {
            if (_cutPreviousSound)
            {
                _audioSource.Stop();
            }
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
