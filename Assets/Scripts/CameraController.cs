using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    protected Camera targetCamera;
    public Camera TargetCamera => targetCamera;
    [SerializeField]
    protected float baseFov = 60;
    [SerializeField]
    protected float baseSize = 8;

    protected void Update()
    {
        PanCheck();
    }

    protected bool isPinching;
    protected Vector3 dragStartPosition;
    protected Vector3 dragCurrentPosition;
    protected Plane dragCheckPlane;
    protected void PanCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPinching = true;

            dragCheckPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (dragCheckPlane.Raycast(ray, out float entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (isActive && Input.GetMouseButton(0))
        {
            if (isPinching)
            {
                Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
                if (dragCheckPlane.Raycast(ray, out float entry))
                {
                    dragCurrentPosition = ray.GetPoint(entry);
                    var newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                    transform.position = newPosition;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPinching = false;
        }
    }

    protected bool isActive = true;
    public void ToggleCameraMovement(bool isActive)
    {
        this.isActive = isActive;
    }

    public void Zoom(float stepValue)
    {
        if (targetCamera.orthographic)
        {

        }
        else
        {

        }
    }
}
