using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CameraManager : MonoBehaviour
{
    // --- EVENTS ---
    public event Action<CameraZone> OnZoneChanged;
    public event Action OnEnterDefaultZone;
    public event Action OnExitDefaultZone;
    
    [Header("Main Rendering")]
    public Camera mainRenderCamera; 
    public Transform playerTarget;
    
    [Header("Zones Setup")]
    public CameraZone defaultZone;
    public List<CameraZone> zones;

    [Header("Collision (Anti-Clip)")]
    public LayerMask obstacleMask;
    public float wallBuffer = 0.5f;
    
    [Header("Transition Settings")]
    public float transitionThreshold = 0.1f;

    private Vector3 _currentVelocityPos;
    private Vector3 _currentVelocityRot;
    private float _currentVelocityFOV;

    private Vector3 _lastPlayerPos;
    private Vector3 _playerVelocity;

    private float _shakeTimer;
    private float _shakeIntensity;
    private Vector3 _shakeOffset;

    private CameraZone _currentZone;
    private bool _isTransitioning = false;

    void Start()
    {
        if (mainRenderCamera == null)
            mainRenderCamera = Camera.main;
        
        _currentZone = defaultZone;
        
        if (playerTarget) 
            _lastPlayerPos = playerTarget.position;
    }

    void LateUpdate()
    {
        if (!playerTarget || !mainRenderCamera) 
            return;

        _playerVelocity = (playerTarget.position - _lastPlayerPos) / Time.deltaTime;
        _lastPlayerPos = playerTarget.position;

        FindBestZone();

        CameraZone sourceZone = (_currentZone) ? _currentZone : defaultZone;
        if (sourceZone)
        {
            CameraZone.CameraState targetState = sourceZone.CalculateTargetState(playerTarget, _playerVelocity);
            Vector3 finalTargetPos = HandleObstacles(targetState.Position, playerTarget.position);

            UpdateShake();
            finalTargetPos += _shakeOffset;

            float distanceToTarget = Vector3.Distance(mainRenderCamera.transform.position, finalTargetPos);

            if (_isTransitioning && distanceToTarget <= transitionThreshold)
            {
                _isTransitioning = false;
            }
            
            float activeSmoothTime = _isTransitioning ? targetState.SmoothTimeTransition : targetState.SmoothTime;
            ApplySmoothMotion(finalTargetPos, targetState.Rotation, targetState.FOV, activeSmoothTime);
        }
    }

    Vector3 HandleObstacles(Vector3 desiredCamPos, Vector3 targetPos)
    {
        RaycastHit hit;
        Vector3 direction = desiredCamPos - targetPos;
        float dist = direction.magnitude;

        if (Physics.Raycast(targetPos, direction.normalized, out hit, dist, obstacleMask))
        {
            return hit.point - (direction.normalized * wallBuffer);
        }
        
        return desiredCamPos;
    }

    void ApplySmoothMotion(Vector3 targetPos, Quaternion targetRot, float targetFOV, float currentSmoothTime)
    {
        mainRenderCamera.transform.position = Vector3.SmoothDamp(
            mainRenderCamera.transform.position, 
            targetPos, 
            ref _currentVelocityPos, 
            currentSmoothTime
        );
        
        Quaternion currentRot = mainRenderCamera.transform.rotation;
        float rotSpeed = 1f / Mathf.Max(currentSmoothTime, 0.01f);
        
        mainRenderCamera.transform.rotation = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * rotSpeed * 4f);

        mainRenderCamera.fieldOfView = Mathf.SmoothDamp(
            mainRenderCamera.fieldOfView, 
            targetFOV, 
            ref _currentVelocityFOV, 
            currentSmoothTime
        );
    }

    void FindBestZone()
    {
        CameraZone bestZone = null;
        int highestWeight = -1;

        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].IsTargetInside(playerTarget.position))
            {
                if (zones[i].weight > highestWeight)
                {
                    highestWeight = zones[i].weight;
                    bestZone = zones[i];
                }
            }
        }

        CameraZone targetZone = (bestZone) ? bestZone : defaultZone;
        
        if (targetZone != _currentZone)
        {
            bool isNewDefault = (targetZone == defaultZone);
            bool wasDefault = (_currentZone == defaultZone);

            _currentZone = targetZone;
            
            _isTransitioning = true;

            OnZoneChanged?.Invoke(_currentZone);

            if (isNewDefault && !wasDefault)
            {
                OnEnterDefaultZone?.Invoke();
            }
            else if (!isNewDefault && wasDefault)
            {
                OnExitDefaultZone?.Invoke();
            }
        }
    }

    public void TriggerShake(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeTimer = duration;
    }

    void UpdateShake()
    {
        if (_shakeTimer > 0)
        {
            _shakeOffset = Random.insideUnitSphere * _shakeIntensity;
            _shakeTimer -= Time.deltaTime;
        }
        else
        {
            _shakeOffset = Vector3.zero;
        }
    }
}