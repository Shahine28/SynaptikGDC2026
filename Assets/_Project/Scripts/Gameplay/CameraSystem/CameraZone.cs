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
    // --- DETECTION ---
    [Header("Detection Settings")]
    public DetectionType detectionType = DetectionType.Circle;
    public Vector3 zoneOffset = Vector3.zero;
    
    // Circle
    [Min(0f)] public float zoneRadius = 5f;
    
    // Square (X, Z)
    public Vector2 zoneSize = new Vector2(10f, 10f);
    
    [Range(-10, 100)] public int weight = 0;

    // --- BEHAVIOR ---
    [Header("Camera Behavior")]
    public CameraBehavior behavior = CameraBehavior.Follow;
    
    [Tooltip("Temps pour atteindre la cible (plus petit = plus rapide). 0.1 = vif, 0.5 = lourd")]
    [Range(0.01f, 2f)] public float smoothTime = 0.2f;
    
    [Tooltip("Temps de transition ENTRANT dans cette zone")]
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

        Vector3 basePos = transform.position + viewOffset;
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
                Vector3 targetLookAt = (player.position + anticipation);
                state.Rotation = Quaternion.LookRotation(targetLookAt - state.Position);
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

    // --- LOGIQUE DETECTION ---

    public bool IsTargetInside(Vector3 targetPos)
    {

        Vector3 center = transform.position + transform.TransformDirection(zoneOffset);
        Vector2 pos2D = new Vector2(center.x, center.z);
        Vector2 target2D = new Vector2(targetPos.x, targetPos.z);

        if (detectionType == DetectionType.Circle)
        {
            return Vector2.Distance(pos2D, target2D) <= zoneRadius;
        }
        else // Square
        {
            float halfX = zoneSize.x * 0.5f;
            float halfZ = zoneSize.y * 0.5f;

            float dx = Mathf.Abs(pos2D.x - target2D.x);
            float dz = Mathf.Abs(pos2D.y - target2D.y);

            return dx <= halfX && dz <= halfZ;
        }
    }
    
    // --- EDITOR HELPERS ---

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
        }
#endif
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
        DrawZoneGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
        DrawZoneGizmos(true);
        DrawBehaviorGizmos();
    }

    private void DrawZoneGizmos(bool isSelected)
    {
        Vector3 center = transform.position + transform.TransformDirection(zoneOffset);

        if (detectionType == DetectionType.Circle)
        {
            Handles.color = isSelected ? new Color(0, 1, 0.5f, 0.4f) : new Color(0, 1, 0.5f, 0.1f);
            Handles.DrawSolidDisc(center, Vector3.up, zoneRadius);
            Handles.color = isSelected ? Color.green : new Color(0, 1, 0.5f, 0.5f);
            Handles.DrawWireDisc(center, Vector3.up, zoneRadius);
        }
        else
        {
            Vector3 size3D = new Vector3(zoneSize.x, 0, zoneSize.y);
            
            Gizmos.DrawWireCube(center, size3D);
            
            Color fill = Gizmos.color;
            fill.a = isSelected ? 0.3f : 0.1f;
            Gizmos.color = fill;
            Gizmos.DrawCube(center, size3D);
        }

        if (isSelected)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            Handles.Label(center + Vector3.up * 2f, $"ZONE: {name}\nWeight: {weight}", style);
        }
    }

    private void DrawBehaviorGizmos()
    {
        Vector3 center = transform.position;
        Vector3 camPos = Vector3.zero;
        Quaternion camRot = Quaternion.identity;

        // Visualise theoretical Camera Position based on current settings
        if (behavior == CameraBehavior.Static)
        {
            camPos = transform.position + viewOffset;
            camRot = transform.rotation * Quaternion.Euler(viewRotationOffset);
        }
        else if (behavior == CameraBehavior.LookAt)
        {
            camPos = transform.position + viewOffset;
            Vector3 dir = center - camPos;
            if (dir.sqrMagnitude > 0.001f)
                camRot = Quaternion.LookRotation(dir);
        }
        else // Follow or FollowAndLookAt
        {
            // Simulate camera at the offset position relative to the zone center
            camPos = center + followOffset;
            Vector3 dir = center - camPos;
            if (dir.sqrMagnitude > 0.001f)
                camRot = Quaternion.LookRotation(dir);
        }

        // Draw Line to Camera
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, camPos);

        // Draw Icon & Frustum
        Gizmos.DrawIcon(camPos, "Camera Gizmo", true);
        
        Gizmos.matrix = Matrix4x4.TRS(camPos, camRot, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, targetFOV, 10f, 0.5f, 1.77f);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraZone))]
public class CameraZoneEditor : Editor
{
    CameraZone _script;
    SerializedProperty _detectionType;
    SerializedProperty _zoneOffset;
    SerializedProperty _zoneRadius;
    SerializedProperty _zoneSize;
    SerializedProperty _weight;

    SerializedProperty _behavior;
    SerializedProperty _followOffset;
    SerializedProperty _viewOffset;

    private void OnEnable()
    {
        _script = (CameraZone)target;
        _detectionType = serializedObject.FindProperty("detectionType");
        _zoneOffset = serializedObject.FindProperty("zoneOffset");
        _zoneRadius = serializedObject.FindProperty("zoneRadius");
        _zoneSize = serializedObject.FindProperty("zoneSize");
        _weight = serializedObject.FindProperty("weight");
        _behavior = serializedObject.FindProperty("behavior");
        _followOffset = serializedObject.FindProperty("followOffset");
        _viewOffset = serializedObject.FindProperty("viewOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- header style ---
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        headerStyle.margin = new RectOffset(0, 0, 10, 5);

        // --- ZONE DETECTION ---
        GUILayout.Label("ZONE CONFIGURATION", headerStyle);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.PropertyField(_detectionType);
            EditorGUILayout.PropertyField(_weight);
            EditorGUILayout.PropertyField(_zoneOffset);

            EditorGUILayout.Space(5);
            
            if (_detectionType.enumValueIndex == (int)DetectionType.Circle)
            {
                EditorGUILayout.PropertyField(_zoneRadius);
            }
            else
            {
                EditorGUILayout.PropertyField(_zoneSize, new GUIContent("Box Size (X, Z)"));
            }
        }
        EditorGUILayout.EndVertical();

        // --- CAMERA BEHAVIOR ---
        GUILayout.Label("CAMERA BEHAVIOR", headerStyle);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.PropertyField(_behavior);
            
            CameraBehavior beh = (CameraBehavior)_behavior.enumValueIndex;
            
            if (beh == CameraBehavior.Static || beh == CameraBehavior.LookAt)
            {
                EditorGUILayout.PropertyField(_viewOffset);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("viewRotationOffset"));
            }
            
            if (beh == CameraBehavior.Follow || beh == CameraBehavior.FollowAndLookAt)
            {
                EditorGUILayout.PropertyField(_followOffset);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lockYAxis"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAheadAmount"));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Smoothing & Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothTimeTransition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetFOV"));
        }
        EditorGUILayout.EndVertical();

        // --- TOOLS ---
        GUILayout.Space(10);
        if (GUILayout.Button("Align Zone to Scene View Camera", GUILayout.Height(30)))
        {
            _script.AlignToView();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        CameraZone script = (CameraZone)target;
        
        // --- 1. Zone Center Handle ---
        Vector3 worldCenter = script.transform.position + script.transform.TransformDirection(script.zoneOffset);
        
        EditorGUI.BeginChangeCheck();
        Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Move Zone Center");
            script.zoneOffset = script.transform.InverseTransformDirection(newWorldCenter - script.transform.position);
        }

        // --- 2. Zone Size Handles (Radius / Box) ---
        Handles.color = Color.green;
        
        if (script.detectionType == DetectionType.Circle)
        {
            EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(Quaternion.identity, worldCenter, script.zoneRadius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Resize Zone Radius");
                script.zoneRadius = Mathf.Max(0, newRadius);
            }
        }
        else
        {
            Vector3 size3D = new Vector3(script.zoneSize.x, 0, script.zoneSize.y);
            Vector3 halfSize = size3D * 0.5f;

            // Width Handle (X)
            Vector3 rightHandle = worldCenter + Vector3.right * halfSize.x;
            EditorGUI.BeginChangeCheck();
            Vector3 newRight = Handles.Slider(rightHandle, Vector3.right, HandleUtility.GetHandleSize(rightHandle) * 0.1f, Handles.SphereHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Resize Zone Width");
                float dist = Vector3.Dot(newRight - worldCenter, Vector3.right);
                script.zoneSize.x = Mathf.Max(0, dist * 2f);
            }
            
            // Length Handle (Z)
            Vector3 forwardHandle = worldCenter + Vector3.forward * halfSize.z;
            EditorGUI.BeginChangeCheck();
            Vector3 newForward = Handles.Slider(forwardHandle, Vector3.forward, HandleUtility.GetHandleSize(forwardHandle) * 0.1f, Handles.SphereHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Resize Zone Length");
                float dist = Vector3.Dot(newForward - worldCenter, Vector3.forward);
                script.zoneSize.y = Mathf.Max(0, dist * 2f);
            }
        }

        // --- 3. Camera Offset Handles ---
        bool isStatic = (script.behavior == CameraBehavior.Static || script.behavior == CameraBehavior.LookAt);
        bool isFollow = (script.behavior == CameraBehavior.Follow || script.behavior == CameraBehavior.FollowAndLookAt);

        if (isStatic)
        {
            Vector3 camPos = script.transform.position + script.viewOffset;
            
            // Position Handle
            EditorGUI.BeginChangeCheck();
            Vector3 newCamPos = Handles.PositionHandle(camPos, script.transform.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Move Camera Offset");
                script.viewOffset = newCamPos - script.transform.position;
            }

            // Rotation Handle (Only Static needs fixed rotation, LookAt overrides it)
            if (script.behavior == CameraBehavior.Static)
            {
                Quaternion currentRot = script.transform.rotation * Quaternion.Euler(script.viewRotationOffset);
                EditorGUI.BeginChangeCheck();
                Quaternion newRot = Handles.RotationHandle(currentRot, camPos);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(script, "Rotate Camera Offset");
                    Quaternion delta = Quaternion.Inverse(script.transform.rotation) * newRot;
                    script.viewRotationOffset = delta.eulerAngles;
                }
            }
        }
        else if (isFollow)
        {
            // For Follow, we visualize the offset relative to the zone center
            Vector3 center = script.transform.position;
            Vector3 camPos = center + script.followOffset;

            EditorGUI.BeginChangeCheck();
            Vector3 newCamPos = Handles.PositionHandle(camPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                 Undo.RecordObject(script, "Move Follow Offset");
                 script.followOffset = newCamPos - center;
            }
        }
    }
}
#endif