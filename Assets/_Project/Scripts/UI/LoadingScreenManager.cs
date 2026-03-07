using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private GameObject loadingScreenInstance;

    [Header("Durées")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDurationBeforeLoad = 0.5f;
    [SerializeField] private float holdDurationAfterSceneLoaded = 0.5f;

    [Header("Texte")]
    public List<string> textFiller = new();

    private LoadingText loadingText;
    private CanvasGroup currentCanvasGroup;

    private Coroutine currentLoadingInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (loadingScreenInstance)
        {
            currentCanvasGroup = loadingScreenInstance.GetComponentInChildren<CanvasGroup>();
            loadingText = loadingScreenInstance.GetComponentInChildren<LoadingText>();
            
            loadingScreenInstance.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"LoadingScreenManager : No Loading Screen referenced");
        }
    }

    #region Debug
    [Button]
    private void DebugLoadGameScene()
    {
        LoadScene("Proto_Scene_Final");
    }
    [Button]
    private void DebugLoadMenuScene()
    {
        LoadScene("MainMenu");
    }
    
    [Button]
    public void DebugResetScene()
    {
        SceneManager.LoadScene(0);
        CleanupLoadingInstance();
    }
    #endregion
    
    public void LoadScene(string sceneName)
    {
        if (currentLoadingInstance != null)
        {
            Debug.LogWarning("Une scène est déjà en cours de chargement.");
            return;
        }

        currentLoadingInstance = StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        loadingScreenInstance.gameObject.SetActive(true);

        currentCanvasGroup.alpha = 0f;
        loadingText?.StartText(textFiller);

        // On calcule la durée totale
        float totalTime = fadeDuration * 2 + holdDurationBeforeLoad + holdDurationAfterSceneLoaded;
        float elapsed = 0f;

        // On crée une fonction locale pour MAJ la progression
        void UpdateProgress()
        {
            float progress = Mathf.Clamp01(elapsed / totalTime);
            loadingText?.SetLoadingProgress(progress);
        }

        // --- Fade In ---
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            if (currentCanvasGroup)
                currentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            UpdateProgress();
            yield return null;
        }

        // --- Hold avant load ---
        t = 0f;
        while (t < holdDurationBeforeLoad)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            UpdateProgress();
            yield return null;
        }

        // --- Load Scene Async ---
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            t = Time.deltaTime;
            elapsed += t;
            UpdateProgress();
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);

        // --- Hold après load ---
        t = 0f;
        while (t < holdDurationAfterSceneLoaded)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            UpdateProgress();
            yield return null;
        }
        
        // Assure 100 %
        loadingText?.SetLoadingProgress(1f);
        
        // --- Fade Out ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            if (currentCanvasGroup)
                currentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            UpdateProgress();
            yield return null;
        }

        // --- Fin ---
        CleanupLoadingInstance();
    }

    private void CleanupLoadingInstance()
    {
        loadingText?.StopAll();
        loadingScreenInstance.gameObject.SetActive(false);
    }
}
