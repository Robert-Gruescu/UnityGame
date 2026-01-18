using UnityEngine;
using System.Collections;
using UnityEngine.UI;



public class Player : MonoBehaviour
{


    public int coinCount = 0;
    public int coins = 0; // For shop system compatibility
    
    // --- Health ---
    public int maxHealth = 100;
    public int currentHealth;
    public int damagePerHit = 10;

    //Parameters
    public bool isDodging = false;
    private bool isDashing = false; // separat de isDodging

    public bool IsBlocking = false;
    public bool isDead = false;
    public bool isGround = true;

    // --- Dodge and Dash ---
    public float dodgeDuration = 0.4f;
    public float dashDuration = 0.45f;

    public float dodgeSpeed = 2f;
    public float dashSpeed = 0.5f;

    // --- Components ---
    public Animator animator;
    public Rigidbody2D rb;


    public float jumpHeight = 10f;

    // --- Movement ---
    private float movement = 0f;
    public float moveSpeed = 5f;
    private bool facingRight = true;


    // --- UI Elements ---
    public Text healthText;
    public Text coinText;


    // --- Counters ---
    private int dodgeCount = 0;
    private int dashCount = 0;
    public int maxConsecutive = 3;


    // --- Attack ---
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;

    void Update()
    {
        // Initialize currentHealth once
        if (currentHealth <= 0)
            currentHealth = maxHealth;

        // Sync coins with coinCount for shop system
        coins = coinCount;
        coinText.text = coinCount.ToString();

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isDead)
        {
            return;
        }
        healthText.text = maxHealth.ToString();
        //dodgeroll
        if (isDodging) return;

        HandleDodgeRoll(horizontalInput);
        HandleDash(horizontalInput);


        // ----------------------------------------------------
        // 3. Mișcare normală
        // ----------------------------------------------------
        movement = horizontalInput;

        if (movement > 0f && !facingRight) Flip();
        else if (movement < 0f && facingRight) Flip();

        // ----------------------------------------------------
        // 4. Jump
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
            isGround = false;
            animator.SetBool("Jump", true);
        }

        animator.SetFloat("Run", Mathf.Abs(movement) > 0.1f ? 1f : 0f);

        // ----------------------------------------------------
        // 5. Atacuri
        // ----------------------------------------------------
        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Basic_Attack");

        if (Input.GetMouseButtonDown(1))
        {
            IsBlocking = true;
            animator.SetBool("IsBlocking", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            IsBlocking = false;
            animator.SetBool("IsBlocking", false);
        }
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(movement, 0f, 0f) * Time.fixedDeltaTime * moveSpeed;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        rb.AddForce(new Vector2(0f, jumpHeight), ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
            animator.SetBool("Jump", false);

            // Reset counters când ajunge pe sol
            dodgeCount = 0;
            dashCount = 0;
        }
    }

    // ----------------------------------------------------
    void HandleDodgeRoll(float horizontalInput)
    {
        // deja faci dodge → nu mai verificăm nimic
        if (isDodging) return;

        // Ctrl este trigger-ul
        if (!Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.RightControl))
            return;

        // condițiile existente
        if (!isGround) return;
        if (dodgeCount >= maxConsecutive) return;
        if (Mathf.Abs(horizontalInput) < 0.1f) return;

        // orientare corectă
        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();

        dashCount = 0;

        // pornește imediat animația și mișcarea
        StartCoroutine(DodgeRoll(horizontalInput));
    }

    private IEnumerator DodgeRoll(float horizontalInput)
    {
        isDodging = true;
        dodgeCount++;

        // 1️⃣ Pornește animația de Dodge imediat
        animator.SetTrigger("DodgeRoll");

        // 2️⃣ Blocăm mișcarea normală (nu afectăm velocity-ul din FixedUpdate)
        float direction = horizontalInput > 0 ? 1f : -1f;

        // Aplicăm un impuls scurt
        rb.linearVelocity = Vector2.zero; // resetăm orice viteză anterioară
        rb.AddForce(new Vector2(direction * dodgeSpeed, 0f), ForceMode2D.Impulse);

        // 3️⃣ În timpul dodge-ului ignorăm parametrii de run
        float dodgeTimer = 0f;
        while (dodgeTimer < dodgeDuration)
        {
            dodgeTimer += Time.deltaTime;

            // forțăm Run = 0 în animator, ca să nu treacă la dash/run
            animator.SetFloat("Run", 0f);

            yield return null;
        }

        // 4️⃣ Finalizăm Dodge
        isDodging = false;
    }


    // ----------------------------------------------------
    //  DASH (Shift)
    // ----------------------------------------------------
    void HandleDash(float horizontalInput)
    {
        // deja faci dash → nu mai verificăm nimic
        if (isDashing) return;

        // Shift este trigger-ul
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (!shiftHeld) return;

        // verificăm direcția
        if (Mathf.Abs(horizontalInput) < 0.1f) return;

        // condițiile existente
        if (!isGround) return;
        if (dashCount >= maxConsecutive) return;

        // orientare corectă
        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();

        dodgeCount = 0; // resetăm counter-ul de dodge pentru claritate

        // pornește imediat animația și mișcarea
        StartCoroutine(Dash(horizontalInput));
    }

    private IEnumerator Dash(float horizontalInput)
    {
        isDashing = true;
        dashCount++;

        animator.SetTrigger("Dash");

        float direction = horizontalInput > 0 ? 1f : -1f;

        float timer = 0f;
        while (timer < dashDuration)
        {
            timer += Time.deltaTime;

            // mișcare constantă pe durata dash-ului
            rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);

            animator.SetFloat("Run", 0f); // blocăm Run

            yield return null;
        }

        // Odată ce s-a terminat
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isDashing = false;
    }


    // Take damage method  

    public void TakeDamage(int damage)
    {
        if (isDead) return; // dacă deja e mort, nu mai aplicăm damage

        if (IsBlocking)
        {
            // Player blochează → damage redus sau 0
            int reducedDamage = damage / 2; // exemplu: primește jumătate din damage
            maxHealth -= reducedDamage;
            Debug.Log("Player blocked! Took " + reducedDamage + " damage.");
        }
        else
        {
            // Player nu blochează → primește damage complet
            maxHealth -= damage;
            Debug.Log("Player took " + damage + " damage.");
        }

        // Verificăm dacă moare după ce aplicăm damage-ul
        if (maxHealth <= 0)
        {
            Death();
        }
    }




    // Attack dmg 
    public void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            attackLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();

            if (enemyScript != null)
            {
                enemyScript.TakeDamage(10);
            }

            Debug.Log("Hit: " + enemy.name);
        }
    }

    //Coin colection method
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            coinCount++;
            other.gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Collected");
            Destroy(other.gameObject, 0.35f);
            Debug.Log("Coin collected!");
        }
    }



    // Gizmos pentru raza de atac
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }


    // Death logic + animation
    public void Death()
    {
        if (isDead) return;
        isDead = true;

        // 🔥 Oprește TOT
        movement = 0;
        rb.linearVelocity = Vector2.zero;

        // 🔥 Resetăm orice animație activă
        animator.SetFloat("Run", 0f);
        IsBlocking = false;
        animator.SetBool("IsBlocking", false);
        animator.SetBool("isDead", true);

        // 🔥 Dezactivăm collider-ul ca inamicii să nu mai-l împingă
        // 🔥 Dezactivăm coliziunile doar cu inamicii
        Collider2D playerCol = GetComponent<Collider2D>();
        EnemyScript[] enemies = FindObjectsOfType<EnemyScript>();
        foreach (EnemyScript enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (playerCol != null && enemyCol != null)
                Physics2D.IgnoreCollision(playerCol, enemyCol, true);
        }

        FindObjectOfType<GameManager>().isGameActive = false;
        //Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        coinCount += amount;
        Debug.Log("Added " + amount + " coins! Total: " + coins);
    }
}




