
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DreamTimer _dreamTimer;
    [SerializeField] private GameObject _defeatPanel;
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _pausePanel;

    [Header("Objetivo del nivel")]
    [SerializeField] private TMP_Text _objectiveText;
    [SerializeField] private int _floresNecesarias = 2;

    [Header("Salida")]
    [SerializeField] private GameObject _exitBarrier;

    [Header("Siguiente nivel")]
    [SerializeField] private string _nextSceneName;

    [Header("Sonidos")]
    [Tooltip("Suena al cruzar la salida con la flor (nivel completado).")]
    [SerializeField] private AudioClip _victorySound;
    [Tooltip("Suena al quedarse sin tiempo.")]
    [SerializeField] private AudioClip _defeatSound;
    [Tooltip("Suena al llegar a la salida sin la flor (salida cerrada).")]
    [SerializeField] private AudioClip _exitLockedSound;
    [SerializeField] private float _exitLockedCooldown = 2f;

    private float _nextExitLockedTime;

    private int _floresRecolectadas;
    private bool _isFinished;
    private bool _isPaused;

    public bool HasCollectedFlower =>
        _floresRecolectadas >= _floresNecesarias;

    private void Update()
    {
        if (_isFinished)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        _floresRecolectadas = 0;
        _isFinished = false;
        _isPaused = false;

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        // La salida comienza cerrada.
        if (_exitBarrier != null)
            _exitBarrier.SetActive(true);

        ActualizarObjetivo();
    }

    private void OnEnable()
    {
        if (_dreamTimer != null)
            _dreamTimer.OnDreamEnded += HandleDreamEnded;
    }

    private void OnDisable()
    {
        if (_dreamTimer != null)
            _dreamTimer.OnDreamEnded -= HandleDreamEnded;
    }

    private void ActualizarObjetivo()
    {
        if (_objectiveText == null)
            return;

        _objectiveText.text =
            $"Hongos del sueño: {_floresRecolectadas}/{_floresNecesarias}\n" +
            "Encontrá los hongos especiales.";
    }

    private void HandleDreamEnded()
    {
        if (_isFinished)
            return;

        _isFinished = true;
        _isPaused = false;
        Sfx.Play2D(_defeatSound);

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_defeatPanel != null)
            _defeatPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void TryCompleteLevel()
    {
        if (_isFinished)
            return;

        if (!HasCollectedFlower)
        {
            ActualizarObjetivo();

            // LevelExit llama a esto cada frame mientras estas en la zona: sonido con pausa
            if (Time.unscaledTime >= _nextExitLockedTime)
            {
                _nextExitLockedTime = Time.unscaledTime + _exitLockedCooldown;
                Sfx.Play2D(_exitLockedSound);
            }

            return;
        }

        _isFinished = true;
        _isPaused = false;
        Sfx.Play2D(_victorySound);

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_victoryPanel != null)
            _victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // Hongo común: no suma al objetivo.
    public void CollectCommonMushroom()
    {
        Debug.Log("Hongo común recogido. No suma al objetivo.");
    }

    // Hongo especial: suma al contador del nivel.
    public void CollectFlower()
    {
        if (_isFinished || _isPaused || HasCollectedFlower)
            return;

        _floresRecolectadas++;

        if (_floresRecolectadas >= _floresNecesarias)
        {
            _floresRecolectadas = _floresNecesarias;

            if (_exitBarrier != null)
                _exitBarrier.SetActive(false);

            if (_objectiveText != null)
            {
                _objectiveText.text =
                    "¡Objetivo completado!\nAhora buscá la salida.";
            }

            Debug.Log("¡Objetivo completado! La salida está habilitada.");
        }
        else
        {
            ActualizarObjetivo();
            Debug.Log($"Hongos especiales: {_floresRecolectadas}/{_floresNecesarias}");
        }
    }

    // =========================
    // PAUSA
    // =========================

    public void TogglePause()
    {
        if (_isFinished || _pausePanel == null)
            return;

        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (_isFinished || _isPaused || _pausePanel == null)
            return;

        _isPaused = true;
        Time.timeScale = 0f;
        _pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        Time.timeScale = 1f;

        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }

    // =========================
    // NAVEGACIÓN
    // =========================

    public void RetryLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_nextSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}
