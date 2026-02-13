using UnityEngine;
using UnityEngine.SceneManagement; // Indispensable

public class PortalSiguienteNivel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si es el jugador mediante el Tag
        if (collision.CompareTag("Player"))
        {
            // 1. Obtenemos el índice de la escena que está abierta ahora
            int indiceEscenaActual = SceneManager.GetActiveScene().buildIndex;

            // 2. Cargamos la siguiente escena (actual + 1)
            // Nota: Asegúrate de que las escenas estén en orden en Build Settings
            SceneManager.LoadScene(indiceEscenaActual + 1);
        }

        int siguienteEscena = SceneManager.GetActiveScene().buildIndex + 1;

        // Si la siguiente escena existe en la lista de Build Settings
        if (siguienteEscena < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguienteEscena);
        }
        else
        {
            SceneManager.LoadScene(0); // Carga la primera escena (normalmente el menú)
        }
    }
}