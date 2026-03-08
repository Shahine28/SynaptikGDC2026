using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleAudioEventBinder : MonoBehaviour
{
    [System.Serializable]
    public struct AudioEventBinding
    {
        [Tooltip("Nom descriptif pour le GD (ex: 'On Player Jump', 'On Item Pickup', etc.)")]
        public string EventName;
        
        [Tooltip("Le son qui sera joué lors du déclenchement de l'événement.")]
        public AudioClip Clip;
        
        [Tooltip("Variation du volume (0 = silence, 1 = volume max).")]
        [Range(0f, 1f)] public float VolumeScale;
        
        [Tooltip("Si vrai, le son sera joué en boucle.")]
        public bool Loop;

        [Tooltip("Si vrai, le son sera joué à la position de cet objet (utile si l'objet est détruit juste après).")]
        public bool PlayAtPoint;

        [Tooltip("Optionnel : Transform spécifique où jouer le son (remplace la position de cet objet).")]
        public Transform PositionOverride;

        [Tooltip("Si vrai, le son sera attaché/enfanté au PositionOverride (utile pour que le son suive un objet en mouvement).")]
        public bool AttachToTransform;
    }

    [Header("Configuration")]
    [Tooltip("L'AudioSource optionnelle à utiliser. Si vide, tentera d'utiliser celle de cet objet.")]
    [SerializeField] private AudioSource _audioSource;
    
    [Tooltip("L'AudioMixerGroup à utiliser lors d'un 'PlayAtPoint' (optionnel).")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup _audioMixerGroup;

    [Header("Overlap Settings")]
    [Tooltip("Cut = coupe le son en cours. Queue = file d'attente. Simultaneous = joue tous les sons en même temps.")]
    [SerializeField] private SoundOverlapMode _overlapMode = SoundOverlapMode.Cut;

    [Header("Bindings (GD Only)")]
    [Tooltip("Liste des sons disponibles qui pourront être appelés depuis des UnityEvents.")]
    [SerializeField] private AudioEventBinding[] _audioBindings;

    private float _defaultVolume = 1f;
    private readonly Queue<AudioEventBinding> _soundQueue = new Queue<AudioEventBinding>();
    private Coroutine _queueCoroutine;
    private AudioSource _dedicatedSource;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
            
        if (_audioSource != null)
        {
            _defaultVolume = _audioSource.volume;
            
            _dedicatedSource = gameObject.AddComponent<AudioSource>();
            _dedicatedSource.hideFlags = HideFlags.HideInInspector;
            _dedicatedSource.playOnAwake = false;
            _dedicatedSource.spatialBlend = _audioSource.spatialBlend;
            _dedicatedSource.minDistance = _audioSource.minDistance;
            _dedicatedSource.maxDistance = _audioSource.maxDistance;
            _dedicatedSource.rolloffMode = _audioSource.rolloffMode;
            _dedicatedSource.outputAudioMixerGroup = _audioMixerGroup != null ? _audioMixerGroup : _audioSource.outputAudioMixerGroup;

            if (_audioSource.spatialBlend == 0f)
            {
                Debug.LogWarning($"[Alerte 3D] Votre AudioSource sur {gameObject.name} a son 'Spatial Blend' à 0 (2D) ! Le son sera donc entendu partout à volume max. Mettez le curseur à 1 (3D) dans l'Inspecteur.", this);
            }
        }
        else
        {
            _defaultVolume = 1f;
            _dedicatedSource = gameObject.AddComponent<AudioSource>();
            _dedicatedSource.hideFlags = HideFlags.HideInInspector;
            _dedicatedSource.playOnAwake = false;
            _dedicatedSource.spatialBlend = 1f;
            _dedicatedSource.minDistance = 1f;
            _dedicatedSource.maxDistance = 20f;
            _dedicatedSource.rolloffMode = AudioRolloffMode.Linear;
            if (_audioMixerGroup != null)
                _dedicatedSource.outputAudioMixerGroup = _audioMixerGroup;
            
            _audioSource = _dedicatedSource;
        }
    }

    public void PlaySoundByIndex(int index)
    {
        if (_audioBindings == null || index < 0 || index >= _audioBindings.Length)
        {
            Debug.LogWarning($"[SimpleAudioEventBinder] Indice invalide ({index}) sur {gameObject.name}");
            return;
        }

        Play(_audioBindings[index]);
    }

    public void PlaySoundByName(string eventName)
    {
        if (_audioBindings == null) return;

        foreach (var binding in _audioBindings)
        {
            if (binding.EventName == eventName)
            {
                Play(binding);
                return;
            }
        }
        
        Debug.LogWarning($"[SimpleAudioEventBinder] Nom d'événement non trouvé ({eventName}) sur {gameObject.name}");
    }

    private void Play(AudioEventBinding binding)
    {
        if (binding.Clip == null)
        {
            Debug.LogWarning($"[SimpleAudioEventBinder] Le clip audio pour l'événement '{binding.EventName}' est NULL !");
            return;
        }

        switch (_overlapMode)
        {
            case SoundOverlapMode.Cut:
                PlayImmediate(binding);
                break;
            
            case SoundOverlapMode.Queue:
                _soundQueue.Enqueue(binding);
                if (_queueCoroutine == null)
                    _queueCoroutine = StartCoroutine(ProcessQueueRoutine());
                break;
            
            case SoundOverlapMode.Simultaneous:
                PlaySimultaneous(binding);
                break;
        }
    }

    private void PlayImmediate(AudioEventBinding binding)
    {
        float volume = Mathf.Clamp01(binding.VolumeScale > 0 ? binding.VolumeScale : 1f);

        if (binding.PlayAtPoint)
        {
            Vector3 pos = binding.PositionOverride != null ? binding.PositionOverride.position : transform.position;
            Transform parent = binding.AttachToTransform ? (binding.PositionOverride != null ? binding.PositionOverride : transform) : null;
            PlayClipAtPointCustom(binding.Clip, pos, volume, binding.Loop, _audioMixerGroup, parent);
        }
        else
        {
            _dedicatedSource.Stop();
            _dedicatedSource.clip = binding.Clip;
            _dedicatedSource.volume = Mathf.Clamp01(_defaultVolume * volume);
            _dedicatedSource.loop = binding.Loop;
            _dedicatedSource.Play();
        }
    }

    private void PlaySimultaneous(AudioEventBinding binding)
    {
        float volume = Mathf.Clamp01(binding.VolumeScale > 0 ? binding.VolumeScale : 1f);

        if (binding.PlayAtPoint)
        {
            Vector3 pos = binding.PositionOverride != null ? binding.PositionOverride.position : transform.position;
            Transform parent = binding.AttachToTransform ? (binding.PositionOverride != null ? binding.PositionOverride : transform) : null;
            PlayClipAtPointCustom(binding.Clip, pos, volume, binding.Loop, _audioMixerGroup, parent);
        }
        else
        {
            _dedicatedSource.volume = _defaultVolume;
            _dedicatedSource.PlayOneShot(binding.Clip, volume);
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (_soundQueue.Count > 0)
        {
            var binding = _soundQueue.Dequeue();
            if (!binding.Clip)
                continue;

            float volume = Mathf.Clamp01(binding.VolumeScale > 0 ? binding.VolumeScale : 1f);

            if (binding.PlayAtPoint)
            {
                Vector3 pos = binding.PositionOverride != null ? binding.PositionOverride.position : transform.position;
                Transform parent = binding.AttachToTransform ? (binding.PositionOverride != null ? binding.PositionOverride : transform) : null;
                PlayClipAtPointCustom(binding.Clip, pos, volume, binding.Loop, _audioMixerGroup, parent);
                if (!binding.Loop)
                    yield return new WaitForSeconds(binding.Clip.length);
            }
            else
            {
                _dedicatedSource.clip = binding.Clip;
                _dedicatedSource.volume = Mathf.Clamp01(_defaultVolume * volume);
                _dedicatedSource.loop = binding.Loop;
                _dedicatedSource.Play();

                if (!binding.Loop)
                    yield return new WaitForSeconds(binding.Clip.length);
            }
        }

        _queueCoroutine = null;
    }

    private void PlayClipAtPointCustom(AudioClip clip, Vector3 position, float volume, bool loop = false, UnityEngine.Audio.AudioMixerGroup mixerGroup = null, Transform parentObj = null)
    {
        if (clip == null) return;
        
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.position = position;
        
        if (parentObj != null)
            tempGO.transform.SetParent(parentObj);
        
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        
        if (_audioSource != null)
        {
            aSource.spatialBlend = _audioSource.spatialBlend;
            aSource.minDistance = _audioSource.minDistance;
            aSource.maxDistance = _audioSource.maxDistance;
            aSource.rolloffMode = _audioSource.rolloffMode;
            if (mixerGroup == null) mixerGroup = _audioSource.outputAudioMixerGroup;
        }
        else
        {
            aSource.spatialBlend = 1f;
            aSource.minDistance = 1f;
            aSource.maxDistance = 20f;
            aSource.rolloffMode = AudioRolloffMode.Linear;
        }

        aSource.volume = volume;
        aSource.loop = loop;
        
        if (mixerGroup != null)
        {
            aSource.outputAudioMixerGroup = mixerGroup;
        }
        
        aSource.Play();

        if (!loop)
        {
            Destroy(tempGO, clip.length + 5);
        }
    }
}
