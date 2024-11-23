using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private short currentVoiceover = -1;

    [SerializeField]
    private AudioSource voiceoverAudio;

    void Start()
    {
        player = GetComponent<Player>();
    }

    public IEnumerator Tutorial()
    {
        tutorialRoot.SetActive(true);
        transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
        player.state = Player.State.DOCKED;
        if (!Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
        GameObject[] divers = GameObject.FindGameObjectsWithTag("Diver");
        foreach (GameObject diver in divers)
        {
            diver.GetComponent<AudioSource>().volume = 0f;
            diver.GetComponent<AudioSource>().Stop();
        }

        //UNDOCK
        AdvanceVoiceovers();
        yield return new WaitUntil(() => player.state == Player.State.FREE);
        print("Undocked, get to sphere");

        //INTRODUCE SONAR SOUND
        AdvanceVoiceovers();
        moveSphere.GetComponent<AudioSource>().Play();

        //MOVE
        //yield return new WaitUntil(() => !voiceoverAudio.isPlaying);
        //AdvanceVoiceovers();
        yield return new WaitUntil(() => Vector3.Distance(moveSphere.transform.position, transform.position) <= 8f);
        print("Got move sphere, get to tilt sphere");
        Destroy(moveSphere.transform.parent.GetChild(moveSphere.transform.GetSiblingIndex() + 1).gameObject);
        Destroy(moveSphere);

        //TILT + height warning
        tiltSphere.GetComponent<AudioSource>().Play();
        AdvanceVoiceovers();
        yield return new WaitUntil(() => Vector3.Distance(tiltSphere.transform.position, transform.position) <= 8f);
        AdvanceVoiceovers();
        print("Got tilt sphere, change view");
        Destroy(tiltSphere.transform.parent.GetChild(tiltSphere.transform.GetSiblingIndex() + 1).gameObject);
        Destroy(tiltSphere);

        //CHANGE VIEW
        //yield return new WaitUntil(() => !voiceoverAudio.isPlaying);
        //AdvanceVoiceovers();
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == false);
        AdvanceVoiceovers();
        print("Changed view, change view again");
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == true);
        print("Changed view again, listen to diver sounds.");

        //LIGHTS
        AdvanceVoiceovers();
        yield return new WaitUntil(() => player.GetLightsOn());
        print("Turned on all lights, listen to diver sounds");

        //FIND + RESCUE DIVER
        AdvanceVoiceovers();
        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);

        GameObject tutorialDiver = Instantiate(GetComponent<Player>().GetDiverPrefab(), transform.position - transform.right * 10, Quaternion.identity);
        AudioSource tutorialDiverAudio = tutorialDiver.GetComponent<AudioSource>();
        Diver tDiverComponent = tutorialDiver.GetComponent<Diver>();
        tDiverComponent.Init();
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.right * 50;
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position - transform.right * 100;
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 10;
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 50;
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
        yield return new WaitForSeconds(tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2);

        tutorialDiver.transform.position = transform.position + transform.forward * 100;
        tDiverComponent.UpdateSound();

        print(tutorialDiverAudio.clip.length + " " + tutorialDiverAudio.pitch + " " + (tutorialDiverAudio.clip.length / tutorialDiverAudio.pitch * 2));
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

        print("Listened to diver sounds, rescue diver");
        
        AdvanceVoiceovers();
        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);
        tutorialDiver = GameObject.Find("TestingDiver");
        if (tutorialDiver != null)
        {
            tutorialDiver.GetComponent<AudioSource>().volume = 1f;
            tutorialDiver.GetComponent<AudioSource>().Play();
        }
        yield return new WaitUntil(() => player.GetSavedDivers() == 1);
        print("Diver rescued, go dock");

        //RETURN TO BASE + DOCK
        AdvanceVoiceovers();
        yield return new WaitUntil(Finish);
        print("Docked, finish");
        AdvanceVoiceovers();

        yield return new WaitUntil(() => !voiceoverAudio.isPlaying);

        foreach (GameObject diver in divers)
        {
            if (diver == null) continue;
            diver.GetComponent<AudioSource>().volume = 1f;
            diver.GetComponent<AudioSource>().Play();
        }
    }

    private void AdvanceVoiceovers()
    {
        currentVoiceover++;
        voiceoverAudio.clip = voiceovers[currentVoiceover];
        voiceoverAudio.Play();
    }

    bool Finish()
    {
        print(Vector3.Distance(transform.position, startingPosition) + " " + (player.IsDocked() || Vector3.Distance(transform.position, startingPosition) <= 0.5f));
        if (player.IsDocked() || Vector3.Distance(transform.position, startingPosition) <= 0.5f) return true;
        else return false;
    }
}
