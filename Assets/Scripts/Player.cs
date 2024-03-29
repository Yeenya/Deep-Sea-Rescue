using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;
using UnityEditor.Experimental.GraphView;

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

    public readonly float maxElectricity = 900f;
    private float electricity;
    private float chargedElectricity = 0f;
    private bool mainLightOn = false;
    private bool leftLightOn = false;
    private bool rightLightOn = false;

    private int savedDivers = 0;

    private bool filesWritten = false;

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

    private string logFilePath;
    private StreamWriter writer;

    private Vector3 lastPosition;
    private float distance = 0f;
    private float time = 0f;

    private enum State
    {
        FREE,
        DOCKED,
        REPLAY
    }

    private State state = State.DOCKED;
    private bool nearDock = false;
    [SerializeField]
    private AudioSource baseSonar;

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

        lastPosition = transform.position;
    }

    void Update()
    {
        if (electricity <= 0)
        {
            GameOver();
            return;
        }
        if (state == State.FREE)
        {
            MoveSubmarine();
        }
        ConstrainMove();
        CheckInput();
        ChangeElectricity();
        RotorAudio();
        ChangeBaseSonar();
    }

    void FixedUpdate()
    {
        if (!filesWritten) return;
        distance += Vector3.Distance(lastPosition, transform.position);
        lastPosition = transform.position;
        time += Time.fixedDeltaTime;
        string log = "";
        log += Vector3ToString(transform.position) + ",";
        log += ReplaceDecimal(distance) + ",";
        log += ReplaceDecimal(time) + ",";
        log += QuaternionToString(transform.rotation) + ",";
        log += QuaternionToString(Camera.main.transform.localRotation) + ",";
        log += mainLightOn + ",";
        log += leftLightOn + ",";
        log += rightLightOn + ",";
        log += savedDivers + ",";
        log += ReplaceDecimal(electricity);
        writer.WriteLine(log);
    }

    private string ReplaceDecimal(float number)
    {
        return number.ToString().Replace(',', '.');
    }

    private string Vector3ToString(Vector3 vector)
    {
        string result = ReplaceDecimal(vector.x) + "," + ReplaceDecimal(vector.y) + "," + ReplaceDecimal(vector.z);
        return result;
    }

    private string QuaternionToString(Quaternion quaternion)
    {
        string result = ReplaceDecimal(quaternion.x) + "," + ReplaceDecimal(quaternion.y) + "," + ReplaceDecimal(quaternion.z) + "," + ReplaceDecimal(quaternion.w);
        return result;
    }

    private void MoveSubmarine()
    {
        // Forward/backward
        if (Input.GetKey(KeyCode.W)) velocity += 0.1f;
        else if (Input.GetKey(KeyCode.S)) velocity -= 0.1f;
        else if (velocity != 0) velocity -= Mathf.Sign(velocity) * 0.2f;

        if (velocity > maxVelocity) velocity = maxVelocity;
        else if (velocity < -maxVelocity) velocity = -maxVelocity;
        else if (velocity < 0.1 && velocity > -0.1) velocity = 0;

        float velocitySign = velocity != 0 ? Mathf.Sign(velocity) : 1;

        // Left/right
        if (Input.GetKey(KeyCode.A)) horizontalRotationSpeed -= 3f * velocitySign;
        else if (Input.GetKey(KeyCode.D)) horizontalRotationSpeed += 3f * velocitySign;
        else if (horizontalRotationSpeed != 0) horizontalRotationSpeed -= Mathf.Sign(horizontalRotationSpeed) * 5;

        if (horizontalRotationSpeed > maxRotationSpeed) horizontalRotationSpeed = maxRotationSpeed;
        else if (horizontalRotationSpeed < -maxRotationSpeed) horizontalRotationSpeed = -maxRotationSpeed;
        else if (horizontalRotationSpeed < 3 && horizontalRotationSpeed > -3) horizontalRotationSpeed = 0;

        // Up/down
        if (Input.GetKey(KeyCode.LeftControl)) verticalRotationSpeed += 3f;
        else if (Input.GetKey(KeyCode.LeftShift)) verticalRotationSpeed -= 3f;
        else if (verticalRotationSpeed != 0) verticalRotationSpeed -= Mathf.Sign(verticalRotationSpeed) * 5;

        if (verticalRotationSpeed > maxRotationSpeed) verticalRotationSpeed = maxRotationSpeed;
        else if (verticalRotationSpeed < -maxRotationSpeed) verticalRotationSpeed = -maxRotationSpeed;
        else if (verticalRotationSpeed < 3 && verticalRotationSpeed > -3) verticalRotationSpeed = 0;
        
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
        if (transform.position.x > 490) transform.position = new Vector3(490, transform.position.y, transform.position.z);
        else if (transform.position.x < 10) transform.position = new Vector3(10, transform.position.y, transform.position.z);

        if (transform.position.z > 490) transform.position = new Vector3(transform.position.x, transform.position.y, 490);
        else if (transform.position.z < 10) transform.position = new Vector3(transform.position.x, transform.position.y, 10);
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();

        if (Input.GetKeyDown(KeyCode.F)) ChangeMainLight();
        if (Input.GetKeyDown(KeyCode.C)) ChangeLeftLight();
        if (Input.GetKeyDown(KeyCode.V)) ChangeRightLight();

        if (Input.GetKeyDown(KeyCode.R)) RescueDiver();
        if (Input.GetKeyDown(KeyCode.Space)) BaseDock();
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

    private void ChangeElectricity()
    {
        if (state == State.FREE)
        {
            float coefficient = 1;
            if (mainLightOn) coefficient += 1;
            if (leftLightOn) coefficient += 0.5f;
            if (rightLightOn) coefficient += 0.5f;
            electricity -= Time.deltaTime * coefficient;
        }
        else if (state == State.DOCKED && chargedElectricity < maxElectricity / 2)
        {
            float addition = Time.deltaTime * 5f;
            electricity += addition;
            chargedElectricity += addition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Play();
            terrainDragSound.DOFade(1, 1);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            if (velocity == 0 && terrainParticles.isPlaying)
            {
                terrainParticles.Stop();
                terrainDragSound.DOFade(0, 1);
            }
            else if (velocity != 0 && !terrainParticles.isPlaying)
            {
                terrainParticles.Play();
                terrainDragSound.DOFade(1, 1);
            }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tube"))
        {
            nearDock = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tube"))
        {
            nearDock = false;
        }
    }

    private void RotorAudio()
    {
        if (velocity == 0) rotorSound.Stop();
        else if (!rotorSound.isPlaying) rotorSound.Play();

        rotorSound.pitch = Mathf.Abs(velocity) / maxVelocity;
        rotorSound.volume = rotorSound.pitch / 4;
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
                if (savedDivers == 6)
                {
                    gameWonMenu.SetActive(true);
                    Time.timeScale = 0f;
                    gameOver = true;
                }
                break;
            }
        }
    }

    private void BaseDock()
    {
        if (state == State.DOCKED)
        {
            state = State.FREE;
        }
        else if (state == State.FREE && nearDock)
        {
            state = State.DOCKED;
            Transform dockPoint = GameObject.FindGameObjectWithTag("Tube").transform.GetChild(0);
            transform.SetPositionAndRotation(dockPoint.position, dockPoint.rotation);
        }
    }

    private void ChangeBaseSonar()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (baseSonar.isPlaying) baseSonar.Stop();
            else baseSonar.Play();
        }

        if (baseSonar.isPlaying)
        {
            float distance = Vector3.Distance(transform.position, baseSonar.transform.position);
            if (Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) baseSonar.volume = 0.25f;
            else baseSonar.volume = 0f;
            float nonModifiedPitch = (baseSonar.maxDistance - distance) / baseSonar.maxDistance;
            baseSonar.pitch = 0.5f + (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 0.5f; //easeInSine from https://easings.net/#easeInSine
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

    public void WriteFilesIfNeccessary()
    {
        if (filesWritten || GetComponent<ReplayPlayer>().enabled) return;
        filesWritten = true;
        if (!Directory.Exists(Application.persistentDataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Data");
        }
        using (writer = new StreamWriter(Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "_Settings.csv"))
        {
            writer.WriteLine(
                "Submarine Position X,Submarine Position Y,Submarine Position Z," +
                "Diver1 Position X,Diver1 Position Y,Diver1 Position Z," +
                "Diver2 Position X,Diver2 Position Y,Diver2 Position Z," +
                "Diver3 Position X,Diver3 Position Y,Diver3 Position Z," +
                "Diver4 Position X,Diver4 Position Y,Diver4 Position Z," +
                "Diver5 Position X,Diver5 Position Y,Diver5 Position Z," +
                "Testing Diver Position X,Testing Diver Position Y,Testing Diver Position Z");
            writer.WriteLine(Vector3ToString(transform.position) + ","
                + Vector3ToString(GameObject.Find("Diver1").transform.position) + ","
                + Vector3ToString(GameObject.Find("Diver2").transform.position) + ","
                + Vector3ToString(GameObject.Find("Diver3").transform.position) + ","
                + Vector3ToString(GameObject.Find("Diver4").transform.position) + ","
                + Vector3ToString(GameObject.Find("Diver5").transform.position) + ","
                + Vector3ToString(GameObject.Find("TestingDiver").transform.position));
        }
        logFilePath = Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".csv";
        File.WriteAllText(logFilePath, "");
        writer = new StreamWriter(logFilePath);
        writer.WriteLine(
            "Position X,Position Y,Position Z," +
            "Distance," +
            "Time," +
            "Submarine Rotation X,Submarine Rotation Y,Submarine Rotation Z,Submarine Rotation W," +
            "Camera Rotation X,Camera Rotation Y,Camera Rotation Z,Camera Rotation W," +
            "Main Light On," +
            "Left Light On," +
            "Right Light On," +
            "Saved Divers," +
            "Electricity");
    }
}
