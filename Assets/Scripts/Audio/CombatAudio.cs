using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAudio : MonoBehaviour
{
    #region Singleton
    public static CombatAudio instance;
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


    public enum ActionAudio { Selection, Move, Slashing, Bludgeoning, Piercing, Gun, HeavyGun, Throwing, HeavyThrow, Heal, DrinkMead, GiveActionPoint, Cannon, ShipImpact}
    public enum TakeDamageAudio { HumanMale, HumanFemale, Ghost, Fishpeople}
    public enum DeathAudio { HumanMale, HumanFemale, Ghost, Fishpeople }


    [SerializeField] AudioSource sfxPlayer = null;
    [SerializeField] AudioSource stingerPlayer = null;
    [SerializeField] AudioSource musicPlayer = null;


    [Header("Action")]
    [SerializeField] AudioClip[] select = null;
    [SerializeField] AudioClip[] move = null;
    [SerializeField] AudioClip slashing = null;
    [SerializeField] AudioClip bludgeoning = null;
    [SerializeField] AudioClip piercing = null;
    [SerializeField] AudioClip[] gun = null;
    [SerializeField] AudioClip heavyGun = null;
    [SerializeField] AudioClip throwing = null;
    [SerializeField] AudioClip heavyThrow = null;
    [SerializeField] AudioClip heal = null;
    [SerializeField] AudioClip drinkMead = null;
    [SerializeField] AudioClip giveActionPoint = null;
    [SerializeField] AudioClip cannon = null;
    [SerializeField] AudioClip[] shipImpact = null;

    [Header("Damage")]
    [SerializeField] AudioClip[] humanMale = null;
    [SerializeField] AudioClip[] humanfemale = null;
    [SerializeField] AudioClip[] ghost = null;
    [SerializeField] AudioClip[] fishpeople = null;

    [Header("Death")]
    [SerializeField] AudioClip[] humanMaleDeath = null;
    [SerializeField] AudioClip[] humanfemaleDeath = null;
    [SerializeField] AudioClip[] ghostDeath = null;
    [SerializeField] AudioClip[] fishpeopleDeath = null;

    [Header("Stingers")]
    [SerializeField] AudioClip[] introStingers = null; //Play if not boss
    [SerializeField] AudioClip[] loseStingers = null;
    [SerializeField] AudioClip[] winStingers = null;

    [Header("Music")]
    [SerializeField] AudioClip[] battleMusics = null;
    [SerializeField] AudioClip finalBossMusic = null;

    private void Start()
    {
        CombatDelegates.instance.OnPlayerWon += PlayWinStinger;
        CombatDelegates.instance.OnPlayerLost += PlayLoseStinger;
    }

    public void PlayAudio(ActionAudio audio)
    {
        AudioClip clip = null;

        switch (audio)
        {
            case ActionAudio.Selection:
                clip = Utility.ReturnRandom(select);
                break;
            case ActionAudio.Move:
                clip = Utility.ReturnRandom(move);
                break;
            case ActionAudio.Slashing:
                clip = slashing;
                break;
            case ActionAudio.Bludgeoning:
                clip = bludgeoning;
                break;
            case ActionAudio.Piercing:
                clip = piercing;
                break;
            case ActionAudio.Gun:
                clip = Utility.ReturnRandom(gun);
                break;
            case ActionAudio.HeavyGun:
                clip = heavyGun;
                break;
            case ActionAudio.Throwing:
                clip = throwing;
                break;
            case ActionAudio.HeavyThrow:
                clip = heavyThrow;
                break;
            case ActionAudio.Heal:
                clip = heal;
                break;
            case ActionAudio.DrinkMead:
                clip = drinkMead;
                break;
            case ActionAudio.GiveActionPoint:
                clip = giveActionPoint;
                break;
            case ActionAudio.Cannon:
                clip = cannon;
                break;
            case ActionAudio.ShipImpact:
                clip = Utility.ReturnRandom(shipImpact);
                break;
        }
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlayAudio(TakeDamageAudio audio)
    {
        AudioClip clip = null;

        switch (audio)
        {
            case TakeDamageAudio.HumanMale:
                clip = Utility.ReturnRandom(humanMale);
                break;
            case TakeDamageAudio.HumanFemale:
                clip = Utility.ReturnRandom(humanfemale);
                break;
            case TakeDamageAudio.Ghost:
                clip = Utility.ReturnRandom(ghost);
                break;
            case TakeDamageAudio.Fishpeople:
                clip = Utility.ReturnRandom(fishpeople);
                break;
        }
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlayAudio(DeathAudio audio)
    {
        AudioClip clip = null;

        switch (audio)
        {
            case DeathAudio.HumanMale:
                clip = Utility.ReturnRandom(humanMaleDeath);
                break;
            case DeathAudio.HumanFemale:
                clip = Utility.ReturnRandom(humanfemaleDeath);
                break;
            case DeathAudio.Ghost:
                clip = Utility.ReturnRandom(ghostDeath);
                break;
            case DeathAudio.Fishpeople:
                clip = Utility.ReturnRandom(fishpeopleDeath);
                break;
        }
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlayAudio(AudioClip sfxAudioClip)
    {
        sfxPlayer.PlayOneShot(sfxAudioClip);
    }

    public void PlayIntroStinger()
    {
        AudioClip stinger = Utility.ReturnRandom(introStingers);
        stingerPlayer.PlayOneShot(stinger);
    }

    private void PlayWinStinger()
    {
        AudioClip stinger = Utility.ReturnRandom(winStingers);
        stingerPlayer.PlayOneShot(stinger);
    }

    private void PlayLoseStinger()
    {
        AudioClip stinger = Utility.ReturnRandom(loseStingers);
        stingerPlayer.PlayOneShot(stinger);
    }

    public void PlayMusic(bool isFinalBattle)
    {
        AudioClip music = isFinalBattle ? finalBossMusic : Utility.ReturnRandom(battleMusics);
        musicPlayer.clip = music;
        musicPlayer.loop = true;
        musicPlayer.Play();
    }
}
