using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionsMenu : MonoBehaviour
{
    public RectTransform iniciar;
    public GameObject imagem;
    public GameObject canvas1;
    public GameObject canvas2;     
    public bool mover = false;

    public void Update()
    {
        print("aaaa");
        if (mover)
        {
            imagem.transform.position -= new Vector3(0, 3, 0);
            iniciar.transform.position -= new Vector3(0, 3, 0);
            print("aaaa");
            if (iniciar.transform.position.y <= 20)
            {
                mover = false;
            }
        }
    }

    public void escolha_missao()
    {
        canvas1.SetActive(false);
        print("aqfegeg");
        mover = true;
    }
    public void iniciar_missao()
    {
        SceneManager.LoadScene("SampleScene");
    }
}