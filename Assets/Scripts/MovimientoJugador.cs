using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoJugador : MonoBehaviour
{
    private Animator animator;

    private Rigidbody2D rb2D;

    private combateJugador scriptCombate;

    [Header("Movimiento")]

    private float movimientoHorizontal = 0f;
    [Range(0,1000)][SerializeField] private float velocidadMovimiento;
    [Range(0,0.3f)][SerializeField] private float suavizadoDeMovimiento;
    private Vector3 velocidad = Vector3.zero;
    private bool mirandoDerecha = true;

    [Header("Salto")]

    [SerializeField] private float fuerzaDeSalto = 10f;
    [SerializeField] public float longitudRaycastSueloY = 0.2f;
    [SerializeField] public float longitudRaycastSueloX = 0.5f;
    [SerializeField] private LayerMask queEsSuelo;
    [SerializeField] private Transform controladorSuelo;
    [SerializeField] private Vector3 dimensionesCaja;
    [SerializeField] private bool enSuelo;

    [Header("Vida")]

    public int vida = 5;
    private bool recibeDamage;
    private float fRebote = 7f;

    [Header("Regeneración por Items")]
    public int vidaMaxima = 5;
    public int cantidadItemsVida = 3; 
    [Range(0, 1)] public float porcentajeRecuperacion = 0.15f;
    public bool estaRegenerando = false;
    public Animator animatorPocionUI; // Arrastra aquí la poción del Canvas
    private Coroutine corrutinaRegen;

    [Header("Rodado")]
    [SerializeField] private float fuerzaDeRodado = 12f;
    [SerializeField] private float duracionRodado = 1.03f;
    [SerializeField] private Vector2 tamañoRodado = new Vector2(1f, 0.5f); // Ajusta en el Inspector
    [SerializeField] private Vector2 offsetRodado = new Vector2(0f, -0.25f); // Ajusta en el Inspector
    private bool estaRodando = false;
    private BoxCollider2D boxCollider;
    private Vector2 tamañoOriginalCollider;
    private Vector2 offsetOriginalCollider;

    [Header("Easter Egg Inactividad")]
    [SerializeField] private float tiempoParaEasterEgg = 30f;
    private float cronometroInactividad = 0f;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vidaMaxima = vida;
        boxCollider = GetComponent<BoxCollider2D>();
        scriptCombate = GetComponent<combateJugador>();
        tamañoOriginalCollider = boxCollider.size;
        offsetOriginalCollider = boxCollider.offset;
    }

    private void Update()
    {
        float inputTeclado = 0;

        enSuelo = Grounded();

        // 1. DETECTAR INTENCIÓN DE MOVIMIENTO (Para el Easter Egg)
        bool intentandoMoverse = Keyboard.current.aKey.isPressed || 
                                Keyboard.current.dKey.isPressed || 
                                Keyboard.current.leftArrowKey.isPressed || 
                                Keyboard.current.rightArrowKey.isPressed ||
                                Keyboard.current.spaceKey.wasPressedThisFrame ||
                                Keyboard.current.leftCtrlKey.wasPressedThisFrame ||
                                Keyboard.current.fKey.wasPressedThisFrame ||
                                Mouse.current.leftButton.wasPressedThisFrame ||
                                Mouse.current.rightButton.wasPressedThisFrame;

    // 2. LÓGICA DEL CRONÓMETRO EASTER EGG
        if (!intentandoMoverse && enSuelo && !recibeDamage && !estaRegenerando && !estaRodando)
        {
            cronometroInactividad += Time.deltaTime;

            if (cronometroInactividad >= tiempoParaEasterEgg)
            {
                animator.SetTrigger("EasterEgg");
                cronometroInactividad = 0f; // Reiniciamos para el siguiente ciclo
            }
        }
        else
        {
            // Si hay cualquier input, reseteamos el tiempo a cero
            cronometroInactividad = 0f;
        }

        // 3. CAPTURAR INPUT HORIZONTAL
        if (!recibeDamage && !estaRodando && !estaRegenerando && (scriptCombate != null && !scriptCombate.EstaBloqueando()))
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) 
                inputTeclado = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) 
                inputTeclado = 1f;
        }
        else if (scriptCombate != null && scriptCombate.EstaBloqueando())
        {
            inputTeclado = 0f; // Fuerza la velocidad a 0 si bloquea
        }   

        movimientoHorizontal = inputTeclado * velocidadMovimiento;

        // 4. PASAR PARÁMETROS AL ANIMATOR
        // Usamos Abs(inputTeclado) para que la animación de correr sea binaria (0 o 1)
        animator.SetFloat("Horizontal", Mathf.Abs(inputTeclado));
        animator.SetBool("EnSuelo", enSuelo);
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("recibeDamage", recibeDamage);
        animator.SetBool("estaRegenerando", estaRegenerando);
        /*animator.SetBool("estaBloqueando", estaBloqueando);*/
    

        // 5. ACCIÓN: SALTO
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !recibeDamage && !estaRodando && !scriptCombate.EstaBloqueando())
        {
            IntentarSaltar();
        } 

        // 6. ACCIÓN: RODAR (Ctrl)
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && enSuelo && !estaRodando && !recibeDamage && !estaRegenerando) {
        // Si estamos bloqueando, forzamos la salida del bloqueo antes de rodar
        if (scriptCombate != null && scriptCombate.EstaBloqueando()){
            // Necesitas que el método TerminarBloqueo en combateJugador sea PUBLIC
            scriptCombate.TerminarBloqueo(); 
        }
        StartCoroutine(Rodar());
        }

        // 7. ACCIÓN: CURAR (F)
        if (Keyboard.current.fKey.wasPressedThisFrame && enSuelo && !estaRegenerando && !recibeDamage && !estaRodando)
        {
            if (cantidadItemsVida > 0 && vida < vidaMaxima)
            {
                EmpezarRegeneracion();
            }
        }

        // 8. CANCELAR CURACIÓN SI EL JUGADOR SE MUEVE
        if (estaRegenerando && (Mathf.Abs(inputTeclado) > 0 || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            DetenerRegeneracion();
        }

        // 9. Ataque
        if (Mouse.current.leftButton.wasPressedThisFrame /*&& !estaBloqueando*/ && !estaRodando && !recibeDamage && !estaRegenerando && enSuelo)
        {
            if (scriptCombate != null){
                scriptCombate.IntentarAtacar();
            }
        }     

        /*// 10. BLOQUEO
        if (Mouse.current.rightButton.wasPressedThisFrame && enSuelo && !estaRodando && !recibeDamage) {
            estaBloqueando = true;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            estaBloqueando = false;
        }*/
    }

    private void FixedUpdate()
    {
        enSuelo = Grounded();

        Mover(movimientoHorizontal * Time.fixedDeltaTime);

        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("EnSuelo", enSuelo);
    } 

    private void Mover(float mover)
    {
        Vector3 velocidadObjetivo = Vector3.zero;

        if (mover > 0 && !mirandoDerecha){
            Girar();
        }
        else if (mover < 0 && mirandoDerecha){
            Girar();
        }

        if(!recibeDamage){
            velocidadObjetivo = new Vector2(mover, rb2D.linearVelocity.y);
            rb2D.linearVelocity = Vector3.SmoothDamp(rb2D.linearVelocity, velocidadObjetivo, ref velocidad, suavizadoDeMovimiento);

            if (mover > 0 && !mirandoDerecha) Girar();
            else if (mover < 0 && mirandoDerecha) Girar();
        }
    }

    private void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(controladorSuelo.position, dimensionesCaja);
    }

    public void RecibeDamage(Vector2 direction, int cantDamage)
    {
        // 1. Salida de emergencia
        if (estaRodando || recibeDamage) return;

        // 2. Comprobar Bloqueo
        bool estaBloqueando = scriptCombate != null && scriptCombate.EstaBloqueando();

        if (estaBloqueando)
        {
            // Aplicar retroceso de escudo
            scriptCombate.RecibirGolpeBloqueando(direction);
        
            // Lógica de daño reducido (Daño * Porcentaje)
            // Si porcentajeDanioBloqueo es 0.5, esto es exactamente Daño / 2
            float danioCalculado = cantDamage * scriptCombate.porcentajeDanioBloqueo;
            cantDamage = Mathf.RoundToInt(danioCalculado);
        }
        else
        {
            // Solo si NO bloquea aplicamos el rebote fuerte y la animación de dolor
            Vector2 rebote = new Vector2(transform.position.x - direction.x, 1).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(rebote * fRebote, ForceMode2D.Impulse);

            animator.SetBool("recibeDamage", true);
        }

        // 3. Aplicar el daño final (reducido o normal)
        DetenerRegeneracion();
        vida -= cantDamage;
        recibeDamage = true; 

        // 4. Muerte o Recuperación
        if (vida <= 0)
        {
            vida = 0;
            StartCoroutine(EsperarParaMorir());
        }
        else
        {
            // Si cantDamage fue 0 (bloqueo perfecto), igual activamos el cooldown de daño 
            // para evitar que un enemigo quite 100 vidas por segundo
            CancelInvoke("DesactivateDamage");
            Invoke("DesactivateDamage", 0.5f);
        }  
    }

    public void DesactivateDamage()
    {
        recibeDamage = false;
        rb2D.linearVelocity = Vector2.zero;
        animator.SetBool("recibeDamage", false);
    }

    void EmpezarRegeneracion()
    {
        estaRegenerando = true;
        movimientoHorizontal = 0; // Detenemos el movimiento
        corrutinaRegen = StartCoroutine(RegenerarVida());
    }

    IEnumerator RegenerarVida()
    {   
        estaRegenerando = true;

        if (animatorPocionUI == null) 
        {
            GameObject objPocion = GameObject.Find("frascoVida"); 
            if (objPocion != null) animatorPocionUI = objPocion.GetComponent<Animator>();
        }

        if (animatorPocionUI != null) {
            animatorPocionUI.SetTrigger("animarCuracion"); 
        }

        float tiempoAnim = 0.8f; 
        yield return new WaitForSeconds(tiempoAnim);

        int saludARecuperar = Mathf.RoundToInt(vidaMaxima * porcentajeRecuperacion);
        vida = Mathf.Clamp(vida + saludARecuperar, 0, vidaMaxima);
        cantidadItemsVida--; 

        estaRegenerando = false;
    }

    void DetenerRegeneracion()
    {
        if (estaRegenerando)
        {
            estaRegenerando = false;
            if (corrutinaRegen != null) StopCoroutine(corrutinaRegen);
        
            if (animatorPocionUI != null) animatorPocionUI.Play("Idle"); 
        }
    }

    private IEnumerator Rodar()
    {
        if (!Grounded()) yield break;

        estaRodando = true;
        animator.SetTrigger("Rodar");

        AjustarColliderRodado(true);

        float direccionRodado = mirandoDerecha ? 1 : -1;
        rb2D.linearVelocity = new Vector2(direccionRodado * fuerzaDeRodado, rb2D.linearVelocity.y);

        yield return new WaitForSeconds(duracionRodado);

        AjustarColliderRodado(false);
        estaRodando = false;
    }

    private void AjustarColliderRodado(bool activado)
    {
        if (boxCollider == null) return;

        if (activado)
        {
            boxCollider.size = tamañoRodado;

            float bottomOriginal = offsetOriginalCollider.y - (tamañoOriginalCollider.y * 0.5f);
            float nuevoOffsetY = bottomOriginal + (tamañoRodado.y * 0.5f);

            boxCollider.offset = new Vector2(offsetRodado.x, nuevoOffsetY);
        }
        else
        {
            boxCollider.size = tamañoOriginalCollider;
            boxCollider.offset = offsetOriginalCollider;
        }
    }

    IEnumerator EsperarParaMorir()
    {
        // 1. Esperamos a que el Invoke de "DesactivateDamage" o el tiempo de dolor pase
        yield return new WaitForSeconds(0.5f);
        recibeDamage = false;
        animator.SetBool("recibeDamage", false);
        // 3. Ahora sí, lanzamos la animación de muerte
        animator.SetTrigger("Muerte");
    
        if (GameManager.Instance != null){
            GameManager.Instance.gameOver();
        }
    
        // Desactivamos el script para no movernos mientras caemos
        this.enabled = false;
    }

    private bool EstaEnSuelo()
    {
        if (controladorSuelo == null) return false;
        return Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, queEsSuelo);
    }
    public bool Grounded()
    {
     if (Physics2D.Raycast(controladorSuelo.position, Vector2.down, longitudRaycastSueloY, queEsSuelo)|| 
         Physics2D.Raycast(controladorSuelo.position+ new Vector3(longitudRaycastSueloX,0,0), Vector2.down, longitudRaycastSueloY, queEsSuelo)||
            Physics2D.Raycast(controladorSuelo.position- new Vector3(-longitudRaycastSueloX,0,0), Vector2.down, longitudRaycastSueloY, queEsSuelo)
     )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void IntentarSaltar()
    {
        if (!enSuelo) return;

        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
        rb2D.AddForce(Vector2.up * fuerzaDeSalto, ForceMode2D.Impulse);

        enSuelo = false;
        animator.SetBool("EnSuelo", false);
        animator.SetFloat("VerticalVelocity", fuerzaDeSalto);
    }
}