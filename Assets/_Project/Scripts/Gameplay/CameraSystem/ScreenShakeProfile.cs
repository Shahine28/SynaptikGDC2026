using UnityEngine;

[CreateAssetMenu(fileName = "NewScreenShakeProfile", menuName = "Synaptik/Camera/Screen Shake Profile")]
public class ScreenShakeProfile : ScriptableObject
{
    [Header("Shake Settings")]
    [Tooltip("Intensité de la secousse sur la position (X, Y, Z).")]
    public float IntensityPos = 0.5f;

    [Tooltip("Intensité de la secousse sur la rotation en degrés (Pitch, Yaw, Roll).")]
    public float IntensityRot = 1.5f;

    [Tooltip("Durée de la secousse complète en secondes.")]
    public float Duration = 0.3f;
}
