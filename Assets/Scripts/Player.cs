using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;

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
    private Light periscopeLight;
    private Light leftLight;
    private Light rightLight;
    private readonly float periscopeLightMaxIntensity = 4f;
    private readonly float additionalLightsMaxIntensity = 2f;

    public readonly float maxElectricity = 600f;
    private float electricity;
    private bool mainLightOn = false;
    private bool leftLightOn = false;
    private bool rightLightOn = false;

    private int savedDivers = 0;

    [SerializeField]
    private AudioSource rotorSound;

    [SerializeField]
    private ParticleSystem terrainParticles;

    [SerializeField]
    private AudioSource terrainDragSound;

    [SerializeField]
    private GameObject tiltModel;

    [SerializeField]
    private GameObject gameOverMenu;

    [SerializeField]
    private GameObject gameWonMenu;

    public bool gameOver = false;

    string logFilePath;
    StreamWriter writer;

    void Start()
    {
        velocity = 0f;
        maxVelocity = 4f;

        horizontalRotationSpeed = 0f;
        verticalRotationSpeed = 0f;
        maxRotationSpeed = 50f;

        rotor = GameObject.FindGameObjectWithTag("Rotor");
        periscopeLight = GameObject.FindGameObjectWithTag("Periscope").GetComponentInChildren<Light>();
        leftLight = GameObject.FindGameObjectWithTag("Left Light").GetComponent<Light>();
        rightLight = GameObject.FindGameObjectWithTag("Right Light").GetComponent<Light>();

        periscopeLight.intensity = 0;
        leftLight.intensity = 0;
        rightLight.intensity = 0;

        electricity = maxElectricity;

        tiltModel = GameObject.FindGameObjectWithTag("TiltModel");

        logFilePath = Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".txt";
        File.WriteAllText(logFilePath, "");
        writer = new StreamWriter(logFilePath);
    }

    void Update()
    {
        if (electricity <= 0)
        {
            GameOver();
            return;
        }
        MoveSubmarine();
        ConstrainMove();
        CheckInput();
        DrainElectricity();
        RotorAudio();
    }

    void FixedUpdate()
    {
        string log = "";
        writer.WriteLine(log);
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
        if (Input.GetKey(KeyCode.LeftControl)) verticalRotationSpeed += 1f;
        else if (Input.GetKey(KeyCode.LeftShift)) verticalRotationSpeed -= 1f;
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

        rotor.transform.Rotate(0, 0.5f + velocity * 2f, 0);

        GetComponent<Rigidbody>().velocity = Vector3.zero;

        tiltModel.transform.localRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 0, 0));
    }

    private void ConstrainMove()
    {
        if (transform.position.x > 490) GetComponent<Rigidbody>().AddForce(-Vector3.right);
        else if (transform.position.x < 10) GetComponent<Rigidbody>().AddForce(Vector3.right);

        if (transform.position.z > 490) GetComponent<Rigidbody>().AddForce(-Vector3.forward);
        else if (transform.position.z < 10) GetComponent<Rigidbody>().AddForce(Vector3.forward);
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();

        if (Input.GetKeyDown(KeyCode.F)) ChangeMainLight();
        if (Input.GetKeyDown(KeyCode.C)) ChangeLeftLight();
        if (Input.GetKeyDown(KeyCode.V)) ChangeRightLight();

        if (Input.GetKeyDown(KeyCode.R)) RescueDiver();
    }

    private void ChangeMainLight()
    {
        if (periscopeLight.intensity == 0)
        {
            periscopeLight.DOIntensity(periscopeLightMaxIntensity, 0.5f);
            periscopeLight.transform.parent.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }
        else
        {
            periscopeLight.DOIntensity(0, 0.5f);
            periscopeLight.transform.parent.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        }

        mainLightOn = !mainLightOn;
    }

    private void ChangeLeftLight()
    {
        if (leftLight.intensity == 0) leftLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else leftLight.DOIntensity(0, 0.5f);
        leftLightOn = !leftLightOn;
    }

    private void ChangeRightLight()
    {
        if (rightLight.intensity == 0) rightLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else rightLight.DOIntensity(0, 0.5f);
        rightLightOn = !rightLightOn;
    }

    private void DrainElectricity()
    {
        float coefficient = 1;
        if (mainLightOn) coefficient += 1;
        if (leftLightOn) coefficient += 0.5f;
        if (rightLightOn) coefficient += 0.5f;
        electricity -= Time.deltaTime * coefficient;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Play();
            terrainDragSound.DOFade(1, 1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Stop();
            terrainDragSound.DOFade(0, 1);
        }
    }

    private void RotorAudio()
    {
        if (velocity == 0) rotorSound.Stop();
        else if (!rotorSound.isPlaying) rotorSound.Play();

        rotorSound.pitch = Mathf.Abs(velocity) / maxVelocity;
        rotorSound.volume = rotorSound.pitch / 2;
    }

    private void RescueDiver()
    {
        GameObject[] divers = GameObject.FindGameObjectsWithTag("Diver");
        foreach(GameObject diver in divers)
        {
            if (Vector3.Distance(diver.transform.position, transform.position) < 10f)
            {
                diver.GetComponent<Diver>().GetSaved(terrainParticles.transform.position);
                savedDivers++;
                if (savedDivers == 5)
                {
                    gameWonMenu.SetActive(true);
                    Time.timeScale = 0f;
                    gameOver = true;
                }
                break;
            }
        }
    }

    private void GameOver()
    {
        if (!gameOverMenu.activeSelf)
        {
            gameOverMenu.SetActive(true);
            gameOverMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Game over!\r\n\r\nYou ran out of electricity.\r\n\r\nYou managed to save " + savedDivers + " divers.";
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            gameOver = true;
        }
    }

    public float GetElectricity()
    {
        return electricity;
    }
}
