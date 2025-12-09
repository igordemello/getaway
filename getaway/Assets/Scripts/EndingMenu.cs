using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingMenu : MonoBehaviour
{

    public void exit()
    {
        Application.Quit();
    }

    public void menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
