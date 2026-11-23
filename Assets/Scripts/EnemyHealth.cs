using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Efek Terpukul")]
    public float knockbackForce = 5f; 
    public float stunDuration = 0.2f; 
    
    private Color originalColor;
    
    private Animator anim;
    private EnemyAI enemyAI;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRend;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        
        originalColor = spriteRend.color; 
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        if (currentHealth > 0)
        {
            StartCoroutine(DamageEffect(attacker));
        }
        else
        {
            Die();
        }
    }

    IEnumerator DamageEffect(Transform attacker)
    {
        // Matikan AI dan ubah warna jadi merah
        if (enemyAI != null) enemyAI.enabled = false;
        spriteRend.color = Color.red;

        // Hitung arah mundur menjauhi Raka lalu dorong (Knockback)
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Tunggu sebentar (Stun)
        yield return new WaitForSeconds(stunDuration);

        // Kembalikan warna dan nyalakan AI lagi
        spriteRend.color = originalColor;
        rb.linearVelocity = Vector2.zero;
        if (enemyAI != null && currentHealth > 0) enemyAI.enabled = true;
    }

    void Die()
    {
        anim.SetTrigger("Die");

        if (enemyAI != null) enemyAI.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero; 
        
        this.enabled = false;
        Destroy(gameObject, 1f); 
    }
}