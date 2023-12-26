using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boss Intro/New Boss Intro", fileName = "New Boss Intro")]
public class BossIntroBlueprint : ScriptableObject
{

    [TextArea]public string bossName = null;
    public Sprite bossSprite = null;
    public Sprite backgroundSprite = null;
    public AudioClip voiceLine = null;
}
