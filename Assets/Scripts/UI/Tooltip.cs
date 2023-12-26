using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField] Text textField = null;
    [SerializeField] RectTransform backgroundRect = null;
    [SerializeField] GameObject mainObject = null;

    [SerializeField] float textPadding = 10;

    public void SetUp(string message)
    {
        mainObject.SetActive(true);

        textField.text = message;

        Vector2 backgroundSize = new Vector2(textField.preferredWidth + textPadding * 2, textField.preferredHeight + textPadding * 2);
        backgroundRect.sizeDelta = backgroundSize;
    }

    public void HideTooltip()
    {
        mainObject.SetActive(false);
    }
}
