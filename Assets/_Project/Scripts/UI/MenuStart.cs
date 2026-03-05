using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
// using FMODUnity;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public sealed class MenuStart : MonoBehaviour
{
    [Header("Assign the UI")]
    [SerializeField] private RectTransform imageToShake;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject quitPanel;

    [Header("Shake Settings")]
    [SerializeField] private float maxShakeIntensity = 30f;
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent onFullyCharged;

    [Header("Barometer Settings")]
    [Tooltip("Aiguille du baromètre à faire pivoter.")]
    [SerializeField] private RectTransform needleTransform;

    [Tooltip("Rotation de l’aiguille quand charge = 0")]
    [SerializeField] private float needleMinRotation = 90f;

    [Tooltip("Rotation de l’aiguille quand charge = 100%")]
    [SerializeField] private float needleMaxRotation = -90f;

    [Tooltip("Vitesse de retour de l’aiguille à 0 quand la charge descend.")]
    [SerializeField] private float needleReturnSpeed = 3f;
    

    private float chargeProgress;
    private bool isCharging;
    private bool fullyCharged;
    private Vector3 originalPos;

    private bool panelHelpEnabled;
    private bool panelQuitEnabled;
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    private bool subscribedToInputs;

    private bool windowOpened;

    [SerializeField] private string sceneNameToLoad = "Proto_Scene_Final";


    private void Awake()
    {
        if (imageToShake != null)
            originalPos = imageToShake.anchoredPosition;

        InitializePanels();
    }

    private void OnEnable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnActionTriggered += HandleTwoAction;
        _playerInputSystem.OnActionTypeInput +=  HandleAction;
        _playerInputSystem.OnEmotionTypeInput +=  HandleEmotion;
    }

    private void OnDisable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnActionTriggered -= HandleTwoAction;
        _playerInputSystem.OnActionTypeInput -=  HandleAction;
        _playerInputSystem.OnEmotionTypeInput -=  HandleEmotion;
    }
    
    private void InitializePanels()
    {
        if (helpPanel) helpPanel.SetActive(false);
        if (quitPanel) quitPanel.SetActive(false);
        
        panelHelpEnabled = false;
        panelQuitEnabled = false;
    }
    

    private void HandleEmotion(EmotionType emotion)
    {
        if (emotion == EmotionType.Aggressive)
        {
            ToggleQuitPanel(true);
            ToggleHelpPanel(false);
        }
        else if (emotion == EmotionType.Curious)
        {
            ToggleQuitPanel(false);
            ToggleHelpPanel(true);
        }
        else
        {
            ToggleQuitPanel(false);
            ToggleHelpPanel(false);
        }
        
    }

    private void HandleAction(ActionType action)
    {
        if (!panelQuitEnabled || action == ActionType.None) 
            return;

        switch (action)
        {
            case ActionType.Action:
                HandleQuitChoice(true);
                break;
            
            case ActionType.Word:
                HandleQuitChoice(false);
                break;
        }
    }

    private void HandleTwoAction(bool twoPressed)
    {
        if (twoPressed) 
            StartCharging();
        else 
            StopCharging();
    }

    private void StartCharging()
    {
        isCharging = true;
        // if (_startEmitter)
        //     _startEmitter.Play();
        
        fullyCharged = false;
    }

    private void StopCharging()
    {
        isCharging = false;
        // if (_startEmitter)
        //     _startEmitter.Stop();
    }

    private void Update()
    {
        if (isCharging && !fullyCharged)
        {
            chargeProgress += Time.deltaTime / chargeTime;
            
            if (chargeProgress >= 1f)
            {
                chargeProgress = 1f;
                fullyCharged = true;
                
                onFullyCharged?.Invoke();
                // if (_startEmitter)
                //     _startEmitter.Stop();
            }
        }
        else
        {
            chargeProgress -= Time.deltaTime;
        }

        chargeProgress = Mathf.Clamp01(chargeProgress);

        // ---- SOUND ----
        // if (_startEmitter != null)
        //     _startEmitter.SetParameter("fuck", chargeProgress);
        

        if (imageToShake)
        {
            var intensity = intensityCurve.Evaluate(chargeProgress) * maxShakeIntensity;
            var offset = Random.insideUnitCircle * intensity;
            imageToShake.anchoredPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
        }

        if (chargeProgress <= 0.001f && imageToShake)
            imageToShake.anchoredPosition = originalPos;
        
        if (needleTransform)
        {
            float targetRotation = Mathf.Lerp(needleMinRotation, needleMaxRotation, chargeProgress);

            float currentRotation = Mathf.LerpAngle(
                needleTransform.localEulerAngles.z,
                targetRotation,
                Time.deltaTime * needleReturnSpeed
            );

            // --- VIBRATION / JITTER réaliste ---
            float vibrationStart = 0.1f;
            float vibrationProgress = Mathf.InverseLerp(vibrationStart, 1f, chargeProgress * 1.5f);
            float vibrationIntensity = Mathf.SmoothStep(0f, 1f, vibrationProgress);

            float slowNoise = (Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f) * 2f;
            float fastSine = Mathf.Sin(Time.time * 300f) * 0.25f;
            float modulated = fastSine * (0.3f + Mathf.Abs(slowNoise) * 0.7f);

            float jitterAmplitude = 1.5f;
            float jitter = modulated * jitterAmplitude * Mathf.Pow(vibrationIntensity, 1.5f);
            needleTransform.localEulerAngles = new Vector3(0f, 0f, currentRotation + jitter);
        }
    }

    private void ToggleQuitPanel(bool enable)
    {
        if (!quitPanel || panelHelpEnabled)
            return;
        
        panelQuitEnabled = enable;
        
        // if (enable)
        //     SoundManager.Instance.UIValid();
        // else
        //     SoundManager.Instance.UIInvalid();
        
        quitPanel.SetActive(panelQuitEnabled);
        
    }

    private void ToggleHelpPanel(bool enable)
    {
        if (!helpPanel || panelQuitEnabled) 
            return;
        
        panelHelpEnabled = enable;
        
        // if (panelHelpEnabled)
        //     SoundManager.Instance.UIValid();
        // else
        //     SoundManager.Instance.UIInvalid();
        
        helpPanel.SetActive(panelHelpEnabled);
        
    }

    private void HandleQuitChoice(bool accept)
    {
        if (!panelQuitEnabled) 
            return;

        if (accept)
        {
            Application.Quit();
        }
        else
        {
            ToggleQuitPanel(false);
        }
    }
    
    public void TestStart()
    {
        Debug.Log("TestStart");
        LoadingScreenManager.Instance?.LoadScene(sceneNameToLoad);
    }
}
