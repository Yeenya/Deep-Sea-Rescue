using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * A diver-like behaving sphere that is used at the beginning of the tutorial.
 */
public class TutorialSphere : MonoBehaviour
{
    private GameObject player;
    private AudioSource sonarSound;
    private CameraController cameraController;

    void Start()
    {
        // Get necessary references
        player = GameObject.FindGameObjectWithTag("Player");
        sonarSound = GetComponent<AudioSource>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        // Modify the volume and pitch just like Diver does. See the diver script for further elaboration on the modifications.
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= sonarSound.maxDistance && cameraController.GetInsideOrOutside()) sonarSound.volume = 1;
        else sonarSound.volume = 0;
        float nonModifiedPitch = (sonarSound.maxDistance - distance) / sonarSound.maxDistance;
        sonarSound.pitch = (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 2 + 1; //easeInSine from https://easings.net/#easeInSine
    }
}
