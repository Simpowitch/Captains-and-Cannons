using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossIntro : MonoBehaviour
{
    [SerializeField] Text bossName = null;
    [SerializeField] Image background = null;
    [SerializeField] Image bossSprite = null;
    [SerializeField] Animator myAnimator = null;
    [SerializeField] AudioSource myAudioSource = null;
    public void RunIntro()
    {
        if (PlayerSession.instance.nextEncounter.bossIntro != null)
        {
            BossIntroBlueprint blueprint;
            blueprint = PlayerSession.instance.nextEncounter.bossIntro;
            bossName.text = blueprint.bossName;
            background.sprite = blueprint.backgroundSprite;
            bossSprite.sprite = blueprint.bossSprite;
            myAudioSource.clip = blueprint.voiceLine;
            myAudioSource.PlayDelayed(2);
            myAnimator.SetTrigger("RunIntro");
        }
    }
}
