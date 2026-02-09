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

    // Données internes pour editor tools
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

        // Position de base (localisée par rapport au transform de la zone)
        Vector3 basePos = transform.position + transform.TransformDirection(viewOffset);
        // Rotation de base
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
                // LookAt player position (plus anticipation)
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
                // Recalculate rotation to look at target from new position
                state.Rotation = Quaternion.LookRotation((player.position + anticipation) - state.Position);
                break;
        }

        return state;
    }

    // --- LOGIQUE DETECTION ---

    public bool IsTargetInside(Vector3 targetPos)
    {
        // 1. Transformer la position cible dans l'espace local de la zone
        //    pour gérer la rotation de la zone si besoin, ou simplement l'offset.
        //    Ici on travaille en World Space aligné ou Local Space ?
        //    Simplifions en utilisant la position transformée par l'offset.

        Vector3 center = transform.position + transform.TransformDirection(zoneOffset);
        
        // On ignore la hauteur (Y) pour la détection 2D au sol
        Vector2 pos2D = new Vector2(center.x, center.z);
        Vector2 target2D = new Vector2(targetPos.x, targetPos.z);

        if (detectionType == DetectionType.Circle)
        {
            return Vector2.Distance(pos2D, target2D) <= zoneRadius;
        }
        else // Square
        {
            float halfX = zoneSize.x * 0.5f;
            float halfZ = zoneSize.y * 0.5f; // zoneSize est Vector2(width, length)

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
        // Couleur de base (non sélectionné)
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
        DrawZoneGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        // Couleur sélectionnée
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
        DrawZoneGizmos(true);
        DrawBehaviorGizmos();
    }

    private void DrawZoneGizmos(bool isSelected)
    {
        Vector3 center = transform.position + transform.TransformDirection(zoneOffset);

        if (detectionType == DetectionType.Circle)
        {
            // Disque au sol
            Handles.color = isSelected ? new Color(0, 1, 0.5f, 0.4f) : new Color(0, 1, 0.5f, 0.1f);
            Handles.DrawSolidDisc(center, Vector3.up, zoneRadius);
            Handles.color = isSelected ? Color.green : new Color(0, 1, 0.5f, 0.5f);
            Handles.DrawWireDisc(center, Vector3.up, zoneRadius);
        }
        else
        {
            // Rectangle au sol
            Vector3 size3D = new Vector3(zoneSize.x, 0, zoneSize.y);
            
            // On peut dessiner un Cube aplati
            Gizmos.DrawWireCube(center, size3D);
            
            Color fill = Gizmos.color;
            fill.a = isSelected ? 0.3f : 0.1f;
            Gizmos.color = fill;
            Gizmos.DrawCube(center, size3D);
        }

        // Label
        if (isSelected)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(center + Vector3.up * 1f, $"ZONE: {name}\nWeight: {weight}", style);
        }
    }

    private void DrawBehaviorGizmos()
    {
        Vector3 center = transform.position;
        Vector3 camPos = behavior == CameraBehavior.Static || behavior == CameraBehavior.LookAt
            ? center + transform.TransformDirection(viewOffset)
            : center + followOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(camPos, 0.3f);
        Gizmos.DrawLine(center, camPos);
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
    // ... autres props si besoin

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
            
            // Show fields based on behavior
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
        
        // 1. Handle for Zone Offset
        Vector3 worldCenter = script.transform.position + script.transform.TransformDirection(script.zoneOffset);
        
        EditorGUI.BeginChangeCheck();
        Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Move Zone Center");
            // Convert back to local offset
            script.zoneOffset = script.transform.InverseTransformDirection(newWorldCenter - script.transform.position);
        }

        // 2. Handles for Size (Radius or Box)
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
            // Box Resize Handles
            Vector3 size3D = new Vector3(script.zoneSize.x, 0, script.zoneSize.y);
            Vector3 halfSize = size3D * 0.5f;

            // X Axis Handle
            Vector3 rightHandle = worldCenter + Vector3.right * halfSize.x;
            EditorGUI.BeginChangeCheck();
            Vector3 newRight = Handles.Slider(rightHandle, Vector3.right, HandleUtility.GetHandleSize(rightHandle) * 0.1f, Handles.SphereHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Resize Zone Width");
                float dist = Vector3.Dot(newRight - worldCenter, Vector3.right);
                script.zoneSize.x = Mathf.Max(0, dist * 2f);
            }
            
            // Z Axis Handle
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
    }
}
#endif