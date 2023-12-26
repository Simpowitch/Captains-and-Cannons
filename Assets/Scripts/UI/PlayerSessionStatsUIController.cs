using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSessionStatsUIController : MonoBehaviour
{
    [SerializeField] OverHeadInfo healthBar = null;
    [SerializeField] Text dubloons = null;

    void Start()
    {
        UpdateHealthbar();
        UpdateDubloons();

        Subscribe(true);
    }

    private void UpdateHealthbar()
    {
        healthBar.SetMaxValue(PlayerSession.instance.ShipMaxHP);
        healthBar.SetCurrentValue(PlayerSession.instance.ShipCurrentHP);
    }

    private void UpdateDubloons()
    {
        dubloons.text = PlayerSession.instance.Dubloons.ToString();
    }

    private void Subscribe(bool status)
    {
        if (status)
        {
            PlayerSession.OnSessionStatsChanged += UpdateHealthbar;
            PlayerSession.OnSessionStatsChanged += UpdateDubloons;
        }
        else
        {
            PlayerSession.OnSessionStatsChanged -= UpdateHealthbar;
            PlayerSession.OnSessionStatsChanged -= UpdateDubloons;
        }
    }

    private void OnDestroy()
    {
        Subscribe(false);
    }
}
