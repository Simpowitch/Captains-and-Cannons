using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Holder of information of which to display to a tooltip

public class TooltipInformation : MonoBehaviour
{
    [SerializeField] bool displayToCustomTooltip = false;

    [SerializeField] Tooltip customTooltip = null;

    [SerializeField] [TextArea(1, 3)] string information = "";

    public void SendToTooltip()
    {
        if (displayToCustomTooltip) 
        {
            customTooltip.SetUp(information);
        }
        else
        {
            MouseTooltip.SetUpToolTip(MouseTooltip.ColorText.Default, information);
        }
    }

    public void DisableTooltip()
    {
        if (displayToCustomTooltip)
        {
            customTooltip.HideTooltip();
        }
        else
        {
            MouseTooltip.HideTooltip();
        }
    }
}
