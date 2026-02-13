using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject menuPrincipal;

    public void Jugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void AbrirOpciones()
    {
        panelOpciones.SetActive(true);
        menuPrincipal.SetActive(false); // Ocultamos el principal
    }

    public void Salir()
    {
        Application.Quit();
    }
}