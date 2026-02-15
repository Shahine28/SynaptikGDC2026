
using AYellowpaper.SerializedCollections;
using UnityEngine;
using FMODUnity;
using NaughtyAttributes;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField, Required] private StudioEventEmitter _eventEmitter;
    
    
    [SerializeField, SerializedDictionary("Clip Name", "Event Reference")]
    private SerializedDictionary<string, EventReference> _eventReferenceFromName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] string eventNameToUseForDebug;

    
    
    public void PlayOneshotByName(string eventName)
    {
        if (!_eventEmitter || !_eventReferenceFromName.ContainsKey(eventNameToUseForDebug)) return;
        _eventEmitter.EventReference = _eventReferenceFromName[eventName];
        _eventEmitter?.Play();
    }
    
    [Button]
    public void DebugPlay()
    {
        PlayOneshotByName(eventNameToUseForDebug);
    }

    [Button]
    public void Stop()
    {
        if (!_eventEmitter || !_eventEmitter.IsPlaying()) return;
        _eventEmitter.Stop();
    }
}
