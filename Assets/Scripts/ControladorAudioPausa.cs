using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ControladorAudioPausa : MonoBehaviour
{
    [Header("UI Paneles (Solo para Escena Combate)")]
    public GameObject panelPausa;
    public GameObject objetoVolumen;

    [Header("Configuración Audio")]
    public AudioMixer miMixer;
    public Slider sliderVolumen;
    private const string KEY_VOLUMEN = "volumenGuardado"; // Clave única para PlayerPrefs
    private const string MIXER_PARAM = "Volumen";      // Nombre en el Audio Mixer

    private bool estaPausado = false;

    void Start()
    {
        // 1. Cargar valor guardado
        float volGuardado = PlayerPrefs.GetFloat(KEY_VOLUMEN, 0.75f);
        
        // 2. Sincronizar Slider y Mixer
        if (sliderVolumen != null)
        {
            sliderVolumen.value = volGuardado;
            // Suscribirse al evento por código para evitar olvidos en el inspector
            sliderVolumen.onValueChanged.AddListener(SetVolume);
        }
        
        SetVolume(volGuardado);
    }

    void Update()
{
    // Verificamos si existe el teclado y si la tecla Escape se presionó JUSTO en este frame
    if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
    {
        TogglePausa();
    }
}

// Nueva función unificada para evitar errores de lógica
public void TogglePausa()
{
    estaPausado = !estaPausado; // Invierte el estado (si es true pasa a false y viceversa)

    if (estaPausado)
    {
        Pausar();
    }
    else
    {
        Reanudar();
    }
}

    // --- LÓGICA DE VOLUMEN UNIFICADA ---
    public void SetVolume(float value)
    {
        // Guardar valor
        PlayerPrefs.SetFloat(KEY_VOLUMEN, value);

        // Aplicar al Mixer
        float dB = (value > 0) ? Mathf.Log10(value) * 20 : -80f;
        miMixer.SetFloat(MIXER_PARAM, dB);

        // Aplicar al AudioListener (opcional, como respaldo)
        AudioListener.volume = value;
    }

    // --- LÓGICA DE NAVEGACIÓN Y PAUSA ---
    public void Pausar()
    {
        estaPausado = true;
        Time.timeScale = 0f;
        if (panelPausa != null) panelPausa.SetActive(true);
    }

    public void Reanudar()
    {
        estaPausado = false;
        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    public void MostrarOcultarVolumen()
    {
        if (objetoVolumen != null)
            objetoVolumen.SetActive(!objetoVolumen.activeSelf);
    }

    public void IrAlMenu(string nombreEscena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
}