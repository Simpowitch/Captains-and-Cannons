using UnityEngine;

//Simon Voss
//Used to show a pop up with information. With additional options to only show once by using playerprefs

public class TooltipPopup : MonoBehaviour
{
    [SerializeField] string uniqueKey = "key";
    [SerializeField] bool showOnlyOnce = false;

    private void Awake()
    {
        if (showOnlyOnce)
        {
            if (PlayerPrefs.HasKey(uniqueKey))
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                PlayerPrefs.SetInt(uniqueKey, 0);
            }
        }
    }
}
