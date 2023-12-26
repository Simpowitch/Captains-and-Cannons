using UnityEditor;

//Simon Voss
//Custom editor that displays the score (strength) of the selected character

[CustomEditor(typeof(CharacterBlueprint), true)]
public class CharacterBlueprintEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CharacterBlueprint character = (CharacterBlueprint) target;

        EditorGUILayout.HelpBox("Score: " + character.GetBlueprintData().GetScore().ToString(), MessageType.Info);
    }
}
