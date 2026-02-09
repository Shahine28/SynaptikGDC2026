using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using FMODUnity;
using Random = UnityEngine.Random;

#region Struct&Enum

[System.Serializable]
struct SoundWithEmotion
{
    public EmotionType emotion;
    public EventReference eventReference;
}

[System.Serializable]
public enum VoicesModels
{
    RAND = 0,
    
    VOICE_A = 1,
    VOICE_T = 2,
    VOICE_P = 3
}

#endregion

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [SerializeField] private StudioBankLoader _Bank;
    [SerializeField] private EventReference _DebugSound;
    
    [Header("Emitters")]
    [SerializeField] private StudioEventEmitter _ambiantEmitter;
    [SerializeField] private StudioEventEmitter _musicEmitter;
    [SerializeField] private StudioEventEmitter _cameraSFXEmitter;
    
    [Header("Universal Sounds")]
    [SerializeField] private EventReference _UIValid;
    [SerializeField] private EventReference _UIInvalid;
    [Space(5)]
    [SerializeField] private EventReference _connectCables;
    
    [Header("Voices")]
    [SerializedDictionary("EmotionType", "EventReference"), SerializeField]
    private SerializedDictionary<EmotionType, EventReference> _voicesA = new SerializedDictionary<EmotionType, EventReference>();
    [SerializedDictionary("EmotionType", "EventReference"), SerializeField]
    private SerializedDictionary<EmotionType, EventReference> _voicesT = new SerializedDictionary<EmotionType, EventReference>();
    [SerializedDictionary("EmotionType", "EventReference"), SerializeField]
    private SerializedDictionary<EmotionType, EventReference> _voicesP = new SerializedDictionary<EmotionType, EventReference>();
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Constant Sounds

    public bool AmbiantChange(EventReference a_audioEvent, bool a_play = true)
    {
        if (a_audioEvent.IsNull)
            return false;
        
        AmbiantStop();
        _ambiantEmitter.EventReference = a_audioEvent;
        AmbiantPlay();
    
        return true;
    }
    public void AmbiantPlay()
    {
        if (_ambiantEmitter != null && !_ambiantEmitter.IsPlaying())
            _ambiantEmitter.Play();
    }
    public void AmbiantStop()
    {
        if (_ambiantEmitter != null && _ambiantEmitter.IsPlaying())
            _ambiantEmitter.Stop();
    }
    
    public bool MusicChange(EventReference a_audioEvent, bool a_play = true)
    {
        if (a_audioEvent.IsNull)
            return false;
        
        MusicStop();
        _musicEmitter.EventReference = a_audioEvent;
        MusicPlay();
    
        return true;
    }
    public void MusicPlay()
    {
        if (_musicEmitter != null && !_musicEmitter.IsPlaying())
            _musicEmitter.Play();
    }
    public void MusicStop()
    {
        if (_musicEmitter != null && _musicEmitter.IsPlaying())
            _musicEmitter.Stop();
    }
    #endregion
    
    #region Recurrent One Shots
    
    public void UIValid()
    {
        FMODUnity.RuntimeManager.PlayOneShot(_UIValid);
    }
    public void UIInvalid()
    {
        FMODUnity.RuntimeManager.PlayOneShot(_UIInvalid);
    }
    
    public void PlaySFX(EventReference a_sound)
    {
        FMODUnity.RuntimeManager.PlayOneShot(a_sound);
    }
    
    public void ConnectCables()
    {
        FMODUnity.RuntimeManager.PlayOneShot(_connectCables);
    }
    #endregion
    
    #region Voices
    
    public EventReference GetVoice(EmotionType a_emotion, VoicesModels a_voice = VoicesModels.RAND)
    {
        EventReference eventRef = new EventReference();

        if (a_voice == VoicesModels.RAND)
        {
            float rand = Random.Range(0f, 3f);
            switch (rand)
            {
                case <= 1f :
                    a_voice = VoicesModels.VOICE_A;
                    break;
                case <= 2f :
                    a_voice = VoicesModels.VOICE_P;
                    break;
                case <= 3f :
                    a_voice = VoicesModels.VOICE_T;
                    break;
            }
        }
        
        switch (a_voice)
        {
            case VoicesModels.VOICE_A:
                if (_voicesA.ContainsKey(a_emotion))
                    eventRef = _voicesA[a_emotion];
                break;
            case VoicesModels.VOICE_P:
                if (_voicesP.ContainsKey(a_emotion))
                    eventRef = _voicesP[a_emotion];
                break;
            case VoicesModels.VOICE_T:
                if (_voicesT.ContainsKey(a_emotion))
                    eventRef = _voicesT[a_emotion];
                break;
            
            default:
                eventRef = _DebugSound;
                break;
        }
        
        return eventRef;
    }
    
    #endregion
}
