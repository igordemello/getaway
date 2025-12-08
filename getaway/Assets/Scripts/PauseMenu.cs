using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Referências")]
    public GameObject PausePanel;        
    public KeyCode[] pauseKeys = { KeyCode.Escape, KeyCode.Escape }; 

    private bool isPaused = false;

    void Start()
    {

        Time.timeScale = 1f;
        isPaused = false;

        if (PausePanel == null)
        {
            Debug.LogError("[PauseMenu] PausePanel NÃO atribuído no Inspector!");
        }
        else
        {
            PausePanel.SetActive(false); 
            Debug.Log("[PauseMenu] PausePanel desativado no Start.");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
 
        foreach (KeyCode k in pauseKeys)
        {
            if (Input.GetKeyDown(k))
            {
                Debug.Log($"[PauseMenu] Tecla {k} pressionada. isPaused = {isPaused}");
                TogglePause();
                return;
            }
        }
    }

    void TogglePause()
    {
        if (isPaused) Continue();
        else Pause();
    }

    public void Pause()
    {
        if (PausePanel == null)
        {
            Debug.LogError("[PauseMenu] Pause() chamado, mas PausePanel é null!");
            return;
        }


        PausePanel.SetActive(true);

 
        var cg = PausePanel.GetComponent<UnityEngine.CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }


        var canvas = PausePanel.GetComponentInParent<Canvas>();
        if (canvas != null && !canvas.gameObject.activeInHierarchy)
        {
            canvas.gameObject.SetActive(true);
            Debug.Log("[PauseMenu] Canvas do Pause foi ativado automaticamente.");
        }

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("[PauseMenu] Jogo PAUSADO. PausePanel ativado.");
    }

    public void Continue()
    {
        if (PausePanel != null) PausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("[PauseMenu] Jogo CONTINUANDO. PausePanel desativado.");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Debug.Log("[PauseMenu] Indo para MainMenu...");
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("[PauseMenu] Sair do jogo solicitado.");
        Application.Quit();
    }


    public void ForceOpen()
    {
        Debug.Log("[PauseMenu] ForceOpen() chamado.");
        Pause();
    }
}
