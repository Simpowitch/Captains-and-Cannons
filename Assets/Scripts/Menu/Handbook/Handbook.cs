using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Handbook : MonoBehaviour
{
    private const float SCROLLMIN = 320f, SCROLLOFFSET = 6.5f;
    private const int MAXSUBJECTSPERPAGE = 12;
    private float subjectButtonHeight = 62;

    [SerializeField] private float scrollSpeed = 0.5f;

    public Text subjectTitle = null;
    public Text subjectDescription = null;
    public Image subjectImage = null;
    public List<HandbookSubject> subjects = new List<HandbookSubject>();
    [SerializeField] HandbookButton subjectButton = null;

    public static Handbook instance;
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
        subjects.Sort((x, y) => string.Compare(x.name, y.name));
        if (transform.childCount == 0)
        {
            for (int i = 0; i < subjects.Count; i++)
            {
                HandbookButton newButton = Instantiate(subjectButton.gameObject, transform).GetComponent<HandbookButton>();
                newButton.SetIndex(i);
            }

        }

    }

    private void UpdateDescription(int subjectIndex)
    {
        subjectTitle.text = instance.subjects[subjectIndex].subjectTitle;
        subjectDescription.text = instance.subjects[subjectIndex].subjectDescription;

        if (instance.subjects[subjectIndex].subjectImage != null)
        {
            subjectImage.enabled = true;
            subjectImage.sprite = instance.subjects[subjectIndex].subjectImage;
        }
        else
        {
            subjectImage.enabled = false;
        }
       
    }

    public void SelectItem(int subjectIndex)
    {
        UpdateDescription(subjectIndex);
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && subjects.Count > MAXSUBJECTSPERPAGE)
        {
            float newYPos = transform.localPosition.y;
            newYPos = transform.localPosition.y + (-Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, Mathf.Clamp(newYPos, SCROLLMIN, subjects.Count * subjectButtonHeight - subjectButtonHeight * SCROLLOFFSET), 0), 5);
        }
    }
}
