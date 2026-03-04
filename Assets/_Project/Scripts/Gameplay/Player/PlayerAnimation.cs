using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramIsGrabbing =  "IsGrabbing";
    [SerializeField] private string _paramGrab = "Grab";
    [SerializeField] private string _paramGive = "Give";
    
    private int _hashIsGrabbing;
    private int _hashGrab;
    private int _hashGive;
    
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    [SerializeField, Required] private PlayerInteraction _playerInteraction;

    protected override void Awake()
    {
        base.Awake();
        _hashIsGrabbing = Animator.StringToHash(_paramIsGrabbing);
        _hashGrab = Animator.StringToHash(_paramGrab);
        _hashGive = Animator.StringToHash(_paramGive);

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
        
        _playerInteraction.OnPickUp.AddListener(OnPickUp);
        _playerInteraction.OnGive.AddListener(OnGive);
        
        
        _playerInteraction.OnItemPickedUp.AddListener(OnPickedUpItem);
        _playerInteraction.OnItemDropped.AddListener(OnDroppedItem);
    }
    
    

    void OnDisable()
    {
        if (_playerInputSystem)
            _playerInputSystem.OnSynaptikInput -= OnPlayerAnimationChanged;

        if (!_playerInteraction) return;
        
        _playerInteraction.OnPickUp.RemoveListener(OnPickUp);
        _playerInteraction.OnGive.RemoveListener(OnGive);
        
        _playerInteraction.OnItemPickedUp.RemoveListener(OnPickedUpItem);
        _playerInteraction.OnItemDropped.RemoveListener(OnDroppedItem);
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

    public override void OnPunch()
    {
        base.OnPunch();
        
        float closestDistance = float.MaxValue;
        IInteraction nearbyInteraction = null;
        for (int i = 0; i < hitByPunchCount; i++)
        {
            if (!_punchCollider[i].TryGetComponent(out IInteraction interaction)) continue;
            float distance = Vector3.Distance(_punchSocket.position, _punchCollider[i].transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearbyInteraction = interaction;
            }
        }

        if (nearbyInteraction == null) return;
        OnHitByPunch?.Invoke();
        _playerInteraction?.HandlePunchImpact(nearbyInteraction);
    }

    // private void SetGrabbing(bool grabbing)
    //     => _animator.SetBool(_hashGrab, grabbing);

    private void OnPickUp()
    {
        _animator.SetTrigger(_hashGrab);
    }

    private void OnGive()
    {
        _animator.SetTrigger(_hashGive);
    }
    
    private void OnPickedUpItem()
    {
        _animator.SetBool(_hashIsGrabbing, true);
    }

    private void OnDroppedItem()
    {
        _animator.SetBool(_hashIsGrabbing, false);
    }
}