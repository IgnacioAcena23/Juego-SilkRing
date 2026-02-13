using UnityEngine;
using TMPro; // Importante para usar TextMeshPro

public class parpadeoTexto : MonoBehaviour
{
    public float velocidadParpadeo = 2.0f; // Qué tan rápido parpadea
    private TextMeshProUGUI texto;

    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Usamos una función matemática (Seno) para crear un ciclo suave de 0 a 1
        float alpha = (Mathf.Sin(Time.time * velocidadParpadeo) + 1.0f) / 2.0f;
        
        // Aplicamos ese valor al color del texto
        Color colorActual = texto.color;
        colorActual.a = alpha;
        texto.color = colorActual;
    }
}