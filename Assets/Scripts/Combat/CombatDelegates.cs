using System;
using UnityEngine;

//Simon Voss
//Custom events and delegates

public class CombatDelegates : MonoBehaviour
{
    #region Singleton
    public static CombatDelegates instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Another instance of : " + instance.ToString() + " was tried to be instanced, but was destroyed from gameobject: " + this.transform.name);
            GameObject.Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion


    public delegate void TurnHandler(CombatManager.State state);
    public TurnHandler OnTurnStatusChanged;

    public event Action OnPlayerLost;
    public void PlayerLost()
    {
        OnPlayerLost?.Invoke();
    }
    public event Action OnPlayerWon;
    public void PlayerWon()
    {
        OnPlayerWon?.Invoke();
    }

    public delegate void CharacterHandler(Character character);
    public CharacterHandler OnSelectedCharacter;
    public CharacterHandler OnCharacterMoved;
    public CharacterHandler OnPreparedActionChanged;
    public CharacterHandler OnActionPerformed;
    public CharacterHandler OnCharacterDied;

    public delegate void QueuedEvent();
    public QueuedEvent OnAnimationFinished;
    public QueuedEvent OnAnimationBegin;

    public delegate void HotkeyPress();
    public HotkeyPress OnCombatHotkeyPress;
}
