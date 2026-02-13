using UnityEngine;

public class AsignarCamaraCanvas : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        // Si el canvas usa cámara, buscamos la cámara principal de la escena actual
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
        }
    }
}