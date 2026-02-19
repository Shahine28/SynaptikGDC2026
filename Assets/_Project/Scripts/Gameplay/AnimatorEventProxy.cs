using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorEventProxy : MonoBehaviour
{
    [SerializeField, SerializedDictionary("Event Name", "Unity Event")]
    SerializedDictionary<string, UnityEvent> _eventDictionaryByName;
    
    public void TriggerProxyEventByName(string eventName)
    {
        if (_eventDictionaryByName.ContainsKey(eventName))
        {
            _eventDictionaryByName[eventName]?.Invoke();
        }
    }
}
