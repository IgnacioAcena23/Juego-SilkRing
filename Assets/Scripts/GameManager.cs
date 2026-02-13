using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button menuButton;

    public Image filtroNegro;
    [SerializeField] public float velocidadOscurecimiento = 0.5f;
    private CanvasGroup gameOverCanvasGroup;
    private bool gameOverActivo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverScreen != null){
            gameOverScreen.SetActive(false);
        }
        if (restartButton != null){
            restartButton.onClick.AddListener(reiniciarEscena);
        }
        /*if (menuButton != null)
        {
            menuButton.onClick.AddListener(irAlMenu);
        }*/

        if (gameOverScreen != null){
            // Intentamos obtenerlo
            gameOverCanvasGroup = gameOverScreen.GetComponent<CanvasGroup>();

            // Si NO existe, lo creamos nosotros mismos por código para que no de error
            if (gameOverCanvasGroup == null) {
                gameOverCanvasGroup = gameOverScreen.AddComponent<CanvasGroup>();
            }

            gameOverScreen.SetActive(false);
            gameOverCanvasGroup.alpha = 0;
        }
    }

    void Update()
    {
        if (gameOverActivo){
            if (Keyboard.current.anyKey.wasPressedThisFrame){
                reiniciarEscena();
            }
        }
        
        /*if (Keyboard.current.escapelKey.wasPressedThisFrame){
            irAlMenu();
        }*/
    }

    public void gameOver()
    {
        if (gameOverActivo) return;
        gameOverActivo = true;

        if (gameOverScreen != null){
            StartCoroutine(SecuenciaMuerte());
        }

        if (gameOverScreen != null){
            gameOverScreen.SetActive(false);
        }
        if (gameOverText != null){
            gameOverText.text = "\nPULSA CUALQUIER TECLA PARA CONTINUAR";
        }
    }

    public void reiniciarEscena()
    {
        Time.timeScale = 1f;
        //Cambiar al Nombre de la escena a la que quiera ir
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /*public void irAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Nombnre de la escena del menú");
    }*/

    IEnumerator SecuenciaMuerte()
    {
        // PASO 1: Muerte y animación completa (Ya lanzada desde MovimientoJugador)
        // Esperamos los 3 segundos que mencionas
        yield return new WaitForSeconds(3.0f); 

        // PASO 2: Oscurecimiento para hacer transición
        if (filtroNegro != null) {
            // Asegúrate de que el objeto de la imagen esté activo, pero transparente
            filtroNegro.gameObject.SetActive(true); 
        
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * velocidadOscurecimiento;
                Color c = filtroNegro.color;
                c.a = alpha;
                filtroNegro.color = c;
                yield return null; // Espera al siguiente frame
            }
        }

        Color finalColor = filtroNegro.color;
        finalColor.a = 1;
        filtroNegro.color = finalColor;

    // PASO 3: Imagen de game over con el texto parpadeante
    // Solo activamos esto cuando el alpha ya llegó a 1 (pantalla totalmente negra)
        if (gameOverScreen != null) {
            gameOverScreen.SetActive(true);

            if (gameOverText != null){
                float tUI = 0;
                while (tUI < 1)
                {
                    tUI += Time.deltaTime * 1.5f; // Velocidad de aparición del menú
                    gameOverCanvasGroup.alpha = tUI;
                    yield return null;
                }
                gameOverCanvasGroup.alpha = 1; // Menú totalmente visible
            }
        }   
    }
}
