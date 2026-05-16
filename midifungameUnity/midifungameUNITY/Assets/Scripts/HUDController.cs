using UnityEngine;
using UnityEngine.UI;

// Hanterar spelets HUD: poäng, hälsostapel, combo och game over-skärm.
public class HUDController : MonoBehaviour
{
    [Header("UI-element")]
    public Text scoreText;
    public Slider healthSlider;
    public Text comboText;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text hitFeedbackText;

    int combo;
    float feedbackTimer;

    void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnHealthChanged += UpdateHealth;
        GameManager.OnHit += OnHit;
        GameManager.OnMiss += OnMiss;
        GameManager.OnGameOver += ShowGameOver;
    }

    void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnHealthChanged -= UpdateHealth;
        GameManager.OnHit -= OnHit;
        GameManager.OnMiss -= OnMiss;
        GameManager.OnGameOver -= ShowGameOver;
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hitFeedbackText != null) hitFeedbackText.gameObject.SetActive(false);
        UpdateScore(0);
        UpdateHealth(GameManager.Instance != null ? GameManager.Instance.maxHealth : 100);
    }

    void Update()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f && hitFeedbackText != null)
                hitFeedbackText.gameObject.SetActive(false);
        }
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Poäng: " + score;
    }

    void UpdateHealth(float health)
    {
        if (healthSlider != null && GameManager.Instance != null)
            healthSlider.value = health / GameManager.Instance.maxHealth;
    }

    void OnHit()
    {
        combo++;
        ShowFeedback(combo > 1 ? "TRÄFF! x" + combo : "TRÄFF!", Color.green);
        if (comboText != null)
            comboText.text = combo > 1 ? "Combo x" + combo : "";
    }

    void OnMiss()
    {
        combo = 0;
        ShowFeedback("MISS", Color.red);
        if (comboText != null) comboText.text = "";
    }

    void ShowFeedback(string message, Color color)
    {
        if (hitFeedbackText == null) return;
        hitFeedbackText.text = message;
        hitFeedbackText.color = color;
        hitFeedbackText.gameObject.SetActive(true);
        feedbackTimer = 0.6f;
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = "Slutpoäng: " + GameManager.Instance.Score;
    }
}
