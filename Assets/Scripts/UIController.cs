using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the UI menus.
 */
public class UIController : MonoBehaviour
{

    void Update()
    {
        // Pop up the menu or close it when user presses Escape key.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!transform.GetChild(0).gameObject.activeSelf) PauseGame();
            else ResumeGame();
        }
    }

    /*
     * Pause the game if in menu.
     */
    public void PauseGame()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /*
     * Resume the game if the menu was closed.
     */
    public void ResumeGame()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /*
     * Close the game.
     */
    public void ExitButton()
    {
        Application.Quit();
    }

    /*
     * Start from the beginning, loading the scene again.
     */
    public void RestartButton()
    {
        SceneManager.LoadScene(0);
    }
}
