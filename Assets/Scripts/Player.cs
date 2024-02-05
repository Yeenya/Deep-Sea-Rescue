using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float velocity;
    [Range(3, 15)]
    [SerializeField]
    private float maxVelocity;

    private float horizontalRotationSpeed;
    private float verticalRotationSpeed;
    private float maxRotationSpeed;

    private GameObject rotor;
    private GameObject periscope;
    private Light leftLight;
    private Light rightLight;
    private readonly float periscopeLightMaxIntensity = 7f;
    private readonly float additionalLightsMaxIntensity = 4f;

    void Start()
    {
        velocity = 0f;
        maxVelocity = 4f;

        horizontalRotationSpeed = 0f;
        verticalRotationSpeed = 0f;
        maxRotationSpeed = 50f;

        rotor = GameObject.FindGameObjectWithTag("Rotor");
        periscope = GameObject.FindGameObjectWithTag("Periscope");
        leftLight = GameObject.FindGameObjectWithTag("Left Light").GetComponent<Light>();
        rightLight = GameObject.FindGameObjectWithTag("Right Light").GetComponent<Light>();
    }

    void Update()
    {
        MoveSubmarine();
        CheckInput();
    }

    private void MoveSubmarine()
    {
        // Forward/backward
        if (Input.GetKey(KeyCode.W)) velocity += 0.1f;
        else if (Input.GetKey(KeyCode.S)) velocity -= 0.1f;
        else if (velocity != 0) velocity -= Mathf.Sign(velocity) * 0.1f;

        if (velocity > maxVelocity) velocity = maxVelocity;
        else if (velocity < -maxVelocity) velocity = -maxVelocity;
        else if (velocity < 0.1 && velocity > -0.1) velocity = 0;

        float velocitySign = velocity != 0 ? Mathf.Sign(velocity) : 1;

        // Left/right
        if (Input.GetKey(KeyCode.A)) horizontalRotationSpeed -= 1f * velocitySign;
        else if (Input.GetKey(KeyCode.D)) horizontalRotationSpeed += 1f * velocitySign;
        else if (horizontalRotationSpeed != 0) horizontalRotationSpeed -= Mathf.Sign(horizontalRotationSpeed);

        if (horizontalRotationSpeed > maxRotationSpeed) horizontalRotationSpeed = maxRotationSpeed;
        else if (horizontalRotationSpeed < -maxRotationSpeed) horizontalRotationSpeed = -maxRotationSpeed;
        else if (horizontalRotationSpeed < 1 && horizontalRotationSpeed > -1) horizontalRotationSpeed = 0;

        // Up/down
        if (Input.GetKey(KeyCode.LeftShift)) verticalRotationSpeed += 1f;
        else if (Input.GetKey(KeyCode.Space)) verticalRotationSpeed -= 1f;
        else if (verticalRotationSpeed != 0) verticalRotationSpeed -= Mathf.Sign(verticalRotationSpeed);

        if (verticalRotationSpeed > maxRotationSpeed) verticalRotationSpeed = maxRotationSpeed;
        else if (verticalRotationSpeed < -maxRotationSpeed) verticalRotationSpeed = -maxRotationSpeed;
        else if (verticalRotationSpeed < 1 && verticalRotationSpeed > -1) verticalRotationSpeed = 0;
        
        // Apply movement to position and rotation
        if (velocity != 0) transform.position += Time.deltaTime * velocity * transform.forward;
        transform.Rotate(new Vector3(verticalRotationSpeed, horizontalRotationSpeed, 0) * Time.deltaTime);
        transform.Rotate(0, 0, -transform.rotation.eulerAngles.z);

        // Limit x rotation so that player doesn't spin to infinity
        Vector3 correctRotation = transform.rotation.eulerAngles;
        if (correctRotation.x > 80 && correctRotation.x < 180) correctRotation.x = 80;
        if (correctRotation.x < 280 && correctRotation.x > 180) correctRotation.x = 280;
        transform.rotation = Quaternion.Euler(correctRotation);

        rotor.transform.Rotate(0, 0.5f + velocity * 0.5f, 0);

        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();

        if (Input.GetKeyDown(KeyCode.F)) ChangeMainLight();
        if (Input.GetKeyDown(KeyCode.C)) ChangeLeftLight();
        if (Input.GetKeyDown(KeyCode.V)) ChangeRightLight();
    }

    private void ChangeMainLight()
    {
        Light periscopeLight = periscope.GetComponentInChildren<Light>();
        if (periscopeLight.intensity == 0)
        {
            periscopeLight.DOIntensity(periscopeLightMaxIntensity, 0.5f);
            periscope.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }
        else
        {
            periscopeLight.DOIntensity(0, 0.5f);
            periscope.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        }
    }

    private void ChangeLeftLight()
    {
        if (leftLight.intensity == 0) leftLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else leftLight.DOIntensity(0, 0.5f);
    }

    private void ChangeRightLight()
    {
        if (rightLight.intensity == 0) rightLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else rightLight.DOIntensity(0, 0.5f);
    }
}
