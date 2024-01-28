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

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cameraOffset = transform.position - player.transform.position;
        insideOrOutside = false;
        fovOutside = GetComponent<Camera>().fieldOfView;
        fovInside = 105f;
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
            transform.SetPositionAndRotation(insideSpot.position, insideSpot.rotation);
        }
    }

    public void ChangeCameraPosition()
    {
        insideOrOutside = !insideOrOutside;
        if (!insideOrOutside)
        {
            Camera.main.fieldOfView = fovOutside;
        }
        else
        {
            Camera.main.fieldOfView = fovInside;
        }
    }
}
