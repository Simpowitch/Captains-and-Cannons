using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Simon Voss
//Custom inspector of events to warn designers if wrongfully setup

[CustomEditor(typeof(NonCombatEvent))]
public class NonCombatEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NonCombatEvent e = (NonCombatEvent)target;

        DrawDefaultInspector();

        if (e.name == "")
        {
            EditorGUILayout.HelpBox("Missing name of event", MessageType.Error);
        }
        if (e.eventDescription == "")
        {
            EditorGUILayout.HelpBox("Missing description of event", MessageType.Error);
        }
        if (e.image == null)
        {
            EditorGUILayout.HelpBox("Missing image of event", MessageType.Error);
        }

        if (e.eventChoices != null && e.eventChoices.Length > 0)
        {
            foreach (var choice in e.eventChoices)
            {
                if (choice.choicePreDescription == "")
                {
                    EditorGUILayout.HelpBox("Missing pre-description of choice", MessageType.Error);
                }

                if (choice.choicePostDescription == "")
                {
                    EditorGUILayout.HelpBox("Missing post-description of choice", MessageType.Error);
                }

                if (choice.choiceConsequences != null && choice.choiceConsequences.Length > 0)
                {
                    foreach (var consequence in choice.choiceConsequences)
                    {
                        if (!consequence.IsEffectCorrectlySetUp(out string explanation))
                        {
                            EditorGUILayout.HelpBox(explanation + ". In " + choice.choicePreDescription + " - " + consequence.typeOfConsequence.ToString(), MessageType.Error);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Missing consequences of choice", MessageType.Error);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Missing choices", MessageType.Error);
        }
    }
}
