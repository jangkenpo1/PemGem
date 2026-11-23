using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    [Header("Pengaturan Serangan")]
    public float attackRange = 0.8f; 
    public float attackOffset = 0.5f; 
    public LayerMask enemyLayers; 
    
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;
    private Vector2 lastFaceDir = new Vector2(0, -1); 
    
    private InputAction moveAction;
    private InputAction attackAction;

    // --- VARIABEL BARU PENGUNCI SERANGAN ---
    private bool isAttacking = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");

        attackAction = new InputAction("Attack", binding: "<Mouse>/leftButton");
    }

    void OnEnable() { moveAction.Enable(); attackAction.Enable(); }
    void OnDisable() { moveAction.Disable(); attackAction.Disable(); }

    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();

        // Hanya boleh ganti arah hadap dan jalan JIKA TIDAK SEDANG MENEBAS
        if (!isAttacking)
        {
            if (movement.sqrMagnitude > 0) 
            {
                lastFaceDir = movement.normalized; 
                
                anim.SetFloat("MoveX", Mathf.Abs(movement.x));
                anim.SetFloat("MoveY", movement.y);
                if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1); 
                else if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);  
            }
            anim.SetFloat("Speed", movement.sqrMagnitude);
        }

        // EKSEKUSI SERANGAN DENGAN PENGUNCI
        // (Hanya bisa ditekan jika isAttacking bernilai false)
        if (attackAction.WasPressedThisFrame() && !isAttacking)
        {
            isAttacking = true; // Kunci tombol serangan
            anim.SetFloat("Speed", 0f); // Paksa Raka berhenti meluncur saat menebas
            
            anim.SetTrigger("Attack");
            Invoke("SerangMusuh", 0.2f); // Tunda hitbox 0.2 detik (sinkronisasi pedang)
            
            // Buka kembali kuncinya setelah 0.5 detik 
            // (Sesuaikan 0.5f ini dengan durasi asli animasi tebasan Aseprite Anda)
            Invoke("SelesaiSerang", 0.5f); 
        }
    }

    void SerangMusuh()
    {
        Vector2 attackPos = rb.position + lastFaceDir * attackOffset;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(1, transform); // Mengirim data posisi Raka
            }
        }
    }

    // Fungsi pembuka gembok serangan
    void SelesaiSerang()
    {
        isAttacking = false;
    }

    void FixedUpdate()
    {
        // Hanya izinkan fisika berjalan jika tidak sedang mengayunkan pedang
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 faceDir = lastFaceDir == Vector2.zero ? new Vector2(0, -1) : lastFaceDir;
        Vector2 attackPos = (Vector2)transform.position + faceDir * attackOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}