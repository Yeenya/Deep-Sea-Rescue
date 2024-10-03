using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloorWarning : MonoBehaviour
{
    private AudioSource floorWarningSound;
    private float maxWarningDistance = 5f;
    private float timeFromStart = 0f;

    void Start()
    {
        floorWarningSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxWarningDistance))
        {
            if (hit.collider.CompareTag("Terrain") && Camera.main.GetComponent<CameraController>().GetInsideOrOutside())
            {
                if (!floorWarningSound.isPlaying)
                {
                    floorWarningSound.Play();
                }
                timeFromStart += Time.deltaTime;

                if (hit.distance <= 1f)
                {
                    floorWarningSound.volume = 0.05f;
                    return;
                }

                float interval = hit.distance / (maxWarningDistance * 4);
                if (timeFromStart >= interval)
                {
                    timeFromStart = 0f;
                    floorWarningSound.volume = floorWarningSound.volume == 0f ? 0.05f : 0f;
                }

                //floorWarningSound.pitch = 1 + (1 - hit.distance / maxWarningDistance) * 4;

            }
        }
        else
        {
            floorWarningSound.Stop();
        }
    }
}
