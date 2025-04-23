using System.Collections;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the narrated tutorial (started by pressing T by default) used before the play itself.
 */
public class AudioTutorial : MonoBehaviour
{
    private Player player;
    private Vector3 startingPosition = new(207.893082f, 131.025192f, 189.029999f);

    [SerializeField]
    private GameObject moveSphere;

    [SerializeField]
    private GameObject tiltSphere;

    [SerializeField]
    private AudioClip[] voiceovers;

    [SerializeField]
    private GameObject tutorialRoot;

    [SerializeField]
    private AudioSource voiceoverAudio;

    private short currentVoiceover = -1;

    void Start()
    {
        player = GetComponent<Player>();
    }

    /*
     * Coroutine handling the tutorial step by step.
     * If necessary, uncomment the prints to see in which phase the tutorial is in Console.
     */
    public IEnumerator Tutorial()
    {
        // Prepare the scene and the player
        tutorialRoot.SetActive(true);
        transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
        player.state = Player.State.DOCKED;
        if (!Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
        
        // Disable all divers
        GameObject[] divers = GameObject.FindGameObjectsWithTag("Diver");
        foreach (GameObject diver in divers)
        {
            diver.GetComponent<AudioSource>().volume = 0f;
            diver.GetComponent<AudioSource>().Stop();
        }

        // 1 - UNDOCK
        AdvanceVoiceovers();
        yield return new WaitUntil(() => player.state == Player.State.FREE);
        //print("Undocked, get to sphere");

        // 2 - INTRODUCE SONAR SOUND
        AdvanceVoiceovers();
        moveSphere.GetComponent<AudioSource>().Play();

        // 3 - MOVE
        yield return new WaitUntil(() => Vector3.Distance(moveSphere.transform.position, transform.position) <= 8f);
        //print("Got move sphere, get to tilt sphere");
        Destroy(moveSphere.transform.parent.GetChild(moveSphere.transform.GetSiblingIndex() + 1).gameObject);
        Destroy(moveSphere);

        // 4 - TILT + HEIGHT WARNING
        tiltSphere.GetComponent<AudioSource>().Play();
        AdvanceVoiceovers();
        yield return new WaitUntil(() => Vector3.Distance(tiltSphere.transform.position, transform.position) <= 8f);
        AdvanceVoiceovers();
        //print("Got tilt sphere, change view");
        Destroy(tiltSphere.transform.parent.GetChild(tiltSphere.transform.GetSiblingIndex() + 1).gameObject);
        Destroy(tiltSphere);

        // 5 - CHANGE VIEW
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == false);
        AdvanceVoiceovers();
        //print("Changed view, change view again");
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == true);
        //print("Changed view again, listen to diver sounds.");

        // 6 - LIGHTS
        AdvanceVoiceovers();
        yield return new WaitUntil(() => player.GetLightsOn());
        //print("Turned on all lights, listen to diver sounds");

        // 7 - FIND + RESCUE DIVER
        AdvanceVoiceovers();
        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);

        // Setup the diver for listening examples
        GameObject tutorialDiver = Instantiate(GetComponent<Player>().GetDiverPrefab(), transform.position - transform.right * 10, Quaternion.identity);
        AudioSource tutorialDiverAudio = tutorialDiver.GetComponent<AudioSource>();
        Diver tDiverComponent = tutorialDiver.GetComponent<Diver>();
        tDiverComponent.Init();
        tDiverComponent.UpdateSound();

        // The next sections move the diver sequentially to various positions and distances from the player, while playing the diver sound
        // It starts on the left 10m away, then left 50m, left 100m,
        // front 10m, front 50m, front 100m,
        // right 10m, right 50m, right 100m
        // back 10m, back 50m, back 100m.

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.right * 50;
        tDiverComponent.UpdateSound();

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.right * 100;
        tDiverComponent.UpdateSound();

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 10;
        tDiverComponent.UpdateSound();

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 50;
        tDiverComponent.UpdateSound();

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 100;
        tDiverComponent.UpdateSound();

        //print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.right * 10;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.right * 50;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.right * 100;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.forward * 10;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.forward * 50;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.forward * 100;
        tDiverComponent.UpdateSound();

        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);
        
        Destroy(tutorialDiver);

        //print("Listened to diver sounds, rescue diver");
        
        AdvanceVoiceovers();
        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);
        tutorialDiver = GameObject.Find("TestingDiver");
        if (tutorialDiver != null)
        {
            tutorialDiver.GetComponent<AudioSource>().volume = 1f;
            tutorialDiver.GetComponent<AudioSource>().Play();
        }
        yield return new WaitUntil(() => player.GetSavedDivers() == 1);
        //print("Diver rescued, go dock");

        //RETURN TO BASE + DOCK
        AdvanceVoiceovers();
        yield return new WaitUntil(Finish); // This tends to not detect that player docked correctly even though everything is set up correctly.
        //print("Docked, finish");
        AdvanceVoiceovers();

        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);

        // Reset the divers to their original state
        foreach (GameObject diver in divers)
        {
            if (diver == null) continue;
            diver.GetComponent<AudioSource>().volume = 1f;
            diver.GetComponent<AudioSource>().Play();
        }
    }

    /*
     * A simple function which advances the voiceover to the next one.
     */
    private void AdvanceVoiceovers()
    {
        currentVoiceover++;
        voiceoverAudio.clip = voiceovers[currentVoiceover];
        voiceoverAudio.Play();
    }

    bool Finish()
    {
        // A print to check if the condition should pass or not. This is where the problem is.
        // print(Vector3.Distance(transform.position, startingPosition) + " " + (player.IsDocked() || Vector3.Distance(transform.position, startingPosition) <= 0.5f));
        if (player.IsDocked() || Vector3.Distance(transform.position, startingPosition) <= 1.0f) return true;
        else return false;
    }
}
