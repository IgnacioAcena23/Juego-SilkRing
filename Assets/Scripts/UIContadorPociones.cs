using UnityEngine;
using TMPro; // Importante para usar TextMeshPro

public class UIContadorPociones : MonoBehaviour
{
    public TextMeshProUGUI texto; // Arrastra aquí el componente de texto
    public MovimientoJugador jugador; // Arrastra aquí a tu Jugador (Hornte2)

    void Update()
    {
        if (jugador != null && texto != null)
        {
            // Actualiza el texto para que diga "x" y el número de pociones
            texto.text = "x" + jugador.cantidadItemsVida.ToString();
        }
    }
}