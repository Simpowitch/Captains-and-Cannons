using UnityEngine;
using UnityEngine.UI;

public class VersionNameUpdater : MonoBehaviour
{
    [SerializeField] Text text = null;

    private void Start()
    {
        text.text = "Version: " + Application.version;
    }
}
