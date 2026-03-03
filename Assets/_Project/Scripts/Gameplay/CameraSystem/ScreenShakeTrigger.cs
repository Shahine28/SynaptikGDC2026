using UnityEngine;

/// <summary>
/// Place this script on an object to easily trigger Screen Shakes.
/// Link it to UnityEvents (like OnClick(), Timeline Signals, Animation Events).
/// </summary>
public class ScreenShakeTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The CameraManager instance. If null, will try to find it in the scene automatically.")]
    public CameraManager cameraManager;

    [Header("Default Shake Settings")]
    [Tooltip("The default profile to play if none is specified.")]
    public ScreenShakeProfile defaultProfile;

    private void Start()
    {
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager == null)
            {
                Debug.LogWarning("ScreenShakeTrigger: No CameraManager found in the scene!", this);
            }
        }
    }

    /// <summary>
    /// Joue le ScreenShake avec le profil par défaut (utile pour les UnityEvents simples sans paramètres).
    /// </summary>
    [ContextMenu("Play Default Shake")]
    public void PlayDefaultShake()
    {
        if (defaultProfile != null)
        {
            PlayShakeFromProfile(defaultProfile);
        }
        else
        {
            Debug.LogWarning("ScreenShakeTrigger: Trying to play default shake but no Default Profile is assigned.", this);
        }
    }

    /// <summary>
    /// Joue le profil passé en paramètre.
    /// </summary>
    public void PlayShakeFromProfile(ScreenShakeProfile profile)
    {
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager == null) return;
        }

        if (profile == null) return;

        cameraManager.TriggerShake(profile.IntensityPos, profile.IntensityRot, profile.Duration);
    }

    /// <summary>
    /// Déclenche un shake avec des valeurs manuelles (utile pour des appels depuis d'autres scripts).
    /// </summary>
    public void PlayCustomShake(float intensityPos, float intensityRot, float duration)
    {
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager == null) return;
        }

        cameraManager.TriggerShake(intensityPos, intensityRot, duration);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ScreenShakeTrigger))]
public class ScreenShakeTriggerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ScreenShakeTrigger trigger = (ScreenShakeTrigger)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Play Default Shake (Test)", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                trigger.PlayDefaultShake();
            }
            else
            {
                Debug.LogWarning("ScreenShakeTrigger: You must be in Play Mode to test the Screen Shake!");
            }
        }
    }
}
#endif
