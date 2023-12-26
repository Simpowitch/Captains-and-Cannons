using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Blueprint responsible for creating a clone of a chef from scriptable object or getting the stored info directly from the scriptable object

[CreateAssetMenu(fileName = "New Chef", menuName = "Character/New Chef")]
public class ChefBlueprint : CharacterBlueprint
{
    public ChefData chefInformation;

    public override CharacterData GetCharacterData()
    {
        return new ChefData(chefInformation);
    }

    public override CharacterData GetBlueprintData()
    {
        return chefInformation;
    }
}
