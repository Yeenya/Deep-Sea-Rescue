using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        player = GameObject.FindGameObjectWithTag("Player");
        cameraOffset = transform.position - player.transform.position;
        insideOrOutside = false;
        fovOutside = GetComponent<Camera>().fieldOfView;
        fovInside = 105f;
        insideRotation = Vector3.zero;
    }
    
    void Update()
    {
        if (!insideOrOutside)
        {
            Vector3 positionDifference = player.transform.TransformPoint(cameraOffset) - transform.position;
            if (positionDifference.sqrMagnitude > 0.3f) transform.position += positionDifference.sqrMagnitude * Time.deltaTime * positionDifference;
            transform.rotation = player.transform.rotation;
            transform.LookAt(player.transform.position + player.transform.forward * 20);
        }
        else
        {
            insideRotation.x -= Input.GetAxis("Mouse Y");
            insideRotation.y += Input.GetAxis("Mouse X");

            if (insideRotation.x > 90) insideRotation.x = 90;
            else if (insideRotation.x < -90) insideRotation.x = -90;

            if (insideRotation.y > 90) insideRotation.y = 90;
            else if (insideRotation.y < -90) insideRotation.y = -90;

            transform.SetPositionAndRotation(insideSpot.position, insideSpot.rotation);
            transform.localRotation *= Quaternion.Euler(insideRotation);
        }
    }

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

    public bool GetInsideOrOutside()
    {
        return insideOrOutside;
    }
}
