using UnityEngine;

[CreateAssetMenu(menuName = "Handbook/New Handbook Subject", fileName = "New Subject")]
public class HandbookSubject : ScriptableObject
{
    public string subjectTitle;
    [TextArea(3,25)] public string subjectDescription;
    public Sprite subjectImage;
}
