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

    [Header("Screen Shake Settings")]
    [Tooltip("Courbe définissant l'intensité du shake au fil du temps (0 à 1).")]
    public AnimationCurve shakeFalloffCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [Tooltip("Fréquence / vitesse d'oscillation du shake.")]
    public float shakeFrequency = 25f;

    // Internal state for smoothing
    private Vector3 _currentVelocityPos;
    private Vector3 _currentVelocityRot;
    private float _currentVelocityFOV;

    private Vector3 _lastPlayerPos;
    private Vector3 _playerVelocity;

    // Internal shake state
    private float _shakeDuration;
    private float _shakeTimer;
    private float _shakeIntensityPos;
    private float _shakeIntensityRot;
    private Vector3 _shakeOffsetPos;
    private Vector3 _shakeOffsetRot;
    private float _shakeSeedX;
    private float _shakeSeedY;
    private float _shakeSeedZ;

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
            finalTargetPos += _shakeOffsetPos;
            Quaternion finalTargetRot = targetState.Rotation * Quaternion.Euler(_shakeOffsetRot);

            float distanceToTarget = Vector3.Distance(mainRenderCamera.transform.position, finalTargetPos);

            if (_isTransitioning && distanceToTarget <= transitionThreshold)
            {
                _isTransitioning = false;
            }
            
            float activeSmoothTime = _isTransitioning ? targetState.SmoothTimeTransition : targetState.SmoothTime;
            ApplySmoothMotion(finalTargetPos, finalTargetRot, targetState.FOV, activeSmoothTime);
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

    /// <summary>
    /// Déclenche un tremblement de caméra (Screen Shake).
    /// </summary>
    /// <param name="intensityPos">Intensité du tremblement sur la position.</param>
    /// <param name="intensityRot">Intensité du tremblement sur la rotation (en degrés).</param>
    /// <param name="duration">Durée du tremblement en secondes.</param>
    public void TriggerShake(float intensityPos, float intensityRot, float duration)
    {
        // Si un shake est déjà en cours, on prend le plus fort et on reset le timer
        _shakeIntensityPos = Mathf.Max(_shakeIntensityPos, intensityPos);
        _shakeIntensityRot = Mathf.Max(_shakeIntensityRot, intensityRot);
        _shakeDuration = duration;
        _shakeTimer = duration;

        // Génération de nouvelles seeds pour le Perlin Noise
        _shakeSeedX = Random.Range(0f, 1000f);
        _shakeSeedY = Random.Range(0f, 1000f);
        _shakeSeedZ = Random.Range(0f, 1000f);
    }

    void UpdateShake()
    {
        if (_shakeTimer > 0 && _shakeDuration > 0)
        {
            _shakeTimer -= Time.deltaTime;
            
            // Calcul de la progression (de 0 à 1)
            float t = 1f - (_shakeTimer / _shakeDuration);
            float falloffMult = shakeFalloffCurve.Evaluate(t);

            // Utilisation de Mathf.PerlinNoise pour un shake fluide et organique
            float timeFreq = Time.time * shakeFrequency;

            float offsetX = (Mathf.PerlinNoise(_shakeSeedX, timeFreq) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(_shakeSeedY, timeFreq) - 0.5f) * 2f;
            float offsetZ = (Mathf.PerlinNoise(_shakeSeedZ, timeFreq) - 0.5f) * 2f;

            _shakeOffsetPos = new Vector3(offsetX, offsetY, offsetZ) * (_shakeIntensityPos * falloffMult);
            _shakeOffsetRot = new Vector3(offsetX, offsetY, offsetZ) * (_shakeIntensityRot * falloffMult);
        }
        else
        {
            _shakeTimer = 0f;
            _shakeIntensityPos = 0f;
            _shakeIntensityRot = 0f;
            _shakeOffsetPos = Vector3.zero;
            _shakeOffsetRot = Vector3.zero;
        }
    }
}
