using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the "front sensor" of the submarine.
 */
public class FloorWarning : MonoBehaviour
{
    [SerializeField]
    private float maxWarningDistance = 5f;

    private AudioSource floorWarningSound;
    private float timeFromStart = 0f;

    void Start()
    {
        floorWarningSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check if something is in front of the submarine
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxWarningDistance))
        {
            // Check if the object is a terrain
            if (hit.collider.CompareTag("Terrain") && Camera.main.GetComponent<CameraController>().GetInsideOrOutside())
            {
                // Ensure the sound is playing
                if (!floorWarningSound.isPlaying)
                {
                    floorWarningSound.Play();
                }
                timeFromStart += Time.deltaTime;

                // If the distance is less than 1 metre, play the sound constantly (again same as in cars)
                if (hit.distance <= 1f)
                {
                    floorWarningSound.volume = 0.05f;
                    return;
                }

                // Simulate beeping as in cars with sensors
                float interval = hit.distance / (maxWarningDistance * 4);
                if (timeFromStart >= interval)
                {
                    timeFromStart = 0f;
                    floorWarningSound.volume = floorWarningSound.volume == 0f ? 0.05f : 0f;
                }
            }
        }
        else
        {
            floorWarningSound.Stop(); // Stop playing when the colliding object is farther than the max distance
        }
    }
}
