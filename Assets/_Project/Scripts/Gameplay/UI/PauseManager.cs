using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the player's Input System. If null, it will be found automatically in the scene.")]
    public PlayerInputSystem playerInputSystem;

    [Header("Input Setup")]
    [Tooltip("L'Action Input Unity pour déclencher le Menu Pause (ex: Escape/Start)")]
    public InputActionReference pauseInputAction;

    [Header("UI Reference")]
    [Tooltip("The Canvas or GameObject containing the Pause Menu UI.")]
    [SerializeField] private GameObject _pauseMenuUI;

    [Header("Events")]
    public UnityEvent OnPause;
    public UnityEvent OnResume;

    private bool _isPaused = false;

    private bool _wasTalkPressed = false;
    private bool _wasActionPressed = false;

    private void Start()
    {
        if (playerInputSystem == null)
        {
            playerInputSystem = FindObjectOfType<PlayerInputSystem>();
            if (playerInputSystem == null)
            {
                Debug.LogWarning("PauseManager: No PlayerInputSystem found in the scene!", this);
            }
        }

        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (pauseInputAction != null && pauseInputAction.action != null)
        {
            pauseInputAction.action.Enable();
            pauseInputAction.action.performed += OnPauseActionPerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseInputAction != null && pauseInputAction.action != null)
        {
            pauseInputAction.action.performed -= OnPauseActionPerformed;
            pauseInputAction.action.Disable();
        }
    }

    private void OnPauseActionPerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void Update()
    {
        if (!_isPaused || playerInputSystem == null)
            return;

        // --- GESTION DES INPUTS DANS LE MENU PAUSE ---
        
        bool currentTalk = playerInputSystem.IsTalkInput;
        bool currentAction = playerInputSystem.IsActionInput;

        if (currentTalk && !_wasTalkPressed)
        {
            TogglePause();
        }
        else if (currentAction && !_wasActionPressed)
        {
            QuitGame();
        }

        _wasTalkPressed = currentTalk;
        _wasActionPressed = currentAction;
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;

        if (_isPaused && playerInputSystem != null)
        {
            _wasTalkPressed = playerInputSystem.IsTalkInput;
            _wasActionPressed = playerInputSystem.IsActionInput;
        }

        if (_isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        
        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPause?.Invoke();
        Debug.Log("[PauseManager] Game PAUSED.");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;

        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnResume?.Invoke();
        Debug.Log("[PauseManager] Game RESUMED.");
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[PauseManager] QUIT GAME requested.");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
