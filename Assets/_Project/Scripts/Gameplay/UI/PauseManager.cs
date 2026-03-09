using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the player's Input System. If null, it will be found automatically in the scene.")]
    public PlayerInputSystem playerInputSystem;

    [Header("Input Setup")]
    [Tooltip("L'Action Input Unity qui maintient le jeu actif (ex: capteur continu). S'il n'est plus activé, le jeu se met en pause.")]
    public InputActionReference pauseInputAction;

    [Header("UI Reference")]
    [Tooltip("The Canvas or GameObject containing the Pause Menu UI.")]
    [SerializeField] private GameObject _pauseMenuUI;

    [Header("Events")]
    public UnityEvent OnPause;
    public UnityEvent OnResume;

    private bool _isPaused = false;

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
        }
    }

    private void OnDisable()
    {
        if (pauseInputAction != null && pauseInputAction.action != null)
        {
            pauseInputAction.action.Disable();
        }
    }

    private void Update()
    {
        return;
        
        if (!playerInputSystem)
            return;

        bool isPauseInputHeld = pauseInputAction && pauseInputAction.action != null && pauseInputAction.action.IsPressed();

        if (!_isPaused)
        {
            if (!isPauseInputHeld)
            {
                SetPauseState(true);
            }
        }
        else
        {
            if (isPauseInputHeld)
            {
                SetPauseState(false);
            }
            
            if (playerInputSystem.IsActionInput)
            {
                QuitGame();
            }
        }
    }

    public void TogglePause()
    {
        SetPauseState(!_isPaused);
    }

    private void SetPauseState(bool shouldPause)
    {
        if (_isPaused == shouldPause)
            return;

        _isPaused = shouldPause;

        if (_isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
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

    private void PauseGame()
    {
        Time.timeScale = 0f;
        
        if (_pauseMenuUI)
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

        if (_pauseMenuUI)
        {
            _pauseMenuUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnResume?.Invoke();
        Debug.Log("[PauseManager] Game RESUMED.");
    }
}
