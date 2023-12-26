using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandbookButton : MonoBehaviour
{
    int myIndex;
    [SerializeField] Text buttonText = null;
    public int GetIndex()
    {
        return myIndex;
    }
    public void SetIndex(int value)
    {
        myIndex = value;
        buttonText.text = Handbook.instance.subjects[value].subjectTitle;
    }
    public void Click()
    {
        Handbook.instance.SelectItem(myIndex);
    }
}
