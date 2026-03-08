using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class MistrustManager : MonoBehaviour
{
    public static MistrustManager Instance { get; private set; }

    [SerializeField] private Slider mistrustSlider;
    [SerializeField, MinMaxSlider(-100f, 200f)] private Vector2 mistrustRange = new(0f, 100f);
    [SerializeField] private float initialMistrust = 50;

    private float _currentMistrustValue;
    public float CurrentMistrustValue => _currentMistrustValue;
    
    
    public static event Action<float, float> OnMistrustChanged;
    public UnityEvent OnMistrustMinReached; 
    public UnityEvent OnMistrustMaxReached; 

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        
        _currentMistrustValue = Mathf.Clamp(initialMistrust, mistrustRange.x, mistrustRange.y);
        if (mistrustSlider != null)
        {
            mistrustSlider.minValue = mistrustRange.x;
            mistrustSlider.maxValue = mistrustRange.y;
            mistrustSlider.value = _currentMistrustValue;
        }
    }

    public void AddMistrust(float amount)
    {
        UpdateMistrust(Mathf.Clamp(CurrentMistrustValue + amount, mistrustRange.x, mistrustRange.y));
    }

    public void RemoveMistrust(float amount)
    {
        UpdateMistrust(Mathf.Clamp(CurrentMistrustValue - amount, mistrustRange.x, mistrustRange.y));
    }

    public void ResetMistrust()
    {
        UpdateMistrust(initialMistrust);
    }
    

    private void UpdateMistrust(float newValue)
    {
        Debug.Log("Mistrust try call");
        OnMistrustChanged?.Invoke(_currentMistrustValue, newValue);
        _currentMistrustValue = newValue;
        mistrustSlider.value = _currentMistrustValue;
        

        if (_currentMistrustValue >= mistrustRange.y)
        {
            OnMistrustMaxReached?.Invoke();
        }
        else if (_currentMistrustValue <= mistrustRange.x)
        {
            OnMistrustMinReached?.Invoke();
        }
    }
}
