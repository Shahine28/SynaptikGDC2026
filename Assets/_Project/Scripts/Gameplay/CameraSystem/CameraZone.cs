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
    [Range(0.01f, 5f)] public float smoothTimeTransition = 1.5f; 
    [Range(10f, 120f)] public float targetFOV = 60f;

    [Header("Follow Settings")]
    public Vector3 followOffset = new Vector3(0, 10, -10);
    [Tooltip("La caméra regarde 'devant' le joueur selon sa vitesse")]
    [Range(0f, 5f)] public float lookAheadAmount = 0f;
    public bool lockYAxis = false;

    [Header("Static / LookAt Settings")]
    public Vector3 viewOffset = Vector3.zero;
    public Vector3 viewRotationOffset = Vector3.zero;

    // Données internes
    [HideInInspector] public Vector3 staticPosition;
    [HideInInspector] public Vector3 staticRotationEuler;

    public struct CameraState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float FOV;
        public float SmoothTime;
        public float SmoothTimeTransition;
    }

    public CameraState CalculateTargetState(Transform player, Vector3 playerVelocity)
    {
        CameraState state = new CameraState();
        state.FOV = targetFOV;
        state.SmoothTime = smoothTime;
        state.SmoothTimeTransition = smoothTimeTransition;

        Vector3 basePos = transform.position + transform.TransformDirection(viewOffset);
        Quaternion baseRot = transform.rotation * Quaternion.Euler(viewRotationOffset);
        
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
                state.Rotation = baseRot;
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
            viewRotationOffset = Vector3.zero;
            
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color zoneColor = new Color(0f, 1f, 0.2f, 0.1f);
        Color outlineColor = new Color(0f, 1f, 0.2f, 0.8f);

        Handles.color = zoneColor;
        Vector3 center = transform.position;

        if (detectionType == DetectionType.Circle)
        {
            Handles.DrawSolidDisc(center, Vector3.up, size);
            Handles.color = outlineColor;
            Handles.DrawWireDisc(center, Vector3.up, size);
        }
        else
        {
            Vector3[] verts = new Vector3[]
            {
                center + new Vector3(-size, 0, -size),
                center + new Vector3(size, 0, -size),
                center + new Vector3(size, 0, size),
                center + new Vector3(-size, 0, size)
            };
            
            Handles.DrawSolidRectangleWithOutline(verts, zoneColor, outlineColor);
        }

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Bold;
        
        Handles.Label(center + Vector3.up * 0.5f, $"{name}\n[{behavior}]", labelStyle);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        Vector3 staticCamPos = transform.position + transform.TransformDirection(viewOffset);
        Quaternion globalRot = transform.rotation * Quaternion.Euler(viewRotationOffset);
        
        Gizmos.color = new Color(1, 0.8f, 0, 1f);

        if (behavior == CameraBehavior.Static || behavior == CameraBehavior.LookAt)
        {
            Gizmos.DrawWireSphere(staticCamPos, 0.3f);

            if (behavior == CameraBehavior.Static)
            {
                Gizmos.matrix = Matrix4x4.TRS(staticCamPos, globalRot, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, targetFOV, 5f, 0.1f, 1.77f);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(staticCamPos, center);
                Gizmos.DrawWireCube(center, Vector3.one * 0.2f);

                Vector3 direction = center - staticCamPos;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    Gizmos.matrix = Matrix4x4.TRS(staticCamPos, lookRot, Vector3.one);
                    Gizmos.DrawFrustum(Vector3.zero, targetFOV, 5f, 0.1f, 1.77f);
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }

            if(viewOffset != Vector3.zero)
            {
                Gizmos.color = new Color(1,1,1, 0.3f);
                Gizmos.DrawLine(center, staticCamPos);
            }
        }
        else if (behavior == CameraBehavior.Follow || behavior == CameraBehavior.FollowAndLookAt)
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawWireCube(center + Vector3.up, new Vector3(0.5f, 2f, 0.5f));
            
            Vector3 camTargetPos = center + followOffset;
            if (lockYAxis) 
                camTargetPos.y = center.y + viewOffset.y;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center + Vector3.up, camTargetPos);
            Gizmos.DrawWireSphere(camTargetPos, 0.5f);
            
            Quaternion lookRot = Quaternion.LookRotation((center + Vector3.up) - camTargetPos);
            Gizmos.matrix = Matrix4x4.TRS(camTargetPos, lookRot, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, targetFOV, 3f, 0.1f, 1.77f);
            Gizmos.matrix = Matrix4x4.identity;

            if (lockYAxis)
            {
                Handles.color = new Color(1, 0, 0, 0.1f);
                Handles.DrawSolidDisc(new Vector3(center.x, center.y + viewOffset.y, center.z), Vector3.up, size * 1.2f);
                Handles.Label(camTargetPos + Vector3.up, "Locked Height Plane");
            }
        }
    }
#endif
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