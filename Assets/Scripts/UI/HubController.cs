using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubController : MonoBehaviour
{


    float angle = 0;

    private void Update()
    {
        RotateToAngle(angle);
    }

    public void NextState()
    {
        angle += 120;
        angle = angle % 360;
        RotateToAngle(angle);
    }
    public void PreviousState()
    {
        angle -= 120;
        angle = angle % 360;
        RotateToAngle(angle);
    }
    public void ToggleMenu()
    {
        if (SettingsMenu.instance.menuIsOpen)
        {
            SettingsMenu.instance.CloseMenu();
        }
        else
        {
            SettingsMenu.instance.OpenMenu();
        }
    }

    void RotateToAngle(float angle)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), 150f * Time.deltaTime);
    }
}
