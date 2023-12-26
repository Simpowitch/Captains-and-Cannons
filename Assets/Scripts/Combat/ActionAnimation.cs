using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss & Samuel

public class ActionAnimation : MonoBehaviour
{
    public enum AnimationState { Melee, Ranged, SpecialEnemy, SpecialFriendly, SpecialSelf }

    #region Singleton
    public static ActionAnimation instance;
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

    CombatAudio.ActionAudio clipToBePlayed;

    [SerializeField] List<Image> originLayers = null;
    [SerializeField] Image originStanding = null;
    [SerializeField] Image abilityIcon = null;

    [SerializeField] Image targetStanding = null;

    [SerializeField] Image projectile = null;

    [SerializeField] List<Image> bloodSplatters = new List<Image>();

    [SerializeField] Sprite[] bloodSplatterSprites = null;

    [SerializeField] Animator sceneAnimator = null;
    [SerializeField] Text numberText = null;
    [SerializeField] GameObject critText = null;

    [SerializeField] Image effectImage = null;
    [SerializeField] Text[] plusSigns = null;

    [SerializeField] Color healColor = Color.green;
    [SerializeField] Color healColorPlusSigns = Color.green;

    [SerializeField] Color captainActionPointColor = Color.blue;
    [SerializeField] Color captainActionPointColorPlusSigns = Color.blue;

    [SerializeField] Color drinkRumColor = Color.yellow;

    [SerializeField] Color damageColor = Color.blue;




    public void PlayAnimation(Character origin, Character target, int number, AnimationState typeOfAnimation, CombatAudio.ActionAudio audio, bool isCrit)
    {
        clipToBePlayed = audio;
        PlayCombatScene(origin, target, number, typeOfAnimation, isCrit);
    }

    public void PlayCombatScene(Character origin, Character target, int number, AnimationState typeOfAnimation, bool isCrit)
    {
        CombatDelegates.instance.OnAnimationBegin?.Invoke();
        sceneAnimator.SetTrigger("StartScene");
        CallAnimationState(origin, target, number, typeOfAnimation, isCrit);
        sceneAnimator.SetTrigger("EndScene");
    }

    public void PlayQueuedAudio()
    {
        CombatAudio.instance.PlayAudio(clipToBePlayed);
    }

    public void AnimationFinished()
    {
        CombatDelegates.instance.OnAnimationFinished?.Invoke();
    }

    private void CallAnimationState(Character origin, Character target, int number, AnimationState typeOfAnimation, bool isCrit)
    {
        targetStanding.sprite = target.SpriteHolder.standing;
        numberText.text = number.ToString();
        numberText.color = damageColor; //Default
        critText.SetActive(isCrit);

        //Target
        int flipX = target.MyTeam == Team.AI ? 1 : -1;
        targetStanding.transform.localScale = new Vector3(flipX, 1, 1);

        //Origin
        flipX = origin.MyTeam == Team.AI ? -1 : 1;
        originStanding.transform.localScale = new Vector3(flipX, 1, 1);
        foreach (var image in originLayers)
        {
            image.transform.localScale = new Vector3(flipX, 1, 1);
        }

        //Bloodsplatters
        foreach (var image in bloodSplatters)
        {
            image.sprite = bloodSplatterSprites[Random.Range(0, bloodSplatterSprites.Length)];
            image.enabled = false;
        }
        int imagesToShow = Mathf.CeilToInt(number / 10);
        for (int i = 0; i < imagesToShow; i++)
        {
            if (i < bloodSplatters.Count)
            {
                bloodSplatters[i].enabled = true;
            }
        }

        //Set images
        originStanding.sprite = origin.SpriteHolder.standing;
        targetStanding.sprite = target.SpriteHolder.standing;

        switch (typeOfAnimation)
        {
            case AnimationState.Melee:
                foreach (var image in originLayers)
                {
                    image.sprite = origin.SpriteHolder.melee;
                }
                sceneAnimator.SetTrigger("Melee");
                break;
            case AnimationState.Ranged:
                foreach (var image in originLayers)
                {
                    image.sprite = origin.SpriteHolder.ranged;
                }
                projectile.sprite = origin.SpriteHolder.rangedEffect;
                sceneAnimator.SetTrigger("Ranged");
                break;
            case AnimationState.SpecialEnemy:
                foreach (var image in originLayers)
                {
                    image.sprite = origin.SpriteHolder.ranged;
                }
                projectile.sprite = origin.SpriteHolder.specialAbilityGunnerAnimationEffect;
                sceneAnimator.SetTrigger("SpecialEnemy");
                break;
            case AnimationState.SpecialFriendly:
                abilityIcon.sprite = origin.SpriteHolder.specialAbilityCaptainFighterChefParticleEffect;

                flipX = target.MyTeam == Team.AI ? -1 : 1;
                targetStanding.transform.localScale = new Vector3(flipX, 1, 1);

                if (origin is Chef)
                {
                    effectImage.color = healColor;
                    foreach (var item in plusSigns)
                    {
                        item.color = healColorPlusSigns;
                    }
                    numberText.color = healColor;
                }
                else if (origin is Captain)
                {
                    effectImage.color = captainActionPointColor;
                    foreach (var item in plusSigns)
                    {
                        item.color = captainActionPointColorPlusSigns;
                    }
                    numberText.color = captainActionPointColor;
                }

                sceneAnimator.SetTrigger("SpecialFriendly");
                break;
            case AnimationState.SpecialSelf:
                abilityIcon.sprite = origin.SpriteHolder.specialAbilityCaptainFighterChefParticleEffect;

                if (origin is Fighter)
                {
                    effectImage.color = drinkRumColor;
                }

                sceneAnimator.SetTrigger("SpecialSelf");
                break;
            default:
                break;
        }
    }
}
