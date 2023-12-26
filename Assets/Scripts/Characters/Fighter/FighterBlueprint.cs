using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Blueprint responsible for creating a clone of a fighter from scriptable object or getting the stored info directly from the scriptable object


[CreateAssetMenu(fileName = "New Fighter", menuName = "Character/New Fighter")]
public class FighterBlueprint : CharacterBlueprint
{
    public FighterData fighterInformation;

    public override CharacterData GetCharacterData()
    {
        return new FighterData(fighterInformation);
    }

    public override CharacterData GetBlueprintData()
    {
        return fighterInformation;
    }
}
