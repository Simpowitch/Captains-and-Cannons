using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Blueprint responsible for creating a clone of a captain from scriptable object or getting the stored info directly from the scriptable object

[CreateAssetMenu(fileName = "New Captain", menuName = "Character/New Captain")]
public class CaptainBlueprint : CharacterBlueprint
{
    public CaptainData captainInformation;

    public override CharacterData GetCharacterData()
    {
        return new CaptainData(captainInformation);
    }
    public override CharacterData GetBlueprintData()
    {
        return captainInformation;
    }
}
