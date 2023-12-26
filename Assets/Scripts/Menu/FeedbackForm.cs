using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackForm : MonoBehaviour
{
    [SerializeField] Text issue = null;
    [SerializeField] Text content = null;
    public void SendFeedback()
    {
        if (content.text != "")
        {
            StatisticsToForm.SendFeedback(issue.text, content.text);
            issue.text = "";
            content.text = "";
        }

    }
}
