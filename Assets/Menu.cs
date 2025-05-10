using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    public GameObject Instrucoes;
    public void MudarCena(){
        SceneManager.LoadScene("jogo");
    }

    public void SairJogo()
    {
        Debug.Log("Sair do jogo...");
        Application.Quit();
    }

    public void MostraInstrucoes(){
        Instrucoes.SetActive(true);
    }
   
}
