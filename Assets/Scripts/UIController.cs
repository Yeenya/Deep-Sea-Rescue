using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!transform.GetChild(0).gameObject.activeSelf) PauseGame();
            else ResumeGame();
        }
    }

    public void PauseGame()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(0);
    }
}
