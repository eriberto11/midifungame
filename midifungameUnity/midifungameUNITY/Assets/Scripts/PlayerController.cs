using UnityEngine;
using MidiJack;

// Styr spelfiguren. MIDI-noter mappas till rörelse och attack.
// Rörelse sker via tangentbord (WASD/pilar) plus MIDI CC-1 (pitch wheel / modwheel).
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Rörelse")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public LayerMask enemyLayer;

    [Header("MIDI-rörelse")]
    // CC-nummer vars värde styr horisontell rörelse (0-63 = vänster, 64-127 = höger)
    public int movementCCNumber = 1;

    Rigidbody2D rb;
    Animator anim;
    bool isGrounded;
    bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        GameManager.OnHit += OnHit;
        GameManager.OnMiss += OnMiss;
        GameManager.OnGameOver += OnGameOver;
        MidiMaster.knobDelegate += OnKnob;
    }

    void OnDisable()
    {
        GameManager.OnHit -= OnHit;
        GameManager.OnMiss -= OnMiss;
        GameManager.OnGameOver -= OnGameOver;
        MidiMaster.knobDelegate -= OnKnob;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        Move(horizontal);

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    void Move(float direction)
    {
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (direction > 0f && !facingRight) Flip();
        else if (direction < 0f && facingRight) Flip();
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            if (anim != null) anim.SetTrigger("Jump");
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (attackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D col in hits)
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null) enemy.TakeDamage(25f);
        }
    }

    // MIDI CC styr rörelse: 0 = max vänster, 63 = stilla, 127 = max höger
    void OnKnob(MidiChannel channel, int knobNumber, float value)
    {
        if (knobNumber != movementCCNumber) return;
        float direction = (value - 0.5f) * 2f; // Normalisera till -1..1
        if (Mathf.Abs(direction) < 0.1f) direction = 0f;
        Move(direction);
    }

    void OnHit()
    {
        Attack();
    }

    void OnMiss()
    {
        if (anim != null) anim.SetTrigger("Hurt");
    }

    void OnGameOver()
    {
        if (anim != null) anim.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        enabled = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
