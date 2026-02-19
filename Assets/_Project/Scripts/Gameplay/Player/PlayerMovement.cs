using NaughtyAttributes;
using UnityEngine;

public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody _rb;
    [SerializeField, Required] private PlayerInputSystem _inputSystem;

    [Header("Settings - Movement")]
    [SerializeField, Min(0f)] private float _baseSpeed = 5f;
    [SerializeField, Min(0f)] private float _acceleration = 20f;
    [SerializeField, Min(0f)] private float _deceleration = 25f;
    [SerializeField, Min(0f)] private float _rotationSpeed = 10f;

    [Header("Settings - Camera")]
    [SerializeField] private bool _cameraRelative = true;
    [SerializeField, ShowIf(nameof(_cameraRelative))] private Camera _targetCamera;

    [Header("Settings - Ability (Fear Boost)")] 
    [SerializeField] private bool _activateBoostOnFear = false;
    [SerializeField, Min(0f)] private float _boostSpeedBonus = 3f;
    [SerializeField, Min(0f)] private float _boostDuration = 2f;
    [SerializeField, Min(0f)] private float _boostCooldown = 3f;
    [SerializeField, Min(0f)] private float _boostDecayTime = 1f;


    private float _currentSpeedBonus;
    private float _boostTimer;
    private float _cooldownTimer;
    
    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();
        _inputSystem = GetComponent<PlayerInputSystem>();
    }

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_inputSystem == null) _inputSystem = GetComponent<PlayerInputSystem>();
        if (_cameraRelative && _targetCamera == null) _targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (_inputSystem != null) 
            _inputSystem.OnSynaptikInput += TryActivateBoost;
    }

    private void OnDisable()
    {
        if (_inputSystem != null) 
            _inputSystem.OnSynaptikInput -= TryActivateBoost;
    }

    private void FixedUpdate()
    {
        HandleBoostTimers(Time.fixedDeltaTime);
        
        Vector2 input = _inputSystem.MoveInput;
        
        if (input.sqrMagnitude < 0.001f)
        {
            StopMovementAndRotation();
            return;
        }
        
        Vector3 targetDirection = CalculateMoveDirection(input); 
        ApplyMovementPhysics(targetDirection);
        ApplyRotation(targetDirection);
    }
    
    private void TryActivateBoost(SynaptikInput input)
    {
        bool canBoost = _cooldownTimer <= 0f;
        bool isFearAction = input.emotionType == EmotionType.Fearful && input.actionType == ActionType.Action;

        if (canBoost && isFearAction && _activateBoostOnFear)
        {
            ActivateBoost();
        }
    }

    private void ActivateBoost()
    {
        _currentSpeedBonus = _boostSpeedBonus;
        _boostTimer = _boostDuration;
        _cooldownTimer = _boostCooldown;
    }

    private void HandleBoostTimers(float deltaTime)
    {
        if (_cooldownTimer > 0f) 
            _cooldownTimer -= deltaTime;
        
        if (_boostTimer > 0f)
        {
            _boostTimer -= deltaTime;
        }
        else if (_currentSpeedBonus > 0f)
        {
            float decayRate = _boostSpeedBonus / _boostDecayTime;
            _currentSpeedBonus = Mathf.Max(0f, _currentSpeedBonus - decayRate * deltaTime);
        }
    }
    
    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        if (!_cameraRelative || !_targetCamera)
            return new Vector3(input.x, 0f, input.y).normalized;
        
        var cameraPlanarRotation = Quaternion.Euler(0, _targetCamera.transform.eulerAngles.y, 0);
        
        Vector3 direction = cameraPlanarRotation * new Vector3(input.x, 0f, input.y);
        return Vector3.ClampMagnitude(direction, 1f);
    }

    private void StopMovementAndRotation()
    {
        Vector3 currentVel = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(0f, currentVel.y, 0f);
        _rb.angularVelocity = Vector3.zero;
    }
    
    private void ApplyMovementPhysics(Vector3 direction)
    {
        float currentMaxSpeed = _baseSpeed + _currentSpeedBonus;


        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        
        Vector3 targetVelocity = direction * currentMaxSpeed;
        
        bool isTryingToMove = direction.sqrMagnitude > 0.01f;
        float speedChangeRate = isTryingToMove ? _acceleration : _deceleration;
        
        Vector3 newHorizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, speedChangeRate * Time.fixedDeltaTime);

        _rb.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);
    }

    private void ApplyRotation(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime * 100f);
    }
}