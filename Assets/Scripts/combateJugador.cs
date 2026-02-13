using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class combateJugador : MonoBehaviour
{
    [Header("Configuración de Ataque")]

    [Tooltip("Transform que define la posición y rotación del golpe.")]
    public Transform controladorGolpe;
    [Tooltip("Tamaño del lado del cubo de golpe (radio * 2).")]
    [Min(0f)] public float radioGolpe = 0.5f;
    [Tooltip("Daño infligido por cada golpe.")]
    [Min(0f)] public float damageGolpe = 1f;
    [Tooltip("Tiempo mínimo entre golpes consecutivos.")]
    [Min(0f)] public float tiempoEntreGolpes = 0.25f;
    [Tooltip("Offset local del cubo de golpe respecto al controlador.")]
    public Vector2 offsetGolpe = Vector2.zero;

    [Header("Estado interno")]
    [SerializeField, Tooltip("Tiempo restante para habilitar el siguiente ataque.")]
    private float siguienteAtaque;

    [Header("Referencias")]
    public Animator animator;
    private Rigidbody2D rb2D;
    private MovimientoJugador movJugador;

    [Header("Configuración de Bloqueo")]
    [SerializeField] private float cooldownBloqueo = 0.5f;
    [SerializeField] private float fuerzaRetrocesoBloqueo = 5f;
    private float tiempoSiguienteBloqueo;
    private bool estaBloqueando = false;
    [Range(0f, 1f)] [SerializeField] public float porcentajeDanioBloqueo = 0.5f;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        movJugador = GetComponent<MovimientoJugador>();
    }

    void Update()
    {
        if (siguienteAtaque > 0){
            siguienteAtaque -= Time.deltaTime;
        }
        if (tiempoSiguienteBloqueo > 0) {
            tiempoSiguienteBloqueo -= Time.deltaTime;
        }  
        ManejarEntradaBloqueo();
    }

    public void IntentarAtacar()
    {
        if (siguienteAtaque <= 0 && !estaBloqueando)
        {
            animator.SetTrigger("golpe");
            siguienteAtaque = tiempoEntreGolpes;
        }
    }

    public void Golpear ()
    {
        Vector2 tamanoGolpe = new Vector2(radioGolpe * 2f, radioGolpe * 2f);
        Vector2 centroGolpe = controladorGolpe.TransformPoint(offsetGolpe);
        Collider2D[] enemigosGolpeados = Physics2D.OverlapBoxAll(
            centroGolpe,
            tamanoGolpe,
            controladorGolpe.eulerAngles.z
        );

        foreach (Collider2D colisionador in enemigosGolpeados){
            if (colisionador.CompareTag("Enemigo")){
                var enemigo = colisionador.GetComponent<Flintstone_Flyer>();

                if (enemigo != null){
                    enemigo.TomarDano(damageGolpe);
                }
            }
        }
    }

    void OnDrawGizmos ()
    {
        if (controladorGolpe == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(controladorGolpe.position, controladorGolpe.rotation, Vector3.one);
        Gizmos.DrawWireCube((Vector3)offsetGolpe, new Vector3(radioGolpe * 2f, radioGolpe * 2f, 0f));
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    public void ManejarEntradaBloqueo()
    {
        // Solo puede bloquear si está en suelo y no está recibiendo daño o rodando
        bool puedeBloquear = movJugador.Grounded() && tiempoSiguienteBloqueo <= 0;

        if (Mouse.current.rightButton.wasPressedThisFrame && puedeBloquear)
        {
            EmpezarBloqueo();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame && estaBloqueando)
        {
            TerminarBloqueo();
        }
    }

    public void EmpezarBloqueo()
    {
        estaBloqueando = true;
        animator.SetTrigger("startBlock"); // WylderBlock2
        animator.SetBool("isBlocking", true); // Para WylderHoldBlock (Loop)
    }

    public void TerminarBloqueo()
    {
        estaBloqueando = false;
        animator.SetBool("isBlocking", false);
        animator.SetTrigger("unBlock"); // WylderUnBlock
        tiempoSiguienteBloqueo = cooldownBloqueo;
    }

    public void RecibirGolpeBloqueando(Vector2 posicionAtacante)
    {
        // Retroceso pequeño al bloquear un golpe
        Vector2 direccionRetroceso = new Vector2(transform.position.x - posicionAtacante.x, 0).normalized;
        rb2D.AddForce(direccionRetroceso * fuerzaRetrocesoBloqueo, ForceMode2D.Impulse);
        
        // Opcional: Trigger de animación de impacto en escudo si tienes una
        /*animator.SetTrigger("blockImpact"); */
    }

    public bool EstaBloqueando() => estaBloqueando;
}