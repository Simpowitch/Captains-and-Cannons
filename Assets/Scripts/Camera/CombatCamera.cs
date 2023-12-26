using UnityEngine;

//Simon Voss & Samuel
//Camera movement with mouse or arrow (or a/d) within combat

public class CombatCamera : MonoBehaviour
{
    [SerializeField] float xSceneWidth = 70f;

    [SerializeField] float panSpeed = 10f;
    [SerializeField] float panBorderThickness = 0.1f;

    float mouseDownXpos = 0;
    float mouseUpXpos = 0;
    const float CAMERAFALLOFSPEED = 0.99f;

    private void Update()
    {
        //Setup
        Vector3 pos = transform.position;
        float cameraXWorldWidth = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, 0)).x - Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x;

        //Click and drag with mouse button
        if (Input.GetMouseButtonDown(2))
        {
            mouseDownXpos = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        }
        if (Input.GetMouseButton(2))
        {
            mouseUpXpos = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            pos.x += mouseDownXpos - mouseUpXpos;
        }
        else
        {
            mouseUpXpos *= CAMERAFALLOFSPEED;
            mouseDownXpos *= CAMERAFALLOFSPEED;
            pos.x += (mouseDownXpos - mouseUpXpos);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= Screen.width * panBorderThickness)
        {
            pos.x -= panSpeed;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width * (1 - panBorderThickness))
        {
            pos.x += panSpeed;
        }

        pos.x = Mathf.Clamp(pos.x, (-xSceneWidth+cameraXWorldWidth)/2, (xSceneWidth-cameraXWorldWidth)/2);
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * panSpeed);
    }
}
