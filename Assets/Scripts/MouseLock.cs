using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Confines mouse to screen

public class MouseLock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void LockToScreen(bool confinedToScreen)
    {
        if (confinedToScreen)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
