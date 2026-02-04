using UnityEngine;
using UnityEngine.Events;

public class AnimatorEventProxy : MonoBehaviour
{
    [SerializeField] private UnityEvent OnProxyEventTrigger;

    [SerializeField] private UnityEvent OnProxyCompletedEventTrigger;

    public void TriggerProxyEvent()
    {
        OnProxyEventTrigger?.Invoke();
    }

    public void TriggerProxyCompletedEvent()
    {
        OnProxyCompletedEventTrigger?.Invoke();
    }
}
