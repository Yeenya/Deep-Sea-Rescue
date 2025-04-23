using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the compass in the cockpit of the submarine.
 */
public class Compass : MonoBehaviour
{
    private Transform needlePoint;
    private GameObject player;

    void Start()
    {
        needlePoint = transform.GetChild(0);
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Keep the needle aiming to the north
        needlePoint.transform.localRotation = Quaternion.Euler(0, -player.transform.rotation.eulerAngles.y, 0);
    }
}
