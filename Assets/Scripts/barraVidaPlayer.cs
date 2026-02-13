using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class barraVidaPlayer : MonoBehaviour
{
    public Image rellenoBarraVida;
    public MovimientoJugador livePlayer;
    private float vidaMaximaAux;

    void Start()
    {
        if (livePlayer == null)
        {
            // Busca al jugador por nombre si no lo asignaste en el inspector
            GameObject obj = GameObject.Find("Hornte2");
            if (obj != null) livePlayer = obj.GetComponent<MovimientoJugador>();
        }

        if (livePlayer != null) vidaMaximaAux = livePlayer.vidaMaxima;
    }

    void Update()
    {
        // Verificamos que todo exista para evitar el NullReferenceException
        if (livePlayer != null && rellenoBarraVida != null && vidaMaximaAux > 0)
        {
            // Calculamos el relleno basándonos en la vida actual del script del jugador
            rellenoBarraVida.fillAmount = (float)livePlayer.vida / vidaMaximaAux;
        }
    }
}
