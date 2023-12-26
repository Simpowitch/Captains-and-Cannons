using UnityEditor;
using UnityEngine.EventSystems;

//Simon Voss
//Enables mutiple objects editing of unity-component event-triggers

[CanEditMultipleObjects]
[CustomEditor(typeof(EventTrigger))]
public class EventTriggerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
    }
}
