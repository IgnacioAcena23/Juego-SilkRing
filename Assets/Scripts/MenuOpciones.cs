using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    public GameObject panelOpciones;
    public GameObject menuPrincipal;

    public void PantallaCompleta(bool esPantallaCompleta)
    {
        Screen.fullScreen = esPantallaCompleta;
    }

    public void CambiarVolumen(float volumen)
    {
        audioMixer.SetFloat("Volumen", volumen);
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false); // Oculta el menú
        menuPrincipal.SetActive(true); // Muestra el menú principal
    }
}