using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the camera movement (both 1st and 3rd person).
 */
public class CameraController : MonoBehaviour
{
    private GameObject player;
    private Vector3 cameraOffset;
    public Transform insideSpot;
    private bool insideOrOutside;
    private float fovOutside;
    private float fovInside;
    private Vector3 insideRotation;

    void Start()
    {
        // Get necessary references and set the camera to first person view with correct fov, position, cursor state etc.
        player = GameObject.FindGameObjectWithTag("Player");
        cameraOffset = transform.position - player.transform.position;
        insideOrOutside = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        fovOutside = GetComponent<Camera>().fieldOfView;
        fovInside = 105f;
        Camera.main.fieldOfView = fovInside;
        insideRotation = Vector3.zero;
    }
    
    void Update()
    {
        if (!insideOrOutside) // Outside - Just follow the submarine and look in its facing direction.
        {
            Vector3 positionDifference = player.transform.TransformPoint(cameraOffset) - transform.position;
            if (positionDifference.sqrMagnitude > 0.3f) transform.position += positionDifference.sqrMagnitude * Time.deltaTime * positionDifference;
            transform.rotation = player.transform.rotation;
            transform.LookAt(player.transform.position + player.transform.forward * 20);
        }
        else // Inside - Move with the submarine but rotate according to mouse movement.
        {
            if (player.GetComponent<Player>().state == Player.State.REPLAY) return; // Disable if the player is in replay mode.

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                insideRotation.x -= Input.GetAxis("Mouse Y");
                insideRotation.y += Input.GetAxis("Mouse X");
            }

            // Limit camera rotation
            if (insideRotation.x > 90) insideRotation.x = 90;
            else if (insideRotation.x < -90) insideRotation.x = -90;

            if (insideRotation.y > 90) insideRotation.y = 90;
            else if (insideRotation.y < -90) insideRotation.y = -90;

            // Move the camera with the submarine + rotate accordingly (combine submarine rotation and mouse input).
            transform.SetPositionAndRotation(insideSpot.position, insideSpot.rotation);
            transform.localRotation *= Quaternion.Euler(insideRotation);
        }
    }

    /*
     * Changes the camera from first person view to third person view and vice versa.
     */
    public void ChangeCameraPosition()
    {
        insideOrOutside = !insideOrOutside;
        if (!insideOrOutside)
        {
            Camera.main.fieldOfView = fovOutside;
            transform.SetPositionAndRotation(player.transform.position + cameraOffset, player.transform.rotation);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Camera.main.fieldOfView = fovInside;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /*
     * Returns the current camera position (inside or outside).
     */
    public bool GetInsideOrOutside()
    {
        return insideOrOutside;
    }
}
