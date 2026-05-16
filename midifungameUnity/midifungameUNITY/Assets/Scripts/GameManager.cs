using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spelinställningar")]
    public int maxHealth = 100;
    public int scorePerHit = 100;
    public int penaltyPerMiss = 25;
    public float healthGainPerHit = 5f;
    public float healthLossPerMiss = 10f;

    public int Score { get; private set; }
    public float Health { get; private set; }
    public bool IsGameOver { get; private set; }

    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<float> OnHealthChanged;
    public static event System.Action OnHit;
    public static event System.Action OnMiss;
    public static event System.Action OnGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Health = maxHealth;
    }

    public void RegisterHit()
    {
        if (IsGameOver) return;
        Score += scorePerHit;
        Health = Mathf.Min(maxHealth, Health + healthGainPerHit);
        FireEvent(OnScoreChanged, Score);
        FireEvent(OnHealthChanged, Health);
        if (OnHit != null) OnHit();
    }

    public void RegisterMiss()
    {
        if (IsGameOver) return;
        Score = Mathf.Max(0, Score - penaltyPerMiss);
        Health -= healthLossPerMiss;
        FireEvent(OnScoreChanged, Score);
        FireEvent(OnHealthChanged, Health);
        if (OnMiss != null) OnMiss();
        if (Health <= 0f) TriggerGameOver();
    }

    public void RestartGame()
    {
        Score = 0;
        Health = maxHealth;
        IsGameOver = false;
        FireEvent(OnScoreChanged, Score);
        FireEvent(OnHealthChanged, Health);
    }

    void TriggerGameOver()
    {
        Health = 0f;
        IsGameOver = true;
        if (OnGameOver != null) OnGameOver();
        Debug.Log("Game Over! Slutpoäng: " + Score);
    }

    static void FireEvent(System.Action<int> ev, int val)
    {
        if (ev != null) ev(val);
    }

    static void FireEvent(System.Action<float> ev, float val)
    {
        if (ev != null) ev(val);
    }
}
