using UnityEditor;

//Simon Voss
//Custom inspector that shows the score (strength) of an encounter by adding all characters' scores in the encounter

[CanEditMultipleObjects]
[CustomEditor(typeof(CombatEvent))]
public class CombatEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CombatEvent ce = (CombatEvent)target;

        EditorGUILayout.HelpBox("Score: " + ce.Score.ToString(), MessageType.Info);
    }
}
