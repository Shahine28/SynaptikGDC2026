using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VFXPlayer : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _particleSystems;
    
    public UnityEvent onVFXPlayed;
    public UnityEvent onVFXStopped;
    public UnityEvent onVFXPaused;
    
    public void PlayVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem?.Play();
        }
        onVFXPlayed?.Invoke();
    }
    public void StopVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem?.Stop();
        }
        onVFXStopped?.Invoke();
    }
    
    public void PauseVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem?.Pause();
        }
        onVFXPaused?.Invoke();
    }
}
