using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorWarning : MonoBehaviour
{
    private AudioSource floorWarningSound;
    private SphereCollider sphereCollider;
    private float maxWarningDistance = 5f;
    private bool frontOrBelow;

    void Start()
    {
        floorWarningSound = GetComponent<AudioSource>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        float closestDistance = float.MaxValue;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Terrain") && hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                frontOrBelow = true;
            }
        }
        /*
        if (Physics.Raycast(transform.parent.position, -transform.parent.up, out hit))
        {
            if (hit.collider.CompareTag("Terrain") && hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                frontOrBelow = false;
            }
        }
        */

        if (closestDistance <= maxWarningDistance)
        {
            if (!floorWarningSound.isPlaying)
            {
                floorWarningSound.Play();
                //transform.localPosition = frontOrBelow ? Vector3.forward * 5 : -Vector3.up * 5;
            }
            floorWarningSound.pitch = 1 + (1 - closestDistance / maxWarningDistance) * 4;
        }
        else
        {
            floorWarningSound.Stop();
        }
    }
}
