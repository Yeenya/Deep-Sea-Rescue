using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.IO;
using System;
using System.Text;
using System.Globalization;
using Unity.VisualScripting;

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

    [SerializeField]
    private GameObject diverPrefab;

    [SerializeField]
    private AudioClip[] dockingSounds;

    [SerializeField]
    private AudioSource dockingAudio;

    public bool gameOver = false;

    private string logFilePath;
    private StreamWriter writer;

    private Vector3 lastPosition;
    private float distance = 0f;
    private float time = 0f;

    public int replaySpeed = 1;
    private StreamReader reader;
    [SerializeField]
    private GameObject insideSpot;

    public enum State
    {
        FREE,
        DOCKED,
        REPLAY,
        TUTORIAL
    }

    public State state = State.DOCKED;
    private bool nearDock = false;
    [SerializeField]
    private AudioSource baseSonar;
    private bool baseSonarPlaying = false;
    private bool tiltDelayActive = false;

    public static readonly float[] majorScalePitch =
    {
        1f, 1.122462f, 1.259921f, 1.334840f, 1.498307f, 1.681793f, 1.887749f, 2f, 2.244924f, 2.519842f, 2.669680f, 2.996614f, 3.363586f, 3.775498f, 4f
    };

    public static readonly float[] majorScalePitchMidpoints =
    {
        1.061231f, 1.191192f, 1.297381f, 1.416574f, 1.590050f, 1.784771f, 1.943874f, 2.122462f, 2.382383f, 2.594761f, 2.833147f, 3.180100f, 3.569542f, 3.887749f
    };

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

#if UNITY_STANDALONE
        WriteFilesIfNeccessary();
#endif
    }

    void Update()
    {
        if (state == State.FREE || state == State.DOCKED)
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
            BaseSonarAudio();
            TiltAudio();
        }
        else if (state == State.REPLAY)
        {
            if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
            if (Input.GetKeyDown(KeyCode.KeypadPlus) && replaySpeed < 10) replaySpeed++;
            if (Input.GetKeyDown(KeyCode.KeypadMinus) && replaySpeed > 1) replaySpeed--;
        }
    }

    void FixedUpdate()
    {
        if (state != State.REPLAY)
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
        else
        {
            for (int i = 1; i < replaySpeed; i++) reader.ReadLine();
            string currentLine = reader.ReadLine();
            if (currentLine == null) return;
            string[] currentValues = currentLine.Split(',');
            if (currentValues.Length != 18) return;

            Vector3 position;
            position.x = float.Parse(currentValues[0], CultureInfo.InvariantCulture);
            position.y = float.Parse(currentValues[1], CultureInfo.InvariantCulture);
            position.z = float.Parse(currentValues[2], CultureInfo.InvariantCulture);
            transform.position = position;

            Quaternion submarineRotation;
            submarineRotation.x = float.Parse(currentValues[5], CultureInfo.InvariantCulture);
            submarineRotation.y = float.Parse(currentValues[6], CultureInfo.InvariantCulture);
            submarineRotation.z = float.Parse(currentValues[7], CultureInfo.InvariantCulture);
            submarineRotation.w = float.Parse(currentValues[8], CultureInfo.InvariantCulture);
            transform.rotation = submarineRotation;

            Quaternion cameraRotation;
            cameraRotation.x = float.Parse(currentValues[9], CultureInfo.InvariantCulture);
            cameraRotation.y = float.Parse(currentValues[10], CultureInfo.InvariantCulture);
            cameraRotation.z = float.Parse(currentValues[11], CultureInfo.InvariantCulture);
            cameraRotation.w = float.Parse(currentValues[12], CultureInfo.InvariantCulture);
            if (Camera.main.GetComponent<CameraController>().GetInsideOrOutside())
            {
                Camera.main.transform.localRotation = cameraRotation;
                Camera.main.transform.position = insideSpot.transform.position;
            }

            if (mainLightOn != bool.Parse(currentValues[13])) ChangeMainLight();
            if (leftLightOn != bool.Parse(currentValues[14])) ChangeLeftLight();
            if (rightLightOn != bool.Parse(currentValues[15])) ChangeRightLight();

            tiltModel.transform.localRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 0, 0));
        }
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
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) verticalRotationSpeed += 3f;
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) verticalRotationSpeed -= 3f;
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

        if (Input.GetKeyDown(KeyCode.T)) StartCoroutine(GetComponent<AudioTutorial>().Tutorial());
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
            //electricity -= Time.deltaTime * coefficient;
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
        rotorSound.volume = rotorSound.pitch / 5;
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
                break;
            }
        }
    }

    private void BaseDock()
    {
        if (state == State.DOCKED)
        {
            state = State.FREE;
            dockingAudio.clip = dockingSounds[1];
            dockingAudio.Play();
        }
        else if (state == State.FREE && nearDock)
        {
            state = State.DOCKED;
            dockingAudio.clip = dockingSounds[0];
            dockingAudio.Play();
            Transform dockPoint = GameObject.FindGameObjectWithTag("Tube").transform.GetChild(0);
            transform.SetPositionAndRotation(dockPoint.position, dockPoint.rotation);
            if (savedDivers == 6)
            {
                gameWonMenu.SetActive(true);
                Time.timeScale = 0f;
                gameOver = true;
            }
        }
    }

    private void BaseSonarAudio()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            baseSonarPlaying = !baseSonarPlaying;
            if (baseSonarPlaying) StartCoroutine(BaseSonarPing());
            else
            {
                baseSonar.volume = 0f;
                StopAllCoroutines();
            }
        }
    }

    private IEnumerator BaseSonarPing()
    {
        if (Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) baseSonar.volume = 0.1f;
        else baseSonar.volume = 0f;

        float distance = Vector3.Distance(transform.position, baseSonar.transform.position);
        float nonModifiedPitch = (baseSonar.maxDistance - distance) / baseSonar.maxDistance;
        baseSonar.pitch = 0.5f + (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 0.5f; //easeInSine from https://easings.net/#easeInSine

        baseSonar.Play();

        yield return new WaitForSeconds(5);
        if (baseSonarPlaying) StartCoroutine(BaseSonarPing());
    }

    private void TiltAudio()
    {
        float pitch;
        if (tiltModel.transform.localRotation.eulerAngles.x <= 81)
        {
            pitch = 2f - tiltModel.transform.localRotation.eulerAngles.x / 80f;
        }
        else
        {
            pitch = 2f + (360f - tiltModel.transform.localRotation.eulerAngles.x) / 40f;
        }

        float melodizedPitch = majorScalePitch[0];

        for (int i  = 0; i < majorScalePitchMidpoints.Length; i++)
        {
            if (pitch > majorScalePitchMidpoints[i])
            {
                melodizedPitch = majorScalePitch[i + 1];
            }
            else
            {
                break;
            }
        }

        tiltModel.GetComponent<AudioSource>().pitch = pitch; //melodizedPitch;

        if (verticalRotationSpeed != 0)
        {
            tiltDelayActive = false;
            tiltModel.GetComponent<AudioSource>().volume = 0.025f;
        }
        else if (!tiltDelayActive)
        {
            tiltDelayActive = true;
            tiltModel.GetComponent<AudioSource>().pitch = melodizedPitch; //tiltModel.GetComponent<AudioSource>().DOPitch(melodizedPitch, 0.25f);
            StartCoroutine(TiltDelay());
        }
    }

    private IEnumerator TiltDelay()
    {
        yield return new WaitForSeconds(1);
        tiltModel.GetComponent<AudioSource>().volume = 0f;
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

    [Serializable]
    public class PositionWrapper
    {
        public string entityName;
        public Vector3 position;

        public PositionWrapper(GameObject entity)
        {
            entityName = entity.name;
            position = entity.transform.position;
        }
    }

    [Serializable]
    public class PositionsWrapper
    {
        public List<PositionWrapper> wrappers;

        public PositionsWrapper()
        {
            wrappers = new();
        }
    }

    public void WriteFilesIfNeccessary()
    {
        if (filesWritten || state == State.REPLAY) return;
        filesWritten = true;
        if (!Directory.Exists(Application.persistentDataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Data");
        }
        using (writer = new StreamWriter(Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "_Settings.json"))//csv"))
        {
            PositionsWrapper list = new();
            list.wrappers.Add(new PositionWrapper(gameObject));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("Diver1")));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("Diver2")));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("Diver3")));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("Diver4")));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("Diver5")));
            list.wrappers.Add(new PositionWrapper(GameObject.Find("TestingDiver")));
            writer.Write(JsonUtility.ToJson(list));
            /* Old CSV way
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
            */
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

    public void SetStreamReader(string file)
    {
        GameObject markers = GameObject.Find("Markers");
        if (markers != null) Destroy(markers);
        markers = new GameObject("Markers");
        StreamReader markerReader = new(file);
        markerReader.ReadLine();
        while (!markerReader.EndOfStream)
        {
            for (int i = 0; i < 49; i++) markerReader.ReadLine();
            string currentLine = markerReader.ReadLine();
            if (currentLine == null) break;
            string[] currentValues = currentLine.Split(',');
            if (currentValues.Length != 18) break;

            Vector3 position;
            position.x = float.Parse(currentValues[0], CultureInfo.InvariantCulture);
            position.y = float.Parse(currentValues[1], CultureInfo.InvariantCulture);
            position.z = float.Parse(currentValues[2], CultureInfo.InvariantCulture);

            Quaternion submarineRotation;
            submarineRotation.x = float.Parse(currentValues[5], CultureInfo.InvariantCulture);
            submarineRotation.y = float.Parse(currentValues[6], CultureInfo.InvariantCulture);
            submarineRotation.z = float.Parse(currentValues[7], CultureInfo.InvariantCulture);
            submarineRotation.w = float.Parse(currentValues[8], CultureInfo.InvariantCulture);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.SetPositionAndRotation(position, submarineRotation);
            marker.transform.Rotate(-90, 0, 0);
            marker.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);

            marker.transform.parent = markers.transform;
        }
        markerReader.Close();
        reader = new(file);
        reader.ReadLine();
    }

    public void SetSettings(string file)
    {
        if (file[..^5].Equals(".json"))
        {
            StreamReader settingsReader = new(file + ".json");
            PositionsWrapper list = JsonUtility.FromJson<PositionsWrapper>(settingsReader.ReadToEnd());
            transform.position = list.wrappers[0].position;
            for (int i = 1; i <= 5; i++)
            {
                GameObject.Find("Diver" + i).transform.position = list.wrappers[i].position;
            }
            GameObject.Find("TestingDiver").transform.position = list.wrappers[6].position;
        }
        else if (file[..^4].Equals(".csv"))
        {
            StreamReader settingsReader = new(file + ".csv");
            settingsReader.ReadLine();
            string[] settings = settingsReader.ReadLine().Split(',');
            Vector3 submarinePosition;
            submarinePosition.x = float.Parse(settings[0], CultureInfo.InvariantCulture);
            submarinePosition.y = float.Parse(settings[1], CultureInfo.InvariantCulture);
            submarinePosition.z = float.Parse(settings[2], CultureInfo.InvariantCulture);
            transform.position = submarinePosition;

            Vector3 diver1Position;
            diver1Position.x = float.Parse(settings[3], CultureInfo.InvariantCulture);
            diver1Position.y = float.Parse(settings[4], CultureInfo.InvariantCulture);
            diver1Position.z = float.Parse(settings[5], CultureInfo.InvariantCulture);
            GameObject.Find("Diver1").transform.position = diver1Position;

            Vector3 diver2Position;
            diver2Position.x = float.Parse(settings[6], CultureInfo.InvariantCulture);
            diver2Position.y = float.Parse(settings[7], CultureInfo.InvariantCulture);
            diver2Position.z = float.Parse(settings[8], CultureInfo.InvariantCulture);
            GameObject.Find("Diver2").transform.position = diver2Position;

            Vector3 diver3Position;
            diver3Position.x = float.Parse(settings[9], CultureInfo.InvariantCulture);
            diver3Position.y = float.Parse(settings[10], CultureInfo.InvariantCulture);
            diver3Position.z = float.Parse(settings[11], CultureInfo.InvariantCulture);
            GameObject.Find("Diver3").transform.position = diver3Position;

            Vector3 diver4Position;
            diver4Position.x = float.Parse(settings[12], CultureInfo.InvariantCulture);
            diver4Position.y = float.Parse(settings[13], CultureInfo.InvariantCulture);
            diver4Position.z = float.Parse(settings[14], CultureInfo.InvariantCulture);
            GameObject.Find("Diver4").transform.position = diver4Position;

            Vector3 diver5Position;
            diver5Position.x = float.Parse(settings[15], CultureInfo.InvariantCulture);
            diver5Position.y = float.Parse(settings[16], CultureInfo.InvariantCulture);
            diver5Position.z = float.Parse(settings[17], CultureInfo.InvariantCulture);
            GameObject.Find("Diver5").transform.position = diver5Position;

            Vector3 testingDiverPosition;
            testingDiverPosition.x = float.Parse(settings[18], CultureInfo.InvariantCulture);
            testingDiverPosition.y = float.Parse(settings[19], CultureInfo.InvariantCulture);
            testingDiverPosition.z = float.Parse(settings[20], CultureInfo.InvariantCulture);
            GameObject.Find("TestingDiver").transform.position = testingDiverPosition;
        }
    }

    public GameObject GetDiverPrefab()
    {
        return diverPrefab;
    }

    public bool GetLightsOn()
    {
        return leftLightOn && mainLightOn && rightLightOn;
    }

    public int GetSavedDivers()
    {
        return savedDivers;
    }

    public bool IsDocked()
    {
        return state == State.DOCKED;
    }
}
