using NaughtyAttributes;
using UnityEngine;


public sealed class PlayerEmotionVisuals : AlienEmotionVisuals
{
    [SerializeField, Required] private PlayerInputSystem _inputSystem;

    protected override void Reset()
    {
        _inputSystem = GetComponent<PlayerInputSystem>();
        base.Reset();
    }

    protected override void Awake()
    {
        base.Awake();
        if (!_inputSystem)
            _inputSystem = GetComponent<PlayerInputSystem>();
    }

    private void OnEnable()
    {
        if (_inputSystem == null) return;
        _inputSystem.OnSynaptikInput += OnEmotionColorChanged;
    }
    
    private void OnDisable()
    {
        if (_inputSystem == null) return;
        _inputSystem.OnSynaptikInput -= OnEmotionColorChanged;
    }
}
