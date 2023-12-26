using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHotkeys : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && CombatManager.instance.GameState == CombatManager.State.PlayerTurn)
        {
            if (CombatManager.instance.SelectedCharacter != null)
            {
                CombatDelegates.instance.OnCombatHotkeyPress?.Invoke();
            }
        }
    }
}
