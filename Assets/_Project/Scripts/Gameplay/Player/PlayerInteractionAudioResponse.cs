using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputSystem))]
public class PlayerInteractionAudioResponse : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerAudioFromInputSO _playerAudioSO;
    [SerializeField] private AudioSource _audioSource;

    private PlayerInputSystem _playerInputSystem;
    private SynaptikInput _currentInput;
    private Coroutine _playCoroutine;
    private bool _canPlayAudio = false;

    private void Awake()
    {
        _playerInputSystem = GetComponent<PlayerInputSystem>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
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
        // Petite sécurité pour ne pas jouer de sons lors du chargement initial/relâchement de boutons au spawn
        yield return new WaitForSeconds(0.2f);
        _canPlayAudio = true;
    }

    private void OnPlayerInput(SynaptikInput synaptikInput)
    {
        // On ne joue de son que s'il y a une vraie action et émotion
        if (!_canPlayAudio || _playerAudioSO == null || synaptikInput.actionType == ActionType.None || synaptikInput.emotionType == EmotionType.None)
            return;
        
        Debug.Log($"[PlayerAudioResponse] Input reçu : ({synaptikInput.emotionType}, {synaptikInput.actionType})");
        _currentInput = synaptikInput;

        if (_playerAudioSO.AudioDataFromInput != null && _playerAudioSO.AudioDataFromInput.TryGetValue(synaptikInput, out PlayerInteractionAudioData audioData))
        {
            if (audioData.Clips != null && audioData.Clips.Length > 0)
            {
                Debug.Log($"[PlayerAudioResponse] On a trouvé {audioData.Clips.Length} sons pour l'input ({synaptikInput.emotionType}, {synaptikInput.actionType})");
                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                }
                
                AudioClip randomClip = audioData.Clips[UnityEngine.Random.Range(0, audioData.Clips.Length)];
                
                if (randomClip != null)
                {
                    float volume = audioData.Volume <= 0f ? 1f : audioData.Volume;

                    Debug.Log($"[PlayerAudioResponse] Son choisi : {randomClip.name}. Délai : {audioData.Delay}s, Volume : {volume}");
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
            Debug.Log($"[PlayerAudioResponse] Aucune donnée audio trouvée pour l'input ({synaptikInput.emotionType}, {synaptikInput.actionType})", this);
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
        Debug.Log($"[PlayerAudioResponse] Lecture de {clip.name} au volume {volume}...");
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip, volume);
            Debug.Log($"[PlayerAudioResponse] Joué via AudioSource !");
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            Debug.Log($"[PlayerAudioResponse] Joué via PlayClipAtPoint !");
        }
    }
}
