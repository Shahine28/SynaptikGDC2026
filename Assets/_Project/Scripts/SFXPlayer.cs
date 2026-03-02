
using AYellowpaper.SerializedCollections;
using UnityEngine;
using NaughtyAttributes;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField, Required] private AudioSource _audioSource;
    
    [SerializeField, SerializedDictionary("Clip Name", "Audio Clip")]
    private SerializedDictionary<string, AudioClip> _clipFromName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] string eventNameToUseForDebug;

    
    public void PlayOneshotByName(string eventName)
    {
        if (!_audioSource || !_clipFromName.ContainsKey(eventName)) return;
        
        AudioClip clip = _clipFromName[eventName];
        if (clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    
    [Button]
    public void DebugPlay()
    {
        PlayOneshotByName(eventNameToUseForDebug);
    }

    [Button]
    public void Stop()
    {
        if (!_audioSource || !_audioSource.isPlaying) return;
        _audioSource.Stop();
    }
}
