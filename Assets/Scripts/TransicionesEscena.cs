using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionesEscena : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public Animator animatorCanvas; // Arrastra aquí el Canvas/Panel con el Animator
    [SerializeField] private string nombreTrigger = "Iniciar";
    //[SerializeField] private float tiempoDeEsperaManual = 1f; // Por si no tienes el AnimationClip a mano

    private bool cambiandoEscena = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Detectar al jugador
        if (collision.CompareTag("Player") && !cambiandoEscena)
        {
            cambiandoEscena = true;
            Debug.Log("Jugador detectado, iniciando transición...");
            StartCoroutine(CambiarEscena());
        }
    }

    IEnumerator CambiarEscena()
    {
        // 2. Ejecutar la animación en el Canvas
        if (animatorCanvas != null)
        {
            animatorCanvas.SetTrigger(nombreTrigger);
        }
        else
        {
            Debug.LogWarning("¡Ojo! No has arrastrado el Canvas al script en el Inspector.");
        }

        // 3. Esperar a que la pantalla se ponga negra
        yield return new WaitForSeconds(2f);

        // 4. Cambiar de nivel
        int siguienteEscena = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (siguienteEscena < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguienteEscena);
        }
        else
        {
            Debug.Log("Fin del juego o volviendo al inicio.");
            SceneManager.LoadScene(0);
        }
    }
}