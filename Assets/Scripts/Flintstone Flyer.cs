using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flintstone_Flyer : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb2D;
    private Animator animator;
    public GameObject paredDerecha;

    [Header("Movimiento")]
    public float detectionRadius = 8f;
    public float attackRadius = 2.5f;
    public float moveSpeed = 3f;
    [SerializeField] private float escalaBase = 0.75f;
    private Vector2 direction;
    private bool estaMuerto = false;
    private bool estaAtacando = false;

    [Header("Configuración de Ataques (AJUSTAR AQUÍ)")]
    public Transform controladorAtaque;
    public Transform controladorSlash;     // Arrastra aquí un nuevo objeto vacío
    public Transform controladorEstocada;  // Arrastra aquí otro objeto vacío
    
    [Space(5)]
    [Header("Ataque Slash")]
    public float radioSlash = 1.2f;
    public int danioSlash = 1;
    
    [Header("Ataque Estocada")]
    public Vector2 tamanoEstocada = new Vector2(2f, 1f);
    public int danioEstocada = 2;
    public float fuerzaEstocada = 10f;

    [Header("Ataque Waterfowl")]
    public float radioWaterfowl = 2.5f;
    public int danioWaterfowl = 1;

    [Header("Visualización")]
    [SerializeField] private float radioHitboxVisual = 1.2f;
    private Color colorHitbox = Color.green; // Color por defecto

    [Header("Vida")]
    public float vidaActual;
    public float vidaMaxima = 30f;
    
    [Header("Ajustes de IA")]
    public float tiempoEntreAtaques = 1.5f; 
    private float cronometroAtaque;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        if (estaMuerto) return;

        if (cronometroAtaque > 0) cronometroAtaque -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!estaAtacando)
        {
            ManejarSeguimiento(distanceToPlayer);
        }
    }

    // 1. Añade este método justo debajo del Update
    void FixedUpdate()
    {
        if (estaMuerto) return;

        // Si el animator tiene "enMovimiento" en true y NO estamos atacando
        if (animator.GetBool("enMovimiento") && !estaAtacando)
        {
            // Movimiento físico mediante velocidad directa
            rb2D.linearVelocity = new Vector2(direction.x * moveSpeed, rb2D.linearVelocity.y);
        }
        else if (!estaAtacando)
        {
            // Si no se mueve y no ataca, frenamos la velocidad horizontal
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        }
    }

    // 2. Revisa que ManejarSeguimiento esté asignando la dirección correctamente
    private void ManejarSeguimiento(float distancia)
    {
        if (distancia > detectionRadius)
        {
            direction = Vector2.zero;
            animator.SetBool("enMovimiento", false);
        }
        else if (distancia <= attackRadius)
        {
            direction = Vector2.zero;
            animator.SetBool("enMovimiento", false);
        
            if (cronometroAtaque <= 0)
            {
            DecidirAtaque();
            
            }
        }
        else // RANGO DE PERSECUCIÓN
        {
            // Calculamos la dirección hacia el jugador
            direction = (player.position - transform.position).normalized;
        
            animator.SetBool("enMovimiento", true);

            // Giro de Sprite
            transform.localScale = new Vector3(direction.x > 0 ? escalaBase : -escalaBase, escalaBase, escalaBase);
        }
    }

    private void DecidirAtaque()
    {
        estaAtacando = true;
        cronometroAtaque = tiempoEntreAtaques;
        animator.SetBool("enMovimiento", false);
        rb2D.linearVelocity = Vector2.zero;

        int randomAttack = Random.Range(0, 3); 
        if (randomAttack == 0) animator.SetTrigger("ataqueSlash");
        else if (randomAttack == 1) animator.SetTrigger("ataqueEstocada");
        else animator.SetTrigger("ataqueWaterfowl");
    }

    // --- EVENTOS DE ANIMACIÓN ---

    public void EventoImpulsoEstocada()
    {
        float mirando = transform.localScale.x > 0 ? 1 : -1;
        rb2D.AddForce(new Vector2(mirando * fuerzaEstocada, 0), ForceMode2D.Impulse);
    }

    // AHORA ESTE MÉTODO ES MÁS FÁCIL DE USAR
    // Solo escribe "slash", "estocada" o "waterfowl" en el String del evento
    public void CheckHitbox(string tipoAtaque)
    {
        float radio = 0;
        int danio = 0;
        Transform puntoDeOrigen = null;
        bool esCaja = false; // Nueva bandera para saber qué forma usar

        switch (tipoAtaque.ToLower())
        {
            case "slash":
                radio = radioSlash;
                danio = danioSlash;
                puntoDeOrigen = controladorSlash;
                colorHitbox = Color.green;
                break;
            case "estocada":
                danio = danioEstocada;
                puntoDeOrigen = controladorEstocada;
                colorHitbox = new Color(1f, 0.5f, 0f);
                esCaja = true; // <--- Activamos el modo cuadrado
                break;
            case "waterfowl":
                radio = radioWaterfowl;
                danio = danioWaterfowl;
                puntoDeOrigen = controladorAtaque;
                colorHitbox = Color.magenta;
                break;
        }   

        if (puntoDeOrigen == null) return;

        Collider2D[] objetos;

        if (esCaja)
        {
            // Detección cuadrada/rectangular
            objetos = Physics2D.OverlapBoxAll(puntoDeOrigen.position, tamanoEstocada, 0f);
        }
        else
        {
            // Detección circular (Slash y Waterfowl)
            objetos = Physics2D.OverlapCircleAll(puntoDeOrigen.position, radio);
        }

        foreach (Collider2D col in objetos)
        {
            if (col.CompareTag("Player"))
            {
                col.GetComponent<MovimientoJugador>().RecibeDamage(transform.position, danio);
            }
        }
    }

    public void FinAtaque()
    {
        estaAtacando = false;
        radioHitboxVisual = 1.2f;
    }

    public void TomarDano(float cantidad)
    {
        if (estaMuerto) return;
        vidaActual -= cantidad;
        if (vidaActual <= 0) Muerte();
    }

    private void Muerte()
    {
        estaMuerto = true;
        animator.SetTrigger("muerte");
        rb2D.simulated = false;

        if (paredDerecha != null) 
        {
            Destroy(paredDerecha);
        }

        Destroy(gameObject, 6f);
    }

    private void OnDrawGizmos()
    {
        // Slash (Verde)
        if (controladorSlash != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(controladorSlash.position, radioSlash);
        }

        // Estocada (Naranja)
        if (controladorEstocada != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            // Dibujamos un cubo de alambre (WireCube) usando el tamaño definido
            Gizmos.DrawWireCube(controladorEstocada.position, new Vector3(tamanoEstocada.x, tamanoEstocada.y, 1));
        }

        // Waterfowl (Magenta)
        if (controladorAtaque != null) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(controladorAtaque.position, radioWaterfowl);
        }

        // Rangos IA
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}