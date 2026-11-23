using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player; 
    public float moveSpeed = 2.5f; 
    public float attackDistance = 1f; 
    
    // Variabel baru untuk sistem serangan
    public float attackCooldown = 1.5f; // Goblin akan memukul setiap 1.5 detik
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // Jika masih jauh, kejar
            if (distance > attackDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                movement = direction;
            }
            // Jika sudah cukup dekat, BERHENTI DAN SERANG!
            else
            {
                movement = Vector2.zero;
                
                // Cek apakah waktu jeda serangan sudah selesai
                if (Time.time >= nextAttackTime)
                {
                    // Tarik pelatuk animasi serang
                    anim.SetTrigger("Attack");
                    
                    // Setel ulang waktu tunggu untuk serangan berikutnya
                    nextAttackTime = Time.time + attackCooldown;
                }
            }

            // Perbarui arah hadap (Scale X) dan Animasi Walk/Idle
            if (movement.sqrMagnitude > 0)
            {
                anim.SetFloat("MoveX", Mathf.Abs(movement.x));
                anim.SetFloat("MoveY", movement.y);

                if (movement.x < 0) {
                    transform.localScale = new Vector3(-1, 1, 1); 
                } else if (movement.x > 0) {
                    transform.localScale = new Vector3(1, 1, 1);  
                }
            }

            anim.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        if (movement.sqrMagnitude > 0)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}