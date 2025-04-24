using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.IO;
using System;
using System.Globalization;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the Player and everything related.
 */
public class Player : MonoBehaviour
{
    private Vector3 testingDiverPos = new(212.271439f, 119.490868f, 219.693787f);
    private Vector3 diver1Pos = new(281.881012f, 36.8699989f, 294.869995f);
    private Vector3 diver2Pos = new(87.8600006f, 23.5f, 94.5f);
    private Vector3 diver3Pos = new(50.2999992f, 40.2999992f, 442.5f);
    private Vector3 diver4Pos = new(443.540009f, 39.0499992f, 90.5899963f);
    private Vector3 diver5Pos = new(427.019989f, 60.6500015f, 438.25f);
    private Vector3 basePos = new(207.893082f, 131.025192f, 187.043793f);

    public readonly static Dictionary<string, List<int>> ordering = new()
    {
        // Order in which each of the players collected the divers. In future, this should be loaded from the replay.
        ["11-02-2025_10-24-56.csv"] = new List<int>() { 0, 1, 5, 6 },
        ["11-02-2025_11-43-22.csv"] = new List<int>() { 0, 1, 5, 2, 6 },
        ["11-02-2025_13-52-54.csv"] = new List<int>() { 0, 1, 2, 6 },
        ["16-12-2024_14-07-40.csv"] = new List<int>() { 0, 1, 6 },
        ["19-12-2024_11-54-36.csv"] = new List<int>() { 0, 1, 3, 2, 6 },
        ["23-03-2025_21-33-31.csv"] = new List<int>() { 0, 1, 5, 3, 2, 4, 6 },
        ["25-03-2025_19-17-19.csv"] = new List<int>() { 0, 1, 5, 2, 4, 3, 6 },
        ["03-04-2025_17-02-37.csv"] = new List<int>() { 0, 2, 1, 6 },
        ["03-04-2025_17-27-39.csv"] = new List<int>() { 0, 1, 5, 2, 4, 3, 6 },
        ["13-04-2025_20-40-37.csv"] = new List<int>() { 0, 1, 5, 4, 2, 6 },
        ["17-04-2025_14-32-41.csv"] = new List<int>() { 0, 1, 5, 5},
        ["17-04-2025_14-51-13.csv"] = new List<int>() { 0, 1, 5, 2, 3, 4, 6},
        ["21-04-2025_19-31-35.csv"] = new List<int>() { 0, 1, 5, 4, 2, 3, 6}
    };

    private List<Vector3> positions;
    private string fileName;

    [SerializeField]
    private float currentAngle; // Used just for debugging purposes. Visible in the Inspector to check that the angle is calculated correctly.

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

    public int savedDivers = 0;

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
    public int currentLineCount = 0;
    private StreamReader reader;
    [SerializeField]
    private GameObject insideSpot;

    /*
     * Describes the state of the player.
     */
    public enum State
    {
        FREE, // The player is undocked and freely moving throughout the scene.
        DOCKED, // The player is docked.
        REPLAY, // The player is reviewing a replay.
        TUTORIAL // The player is just doing the tutorial.
    }

    public State state = State.DOCKED;
    private bool nearDock = false;
    [SerializeField]
    private AudioSource baseSonar;
    private bool baseSonarPlaying = false;
    private bool tiltDelayActive = false;

    // Pitch values for a major scale (1 is the root note)
    public static readonly float[] majorScalePitch =
    {
        1f, 1.122462f, 1.259921f, 1.334840f, 1.498307f, 1.681793f, 1.887749f, 2f, 2.244924f, 2.519842f, 2.669680f, 2.996614f, 3.363586f, 3.775498f, 4f
    };

    // Midpoints between the major scale pitch values. Used for clamping to the closest note.
    public static readonly float[] majorScalePitchMidpoints =
    {
        1.061231f, 1.191192f, 1.297381f, 1.416574f, 1.590050f, 1.784771f, 1.943874f, 2.122462f, 2.382383f, 2.594761f, 2.833147f, 3.180100f, 3.569542f, 3.887749f
    };

    void Start()
    {
        // Set variables to default values.

        positions = new List<Vector3>() { testingDiverPos, diver1Pos, diver2Pos, diver3Pos, diver4Pos, diver5Pos, basePos };

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

        // If the player is playing in a build, prepare the replay files. There is a bug that even if you play in the editor, the files are created,
        // so you can easily "spam" your folder with many almost empty data files. Comment the line calling WriteFilesIfNeccessary function if needed.
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
            // In replay, the player can freely change inside or outside view and the speed of the replay. Everything else is driven by the replay itself.
            if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
            if (Input.GetKeyDown(KeyCode.KeypadPlus) && replaySpeed < 20) replaySpeed++;
            if (Input.GetKeyDown(KeyCode.KeypadMinus) && replaySpeed > 1) replaySpeed--;
        }
    }

    /*
     * FixedUpdate is used for writing / reading replay data.
     */
    void FixedUpdate()
    {
        if (state != State.REPLAY) // Player is normally playing.
        {
            if (!filesWritten) return; // If data files were not (correctly) prepared, do not write anything.
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
        else // Player is viewing a replay.
        {
            // Skip as many lines as the replay speed is set to.
            for (int i = 1; i < replaySpeed; i++)
            {
                currentLineCount++;
                reader.ReadLine();
            }
            string currentLine = reader.ReadLine();
            currentLineCount++;
            if (currentLine == null) return; // If the line is null, stop for safety reasons.
            string[] currentValues = currentLine.Split(',');
            if (currentValues.Length != 18) return; // If the data line does not contain all data it should, stop for safety reasons.

            // Set the submarine to its correct position, rotation, set its lights and tilt model.

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


            // Draw a debug line which is from the submarine to the diver which is going to be saved next (or base at the end).
            // The line is green if the submarine is close enough to hear the diver, red otherwise.
            savedDivers = int.Parse(currentValues[16], CultureInfo.InvariantCulture);
            Vector3 currentTargetPos = positions[ordering[fileName.Split('\\')[1]][int.Parse(currentValues[16], CultureInfo.InvariantCulture)]];
            if (Vector3.Distance(currentTargetPos, transform.position) <= 250f) Debug.DrawLine(transform.position, currentTargetPos, Color.green);
            else Debug.DrawLine(transform.position, currentTargetPos, Color.red);

            //If necessary, uncomment to draw a flattened lines to show the horizontal angle between the submarine facing direction and
            //the direction to the diver going to be saved. Used just for debugging purposes.
            //Vector3 targetVector = Vector3.Normalize(currentTargetPos - transform.position);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(0, transform.forward.y, transform.forward.z) * 20, Color.blue);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(0, targetVector.y, targetVector.z) * 20, Color.blue);

            // Compute the current angle between the submarine facing direction and the direction to the diver going to be saved. Used just for debugging purposes.
            Vector3 rotationVector = transform.forward;
            currentAngle = Vector3.Angle(rotationVector, Vector3.Normalize(currentTargetPos - transform.position));
        }
    }

    /*
     * Replace decimal separator in the float number with a dot for CSV compatibilty of float values.
     */
    private string ReplaceDecimal(float number)
    {
        return number.ToString().Replace(',', '.');
    }

    /*
     * Custom ToString function for Vector3s. It replaces the decimal separator with a dot for CSV compatibility.
     */
    private string Vector3ToString(Vector3 vector)
    {
        string result = ReplaceDecimal(vector.x) + "," + ReplaceDecimal(vector.y) + "," + ReplaceDecimal(vector.z);
        return result;
    }

    /*
     * Custom ToString function for Quaternions. It replaces the decimal separator with a dot for CSV compatibility.
     */
    private string QuaternionToString(Quaternion quaternion)
    {
        string result = ReplaceDecimal(quaternion.x) + "," + ReplaceDecimal(quaternion.y) + "," + ReplaceDecimal(quaternion.z) + "," + ReplaceDecimal(quaternion.w);
        return result;
    }

    /*
     * Check for player input and move the submarine accordingly.
     * Also constrain the rotation, velocity etc.
     */
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
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.DownArrow)) verticalRotationSpeed += 3f;
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.UpArrow)) verticalRotationSpeed -= 3f;
        else if (verticalRotationSpeed != 0) verticalRotationSpeed -= Mathf.Sign(verticalRotationSpeed) * 5;

        if (verticalRotationSpeed > maxRotationSpeed) verticalRotationSpeed = maxRotationSpeed;
        else if (verticalRotationSpeed < -maxRotationSpeed) verticalRotationSpeed = -maxRotationSpeed;
        else if (verticalRotationSpeed < 3 && verticalRotationSpeed > -3) verticalRotationSpeed = 0;
        
        // Apply movement to position and rotation
        if (velocity != 0) transform.position += Time.deltaTime * velocity * transform.forward;
        transform.Rotate(new Vector3(verticalRotationSpeed, horizontalRotationSpeed, 0) * Time.deltaTime);
        transform.Rotate(0, 0, -transform.rotation.eulerAngles.z);

        // Limit x rotation so that player doesn't spin to infinity (If the player is facing completely up or down, the spinning is broken and insanely fast.)
        Vector3 correctRotation = transform.rotation.eulerAngles;
        if (correctRotation.x > 80 && correctRotation.x < 180) correctRotation.x = 80;
        if (correctRotation.x < 280 && correctRotation.x > 180) correctRotation.x = 280;
        transform.rotation = Quaternion.Euler(correctRotation);

        // Reset rotation of the player so that the submarine "lies flat" - is aligned with the horizontal plane.
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) transform.rotation = Quaternion.Euler(0, correctRotation.y, correctRotation.z);

        rotor.transform.Rotate(0, 0.5f + velocity * 2f, 0); // Rotate the rotor at the back of the submarine according to the speed. Just a visual feature.

        GetComponent<Rigidbody>().velocity = Vector3.zero; // Because the movement is not done through Rididbody, eliminate any velocity possibly added to the Rigidbody.

        tiltModel.transform.localRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 0, 0)); // Tilt the model according to the tilt of the submarine.
    }

    /*
     * Do not let the player go outside of the map. The numbers are hardcoded for map size 500 x 500.
     */
    private void ConstrainMove()
    {
        if (transform.position.x > 490) transform.position = new Vector3(490, transform.position.y, transform.position.z);
        else if (transform.position.x < 10) transform.position = new Vector3(10, transform.position.y, transform.position.z);

        if (transform.position.z > 490) transform.position = new Vector3(transform.position.x, transform.position.y, 490);
        else if (transform.position.z < 10) transform.position = new Vector3(transform.position.x, transform.position.y, 10);
    }

    /*
     * Check whether player pressed any of the keys outside of moving keys.
     */
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

    /*
     * Turn the main light (facing forward) on or off.
     */
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

    /*
     * Turn the left light on or off.
     */
    private void ChangeLeftLight()
    {
        if (leftLight.intensity == 0) leftLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else leftLight.DOIntensity(0, 0.5f);
        leftLightOn = !leftLightOn;
    }

    /*
     * Turn the right light on or off.
     */
    private void ChangeRightLight()
    {
        if (rightLight.intensity == 0) rightLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else rightLight.DOIntensity(0, 0.5f);
        rightLightOn = !rightLightOn;
    }

    /*
     * Drain electricity according to the number of lights on.
     */
    private void ChangeElectricity()
    {
        if (state == State.FREE)
        {
            float coefficient = 1;
            if (mainLightOn) coefficient += 1;
            if (leftLightOn) coefficient += 0.5f;
            if (rightLightOn) coefficient += 0.5f;
            //electricity -= Time.deltaTime * coefficient; // For testing purposes, the electricity is not drained. Uncomment to add time constrain to the player's experience.
        }
        else if (state == State.DOCKED && chargedElectricity < maxElectricity / 2) // If the player is docked, charge some limited amount of electricity.
        {
            float addition = Time.deltaTime * 5f;
            electricity += addition;
            chargedElectricity += addition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the player is colliding with the terrain, play the terrain drag sound and start the ParticleSystem of sand.
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Play();
            terrainDragSound.DOFade(1, 1);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Handle the particles and drag sound when the player is still in contact with the terrain.
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
        // Stop the particles and drag sound when the player is no longer in contact with the terrain.
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Stop();
            terrainDragSound.DOFade(0, 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Used only for debugging purposes, when testing whether the docking is correctly detected during tutorial.
        if (other.CompareTag("Tube"))
        {
            nearDock = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Used only for debugging purposes, when testing whether the docking is correctly detected during tutorial.
        if (other.CompareTag("Tube"))
        {
            nearDock = false;
        }
    }

    /*
     * Change the rotor sound and pitch according to the speed of the submarine.
     */
    private void RotorAudio()
    {
        if (velocity == 0) rotorSound.Stop();
        else if (!rotorSound.isPlaying) rotorSound.Play();

        rotorSound.pitch = Mathf.Abs(velocity) / maxVelocity;
        rotorSound.volume = rotorSound.pitch / 5;
    }

    /*
     * Look if some diver is close enough to the submarine and if so, save him.
     */
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

    /*
     * Handle docking / undocking.
     */
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
            if (savedDivers == 6) // If all divers are saved, show the win menu.
            {
                gameWonMenu.SetActive(true);
                Time.timeScale = 0f;
                gameOver = true;
            }
        }
    }

    /*
     * Handling the base pinging sound.
     */
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

    /*
     * This function modifies the base pinging sound in the same way as divers do.
     */
    private IEnumerator BaseSonarPing()
    {
        if (Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) baseSonar.volume = 0.1f;
        else baseSonar.volume = 0f;

        float distance = Vector3.Distance(transform.position, baseSonar.transform.position);
        float nonModifiedPitch = (baseSonar.maxDistance - distance) / baseSonar.maxDistance;
        baseSonar.pitch = 0.5f + (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 0.5f; //easeInSine from https://easings.net/#easeInSine

        baseSonar.Play();

        yield return new WaitForSeconds(5); // Wait five seconds before another ping.
        if (baseSonarPlaying) StartCoroutine(BaseSonarPing());
    }

    /*
     * This function handles the audio cue of tilting the submarine.
     */
    private void TiltAudio()
    {
        // Set the correct pitch (Because facing above horizontal plane return a totally different local rotation value than facing below horizontal plane.)
        float pitch;
        if (tiltModel.transform.localRotation.eulerAngles.x <= 81)
        {
            pitch = 2f - tiltModel.transform.localRotation.eulerAngles.x / 80f;
        }
        else
        {
            pitch = 2f + (360f - tiltModel.transform.localRotation.eulerAngles.x) / 40f;
        }

        // Find the closest melodized pitch value.
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

        tiltModel.GetComponent<AudioSource>().pitch = pitch; // swap to melodizedPitch if you want want melodized pitch throughout the whole tilting procedure.

        // Play the tilt sound when tilting and also after tilting for some time so that player can properly analyze the sound.
        if (verticalRotationSpeed != 0)
        {
            tiltDelayActive = false;
            tiltModel.GetComponent<AudioSource>().volume = 0.025f;
        }
        else if (!tiltDelayActive)
        {
            tiltDelayActive = true;
            tiltModel.GetComponent<AudioSource>().pitch = melodizedPitch; // set the pitch to the melodized one when the player stopped tilting.
            StartCoroutine(TiltDelay());
        }
    }

    /*
     * A simple coroutine to mute the tilting sound after one second.
     */
    private IEnumerator TiltDelay()
    {
        yield return new WaitForSeconds(1);
        tiltModel.GetComponent<AudioSource>().volume = 0f;
    }

    /*
     * Show the game over menu if the player runs out of electricity.
     */
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

    /*
     * A simple getter function for electricity.
     */
    public float GetElectricity()
    {
        return electricity;
    }

    // Wrappers for saving values into JSON files (for safety reasons and possible interference of JSON syntax with the syntax of vectors, quaternions etc.)

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

    /*
     * A function called at the start of the game (if wanted) to prepare the data files for the player.
     */
    public void WriteFilesIfNeccessary()
    {
        if (filesWritten || state == State.REPLAY) return;
        filesWritten = true;

        // Check if the Data directory exists, if not, create it.
        if (!Directory.Exists(Application.persistentDataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Data");
        }

        // Write the Settings JSON file.
        using (writer = new StreamWriter(Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "_Settings.json"))
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
        }

        // Write the headers of the data CSV file.
        logFilePath = Application.persistentDataPath + "/Data/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".csv";
        File.WriteAllText(logFilePath, ""); // Flush the file if it already exists.
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

    /*
     * Set a stream reader that is used for viewing a replay.
     */
    public void SetStreamReader(string file)
    {
        // Set file name and prepare the trajectory markers in the scene.
        fileName = file;
        GameObject markers = GameObject.Find("Markers");
        if (markers != null) Destroy(markers);
        markers = new GameObject("Markers");
        StreamReader markerReader = new(file);
        markerReader.ReadLine(); // Skip the header line.
        while (!markerReader.EndOfStream)
        {
            for (int i = 0; i < 49; i++) markerReader.ReadLine(); // A marker is placed for every 50th line of the data file, meaning one marker per second.
            string currentLine = markerReader.ReadLine();
            if (currentLine == null) break; // safety break
            string[] currentValues = currentLine.Split(',');
            if (currentValues.Length != 18) break; // safety break no. 2

            // Create a marker and set its position and rotation.

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

    /*
     * Read the settings from the JSON file and set the submarine and divers to their correct positions. Useful if you plan to change diver (or submarine) positions.
     */
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

    /*
     * A simple getter function for diver prefab.
     */
    public GameObject GetDiverPrefab()
    {
        return diverPrefab;
    }

    /*
     * A simple getter function for getting whether all lights are on. Used in the tutorial.
     */
    public bool GetLightsOn()
    {
        return leftLightOn && mainLightOn && rightLightOn;
    }

    /*
     * A simple getter function for the number of currently saved divers.
     */
    public int GetSavedDivers()
    {
        return savedDivers;
    }

    /*
     * A simple getter function for docking state.
     */
    public bool IsDocked()
    {
        return state == State.DOCKED;
    }
}
