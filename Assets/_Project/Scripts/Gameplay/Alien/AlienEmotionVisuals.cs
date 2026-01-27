using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


public class AlienEmotionVisuals : MonoBehaviour
{
    [Header("Configuration")] 
    [SerializeField, Required] private AlienColorFromEmotionType _alienColorSO; 
    [SerializeField] private Color _defaultColor = Color.white;
    
    [Header("Visual Settings")]
    [SerializeField] private Renderer[] _emotionRenderers = Array.Empty<Renderer>();
    [SerializeField, Min(0f)] private float _colorFadeDuration = 0.3f;
    
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _colorFadeCoroutine;
    private Color _currentColor;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");


    protected virtual void Reset()
    {
        EnsureRenderers();
    }

    protected virtual void Awake()
    {
        EnsureRenderers();
        _propertyBlock = new MaterialPropertyBlock();
        _currentColor = _defaultColor;
        ApplyColorToRenderers(_currentColor);
    }
    

    protected void EnsureRenderers()
    {
        if (_emotionRenderers != null && _emotionRenderers.Length > 0)
            return;
        if (_emotionRenderers == null || _emotionRenderers.Length == 0)
            _emotionRenderers = GetComponentsInChildren<Renderer>();

        if (_emotionRenderers == null)
        {
            Debug.LogError("No Renderers Assigned to PlayerEmotionVisuals");
        }
    }

    protected void OnEmotionColorChanged(SynaptikInput synaptikInput)
    {
        Color targetColor = _alienColorSO.AlienColorFromEmotion.GetValueOrDefault(synaptikInput.emotionType, _defaultColor);
        if (targetColor == _currentColor)
            return;
        
        if (_colorFadeCoroutine != null)
            StopCoroutine(_colorFadeCoroutine);

        if (_colorFadeDuration <= 0f)
        {
            _currentColor = targetColor;
            ApplyColorToRenderers(targetColor);
        }
        else
        {
            _colorFadeCoroutine = StartCoroutine(FadeRoutine(targetColor));
        }
    }

    private IEnumerator FadeRoutine(Color targetColor)
    {
        var startColor = _currentColor;
        float elapsed = 0f;

        while (elapsed < _colorFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _colorFadeDuration);
            
            _currentColor = Color.Lerp(startColor, targetColor, Mathf.SmoothStep(0f, 1f, t));
            ApplyColorToRenderers(_currentColor);
            yield return null;
        }
    
        _currentColor = targetColor;
        ApplyColorToRenderers(targetColor);
        _colorFadeCoroutine = null;
    }
    
    private void ApplyColorToRenderers(Color color)
    {
        if (_emotionRenderers == null) return;
        
        _propertyBlock.SetColor(BaseColorId, color);
        _propertyBlock.SetColor(ColorId, color);
        
        foreach (var renderer in _emotionRenderers)
        {
            if (renderer) 
                renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
