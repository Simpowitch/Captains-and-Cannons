using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Abstract class responsible for creating a clone of a character from scriptable object

public abstract class CharacterBlueprint : ScriptableObject
{
    public abstract CharacterData GetCharacterData();
    public abstract CharacterData GetBlueprintData();
}
