using System.Collections;
using UnityEngine;

//Simon Voss & Samuel
//Moves the camera on the x axis in the map view by placing cursor on the screen left side or right side. Or by using a or left-arrow, d or right-arrow

public class MapCamera : MonoBehaviour
{
    const float CAMERAFALLOFSPEED = 0.99f;

    [SerializeField] float zPosition = -5f;

    [SerializeField] float panSpeed = 10f;
    [SerializeField] float panBorderThickness = 0.1f;
    [SerializeField] float panLimit = 70;
    float mouseDownXpos = 0;
    float mouseUpXpos = 0;


    private void Update()
    {
        if (autoPanning)
        {
            return;
        }

        //Setup
        Vector3 pos = transform.position;
        
        //Click and drag
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

        pos.x = Mathf.Clamp(pos.x, -panLimit, panLimit);
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * panSpeed);
    }

    public void SetCameraPosition(Vector2 newPos)
    {
        StartCoroutine(SetCameraPosOverTime(newPos));
    }

    bool autoPanning = false;
    float stopDistanceAutoPan = 30;
    public IEnumerator SetCameraPosOverTime(Vector2 newPos)
    {
        autoPanning = true;
        while (Vector2.Distance(newPos, transform.position) > stopDistanceAutoPan)
        {
            float distanceToTarget = Vector2.Distance(newPos, transform.position);
            transform.position = Vector3.Lerp(transform.position, new Vector3(newPos.x, transform.position.y, zPosition), Time.deltaTime * distanceToTarget * 0.01f);
            yield return null;
        }
        autoPanning = false;
    }
}
