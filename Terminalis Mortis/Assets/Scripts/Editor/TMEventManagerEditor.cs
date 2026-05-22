using UnityEditor;

[CustomEditor(typeof(TerminalisMortis.Events.TMEventManager))]
public class TMEventManagerEditor : Editor
{
    private SerializedProperty _onPlayerHit;
    private SerializedProperty _onEnemyHit;
    private SerializedProperty _onEnter;

    private bool _showCombat = true;
    private bool _showInput = true;

    private void OnEnable()
    {
        _onPlayerHit = serializedObject.FindProperty("onPlayerHit");
        _onEnemyHit = serializedObject.FindProperty("onEnemyHit");
        _onEnter = serializedObject.FindProperty("onEnter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("TM Event Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _showCombat = EditorGUILayout.Foldout(_showCombat, "Combat Events", true);
        if (_showCombat)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_onPlayerHit);
            EditorGUILayout.PropertyField(_onEnemyHit);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        _showInput = EditorGUILayout.Foldout(_showInput, "Input Events", true);
        if (_showInput)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_onEnter);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
