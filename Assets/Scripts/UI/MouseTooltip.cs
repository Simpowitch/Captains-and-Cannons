using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Pop-up tooltip that shows information upon request from UI elements etc.

public class MouseTooltip : MonoBehaviour
{
    #region Singleton
    public static MouseTooltip instance;
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
            if (usedInCombatScene)
            {
                CombatDelegates.instance.OnPreparedActionChanged += HideTooltip;
            }
        }
    }
    #endregion

    [SerializeField] bool usedInCombatScene = true;

    [SerializeField] Text textField = null;
    [SerializeField] RectTransform backgroundRect = null;

    [SerializeField] float textPadding = 10;


    [SerializeField] Animator animator = null;

    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] Color friendlyTarget = Color.green;
    [SerializeField] Color enemyTarget = Color.red;
    [SerializeField] Color anyTarget = Color.yellow;
    [SerializeField] Color invalidTarget = Color.blue;

    public enum ColorText { Default, FriendlyTarget, EnemyTarget, AnyTarget, InvalidTarget }

    float mouseOffset = 25;

    private void Update()
    {
        Vector2 pos = Input.mousePosition;
        if (Input.mousePosition.x > Screen.width / 2)
        {
            pos += new Vector2(-mouseOffset - backgroundRect.sizeDelta.x, mouseOffset);
        }
        else
        {
            pos += new Vector2(mouseOffset, mouseOffset);
        }
        this.transform.position = pos;
    }

    private void Start()
    {
        HideTooltip(null);
    }


    private void SetUp(ColorText textColor, string message)
    {
        if (message == "")
        {
            HideTooltip();
            return;
        }


        gameObject.SetActive(true);
        animator.SetTrigger("FadeIn");

        Color color = defaultColor;
        switch (textColor)
        {
            case ColorText.Default:
                color = defaultColor;
                break;
            case ColorText.FriendlyTarget:
                color = friendlyTarget;
                break;
            case ColorText.EnemyTarget:
                color = enemyTarget;
                break;
            case ColorText.AnyTarget:
                color = anyTarget;
                break;
            case ColorText.InvalidTarget:
                color = invalidTarget;
                break;
        }

        textField.color = color;
        textField.text = message;

        Vector2 backgroundSize = new Vector2(textField.preferredWidth + textPadding * 2, textField.preferredHeight + textPadding * 2);
        backgroundRect.sizeDelta = backgroundSize;

        Vector2 pos = Input.mousePosition;
        if (Input.mousePosition.x > Screen.width / 2)
        {
            pos += new Vector2(-mouseOffset - backgroundSize.x, mouseOffset);
        }
        else
        {
            pos += new Vector2(mouseOffset, mouseOffset);
        }
        this.transform.position = pos;
    }

    private void HideTooltip(Character character)
    {
        animator.SetTrigger("FadeOut");
        gameObject.SetActive(false);
    }

    public static void SetUpToolTip(ColorText textColor, string message)
    {
        instance.SetUp(textColor, message);
    }

    public static void HideTooltip()
    {
        instance.HideTooltip(null);
    }
}
