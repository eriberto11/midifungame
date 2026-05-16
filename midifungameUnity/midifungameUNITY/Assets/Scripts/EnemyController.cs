using UnityEngine;
using UnityEngine.UI;

// Grundläggande fiende: rör sig mot spelaren, attackerar med jämna mellanrum.
// Tar skada från spelarens träffar och dör när hälsan tar slut.
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 80f;
    public float moveSpeed = 1.2f;
    public float attackRange = 1.5f;
    public float attackInterval = 2f;

    [Header("Poäng")]
    public int killScore = 500;

    [Header("Referenser")]
    public Slider healthBar;

    float currentHealth;
    float attackTimer;
    Animator anim;
    Transform player;
    bool isDead;
    bool facingRight;

    void Awake()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) player = pc.transform;
        if (healthBar != null) healthBar.value = 1f;
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > attackRange)
            MoveTowards();
        else
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                PerformAttack();
            }
        }
    }

    void MoveTowards()
    {
        float dir = player.position.x - transform.position.x;
        transform.position += new Vector3(Mathf.Sign(dir) * moveSpeed * Time.deltaTime, 0f, 0f);

        bool shouldFaceRight = dir > 0f;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Vector3 scale = transform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (anim != null) anim.SetBool("Walking", true);
    }

    void PerformAttack()
    {
        if (anim != null)
        {
            anim.SetBool("Walking", false);
            anim.SetTrigger("Attack");
        }
        // Fiendens attack registreras som en miss för spelaren (tar skada)
        if (GameManager.Instance != null) GameManager.Instance.RegisterMiss();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        UpdateHealthBar();

        if (anim != null) anim.SetTrigger("Hit");

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        isDead = true;
        if (anim != null)
        {
            anim.SetBool("Walking", false);
            anim.SetTrigger("Die");
        }

        // Bonuspoäng för att döda fiende
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < killScore / GameManager.Instance.scorePerHit; i++)
                GameManager.Instance.RegisterHit();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
    }
}
