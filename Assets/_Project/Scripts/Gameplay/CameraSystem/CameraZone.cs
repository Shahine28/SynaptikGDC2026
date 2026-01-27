using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum DetectionType
{
    Circle, 
    Square
}

public enum CameraBehavior
{
    Static, 
    Follow, 
    LookAt, 
    FollowAndLookAt
}

public class CameraZone : MonoBehaviour
{
    [Header("Detection")]
    public DetectionType detectionType = DetectionType.Circle;
    public float size = 5f;
    public int weight = 0;

    [Header("Behavior")]
    public CameraBehavior behavior = CameraBehavior.Follow;
    [Tooltip("Temps pour atteindre la cible (plus petit = plus rapide). 0.1 = vif, 0.5 = lourd")]
    [Range(0.01f, 2f)] public float smoothTime = 0.2f; 
    [Range(10f, 120f)] public float targetFOV = 60f;

    [Header("Follow Settings")]
    public Vector3 followOffset = new Vector3(0, 10, -10);
    [Tooltip("La caméra regarde 'devant' le joueur selon sa vitesse")]
    [Range(0f, 5f)] public float lookAheadAmount = 0f;
    public bool lockYAxis = false;

    [Header("Static / LookAt Settings")]
    public Vector3 viewOffset = Vector3.zero;

    // Données internes
    [HideInInspector] public Vector3 staticPosition;
    [HideInInspector] public Vector3 staticRotationEuler;

    public struct CameraState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float FOV;
        public float SmoothTime;
    }

    public CameraState CalculateTargetState(Transform player, Vector3 playerVelocity)
    {
        CameraState state = new CameraState();
        state.FOV = targetFOV;
        state.SmoothTime = smoothTime;

        Vector3 basePos = transform.position + transform.TransformDirection(viewOffset);
        
        Vector3 anticipation = Vector3.zero;
        if (lookAheadAmount > 0 && playerVelocity.magnitude > 0.1f)
        {
            anticipation = playerVelocity.normalized * lookAheadAmount;
            anticipation.y = 0; 
        }

        switch (behavior)
        {
            case CameraBehavior.Static:
                state.Position = basePos;
                state.Rotation = transform.rotation;
                break;

            case CameraBehavior.LookAt:
                state.Position = basePos;
                state.Rotation = Quaternion.LookRotation((player.position + anticipation) - state.Position);
                break;

            case CameraBehavior.Follow:
                Vector3 desiredPos = player.position + followOffset + anticipation;
                if (lockYAxis) 
                    desiredPos.y = basePos.y;
                
                state.Position = desiredPos;
                state.Rotation = Quaternion.LookRotation((player.position + anticipation) - state.Position); 
                break;

            case CameraBehavior.FollowAndLookAt:
                Vector3 fPos = player.position + followOffset + anticipation;
                if (lockYAxis) 
                    fPos.y = basePos.y;
                
                state.Position = fPos;
                state.Rotation = Quaternion.LookRotation((player.position + anticipation) - state.Position);
                break;
        }

        return state;
    }

    // --- EDITOR BUTTONS ---
    
    public void AlignToView()
    {
#if UNITY_EDITOR
        var view = SceneView.lastActiveSceneView.camera;
        if (view)
        {
            Undo.RecordObject(transform, "Align Zone to View");
            
            transform.position = view.transform.position;
            transform.rotation = view.transform.rotation;
            viewOffset = Vector3.zero;
            
            staticPosition = transform.position;
            staticRotationEuler = transform.rotation.eulerAngles;
        }
#endif
    }

    public bool IsTargetInside(Vector3 targetPos)
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 target2D = new Vector2(targetPos.x, targetPos.z);

        if (detectionType == DetectionType.Circle)
            return Vector2.Distance(pos2D, target2D) <= size;
        
        return Mathf.Abs(pos2D.x - target2D.x) <= size && Mathf.Abs(pos2D.y - target2D.y) <= size;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.15f);
        if (detectionType == DetectionType.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(size * 2, 0.2f, size * 2));
        }

        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
        Vector3 staticCamPos = transform.position + transform.TransformDirection(viewOffset);

        if (behavior == CameraBehavior.Static)
        {
            Gizmos.matrix = Matrix4x4.TRS(staticCamPos, transform.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, targetFOV, 3f, 0.1f, 1.77f);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else if (behavior == CameraBehavior.LookAt)
        {
            Gizmos.DrawWireSphere(staticCamPos, 0.5f);
            Gizmos.DrawLine(staticCamPos, transform.position); 
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }
        else if (behavior == CameraBehavior.Follow || behavior == CameraBehavior.FollowAndLookAt)
        {
            Vector3 followPos = transform.position + followOffset;
            Gizmos.DrawWireSphere(followPos, 0.5f);
            Gizmos.DrawLine(transform.position, followPos);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraZone))]
public class CameraZoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CameraZone script = (CameraZone)target;
        
        GUILayout.Space(10);
        if (GUILayout.Button("Align Zone to Scene View", GUILayout.Height(30)))
        {
            script.AlignToView();
        }
    }
}
#endif