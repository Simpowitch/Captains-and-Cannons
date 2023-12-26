using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Blueprint responsible for creating a clone of a gunner from scriptable object or getting the stored info directly from the scriptable object

[CreateAssetMenu(fileName = "New Gunner", menuName = "Character/New Gunner")]
public class GunnerBlueprint : CharacterBlueprint
{
    public GunnerData gunnerInformation;

    public override CharacterData GetCharacterData()
    {
        return new GunnerData(gunnerInformation);
    }

    public override CharacterData GetBlueprintData()
    {
        return gunnerInformation;
    }
}
