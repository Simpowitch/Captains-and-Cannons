using UnityEngine;

//Simon Voss
//A sender that sends information about which audio clip to be played

public class UISFXSender : MonoBehaviour
{
    [SerializeField] AudioClip customClip = null;

    public void PlayCustom()
    {
        if (UISFXManager.instance != null)
        {
            UISFXManager.PlayUISFX(customClip);
        }
    }

    public void PlaySelect()
    {
        if (UISFXManager.instance != null)
        {
            UISFXManager.PlayUISFX(UISFXManager.SFX.Select);
        }
    }

    public void PlayBack()
    {
        if (UISFXManager.instance != null)
        {
            UISFXManager.PlayUISFX(UISFXManager.SFX.Back);
        }
    }

    public void PlayHoover()
    {
        if (UISFXManager.instance != null)
        {
            UISFXManager.PlayUISFX(UISFXManager.SFX.Hoover);
        }
    }
}
