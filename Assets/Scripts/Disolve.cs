using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simon Voss
//Simple disolve script to fade out objects

public class Disolve : MonoBehaviour
{
    public enum ActionAtEnd { GameobjectInactive, RendererInactive, Nothing }
    [SerializeField] ActionAtEnd atEnd = ActionAtEnd.RendererInactive;

    bool isDisolving = false;
    float fade = 1f;

    Material material;

    public void DoEffect()
    {
        material = GetComponent<SpriteRenderer>().material;
        isDisolving = true;
        Debug.Log("Disolving");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDisolving)
        {
            return;
        }

        fade -= Time.deltaTime;

        if (fade <= 0f)
        {
            fade = 0; //Clamp to 0
            isDisolving = false;

            switch (atEnd)
            {
                case ActionAtEnd.GameobjectInactive:
                    gameObject.SetActive(false);
                    break;
                case ActionAtEnd.RendererInactive:
                    GetComponent<SpriteRenderer>().enabled = false;
                    break;
                case ActionAtEnd.Nothing:
                    break;
            }
        }
        //Set the fade value to the material
        material.SetFloat("_Fade", fade);
    }
}
