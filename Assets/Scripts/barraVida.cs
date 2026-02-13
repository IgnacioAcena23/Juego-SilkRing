using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class barraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    [SerializeField] private Flintstone_Flyer liveEnemy;

    void Start()
    {
        if (liveEnemy == null)
        {
            // Busca el objeto que tiene el script del Boss
            liveEnemy = Object.FindFirstObjectByType<Flintstone_Flyer>();
        }
    }

    void Update()
    {
        if (liveEnemy != null)
        {
            // Calculamos el porcentaje (0.0 a 1.0)
            float porcentaje = liveEnemy.vidaActual / liveEnemy.vidaMaxima;
            
            // Actualizamos el relleno visual
            rellenoBarraVida.fillAmount = Mathf.Clamp01(porcentaje);

            // Si el Boss muere, podemos ocultar la barra entera
            if (liveEnemy.vidaActual <= 0)
            {
                // Opcional: Desactivar el objeto tras un par de segundos
                Invoke("OcultarBarra", 2f);
            }
        }
    }

    void OcultarBarra()
    {
        gameObject.SetActive(false);
    }
}