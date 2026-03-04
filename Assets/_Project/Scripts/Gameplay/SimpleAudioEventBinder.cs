using System;
using UnityEngine;
using UnityEngine.Events;


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
    }

    [Header("Configuration")]
    [Tooltip("L'AudioSource optionnelle à utiliser. Si vide, tentera d'utiliser celle de cet objet.")]
    [SerializeField] private AudioSource _audioSource;
    
    [Tooltip("L'AudioMixerGroup à utiliser lors d'un 'PlayAtPoint' (optionnel).")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup _audioMixerGroup;

    [Header("Bindings (GD Only)")]
    [Tooltip("Liste des sons disponibles qui pourront être appelés depuis des UnityEvents.")]
    [SerializeField] private AudioEventBinding[] _audioBindings;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundByIndex(int index)
    {
        Debug.Log($"[SimpleAudioEventBinder] PlaySoundByIndex appelé avec l'index {index}");
        if (_audioBindings == null || index < 0 || index >= _audioBindings.Length)
        {
            Debug.LogWarning($"[SimpleAudioEventBinder] Indice invalide ({index}) sur {gameObject.name}");
            return;
        }

        var binding = _audioBindings[index];
        Play(binding);
    }
    

    public void PlaySoundByName(string eventName)
    {
        Debug.Log($"[SimpleAudioEventBinder] PlaySoundByName appelé avec le nom '{eventName}'");
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

        float volume = binding.VolumeScale > 0 ? binding.VolumeScale : 1f;
        Debug.Log($"[SimpleAudioEventBinder] Lecture de {binding.Clip.name} à un volume de {volume}");

        if (binding.PlayAtPoint || _audioSource == null)
        {
            PlayClipAtPointCustom(binding.Clip, transform.position, volume, binding.Loop, _audioMixerGroup);
            Debug.Log($"[SimpleAudioEventBinder] Joué via PlayClipAtPoint ! (Mixer: {(_audioMixerGroup ? _audioMixerGroup.name : "None")}) - Loop: {binding.Loop}");
        }
        else
        {
            if (binding.Loop)
            {
                _audioSource.clip = binding.Clip;
                _audioSource.volume = volume;
                _audioSource.loop = true;
                _audioSource.Play();
                Debug.Log($"[SimpleAudioEventBinder] Joué en boucle via l'AudioSource du composant !");
            }
            else
            {
                _audioSource.PlayOneShot(binding.Clip, volume);
                Debug.Log($"[SimpleAudioEventBinder] Joué via l'AudioSource du composant !");
            }
        }
    }
    

    private static void PlayClipAtPointCustom(AudioClip clip, Vector3 position, float volume, bool loop = false, UnityEngine.Audio.AudioMixerGroup mixerGroup = null)
    {
        if (clip == null) return;
        
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.position = position;
        
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f;
        aSource.volume = volume;
        aSource.loop = loop;
        
        if (mixerGroup != null)
        {
            aSource.outputAudioMixerGroup = mixerGroup;
        }
        
        aSource.Play();

        if (!loop)
        {
            Destroy(tempGO, clip.length);
        }
    }
}
