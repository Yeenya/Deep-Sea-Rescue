using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorWarning : MonoBehaviour
{
    private AudioSource floorWarningSound;
    private float maxWarningDistance = 5f;

    void Start()
    {
        floorWarningSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Terrain") && hit.distance < maxWarningDistance && Camera.main.GetComponent<CameraController>().GetInsideOrOutside())
            {
                if (!floorWarningSound.isPlaying)
                {
                    floorWarningSound.Play();
                }
                floorWarningSound.pitch = 1 + (1 - hit.distance / maxWarningDistance) * 4;
            }
            else
            {
                floorWarningSound.Stop();
            }
        }
    }
}
