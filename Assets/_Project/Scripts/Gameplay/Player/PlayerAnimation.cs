using NaughtyAttributes;
using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramIsGrabbing = "IsGrabbing";
    private int _hashIsGrabbing;
    
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    [SerializeField, Required] private PlayerInteraction _playerInteraction;

    protected override void Awake()
    {
        base.Awake();
        _hashIsGrabbing = Animator.StringToHash(_paramIsGrabbing);

        if (!_playerInputSystem)
            _playerInputSystem = GetComponent<PlayerInputSystem>();
        
        if (!_playerInteraction)
            _playerInteraction = GetComponent<PlayerInteraction>();
    }

    void OnEnable()
    {
        if (_playerInputSystem)
            _playerInputSystem.OnSynaptikInput += OnPlayerAnimationChanged;

        if (!_playerInteraction) return;
        _playerInteraction.OnItemPickedUp += OnPickedUpItem;
        _playerInteraction.OnItemDropped += OnDroppedItem;
    }
    
    

    void OnDisable()
    {
        if (_playerInputSystem)
            _playerInputSystem.OnSynaptikInput -= OnPlayerAnimationChanged;

        if (!_playerInteraction) return;
        _playerInteraction.OnItemPickedUp -= OnPickedUpItem;
        _playerInteraction.OnItemDropped -= OnDroppedItem;
    }

    private void OnPlayerAnimationChanged(SynaptikInput synaptikInput)
    {
        if (synaptikInput.emotionType == EmotionType.None)
        {
            ClearAllEmotions();
            return;
        }

        if (synaptikInput is { emotionType : EmotionType.Aggressive, actionType: ActionType.Action })
        {
            PlayPunch();
        }
        else
        {
            SetEmotion(synaptikInput.emotionType);
        }
    }

    private void SetGrabbing(bool grabbing)
        => _animator.SetBool(_hashIsGrabbing, grabbing);

    private void OnPickedUpItem() => SetGrabbing(true);
    private void OnDroppedItem() => SetGrabbing(false);
}