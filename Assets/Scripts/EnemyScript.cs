using System;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    // Event fired when this enemy dies. Subscribers receive the EnemyScript instance.
    public static Action<EnemyScript> OnEnemyDied;
    
    // Static counter for every 2nd enemy killed
    private static int totalEnemiesKilledGlobal = 0;

    public int maxHealth = 50;
    public float moveSpeed = 2f;
    public Transform pointA;   // punctul din stânga
    public Transform pointB;   // punctul din dreapta
    private int direction = -1; // -1 = stânga, +1 = dreapta
    public bool inRange = false;
    public Transform player;
    public float attackRange = 5f;
    public float retrieveRange = 1f;
    public float chaseSpeed = 3f;
    public Animator animator;

    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayers;
    
    public GameObject coinPrefab; // assign Coin.prefab here


    void Start()
    {
        // Inițializare dacă este necesar
    }

    void Update()
    {

        IsActive();

        if (maxHealth <= 0)
        {
            Death();

        }
        EnemyAttack();
    }

    void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }


    //Logica de attack + animatie
    void EnemyAttack()
    {
        // 1️⃣ Verificăm dacă jocul este activ și playerul este viu
        Player playerScript = player.GetComponent<Player>();
        if (playerScript == null || playerScript.isDead || !FindObjectOfType<GameManager>().isGameActive)
        {
            // Playerul este mort sau jocul oprit → inamicul patrulează
            Patrol();
            animator.SetBool("Attack1", false); // oprim animatia de atac
            return;
        }

        // 2️⃣ Dacă playerul este viu → atac normal
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
        }
        else
        {
            inRange = false;
        }

        if (inRange)
        {
            EnemyPositions();

            if (Vector2.Distance(transform.position, player.position) > retrieveRange)
            {
                animator.SetBool("Attack1", false);
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Attack1", true);
            }
        }
        else
        {
            Patrol();
        }
    }


    void Patrol()
    {
        // Mișcare bazată pe direcția actuală
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        // Schimbare direcție la punctele A și B
        if (direction == -1 && transform.position.x <= pointA.position.x)
        {
            Flip();
        }

        if (direction == 1 && transform.position.x >= pointB.position.x)
        {
            Flip();
        }
    }


    void EnemyPositions()
    {
        if (player.position.x < transform.position.x)
        {
            // Jucătorul este în stânga inamicului
            if (direction != -1)
            {
                Flip();
            }
        }
        else
        {
            // Jucătorul este în dreapta inamicului
            if (direction != 1)
            {
                Flip();
            }
        }

    }

    //Logica de attack(punctul de coleziune)
    public void Attack()
    {
        Collider2D[] collInfo = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, attackLayers);
        if (collInfo.Length > 0)
        {
            if (collInfo[0].GetComponent<Player>() != null)
            {
                collInfo[0].GetComponent<Player>().TakeDamage(10);
            }

        }

    }


    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0)
        {
            return;
        }
        maxHealth -= damage;
        animator.SetTrigger("Hurt");

    }

    // Public helper to set initial facing/direction from external systems (spawner)
    // dir should be 1 (move right) or -1 (move left)
    public void SetDirection(int dir)
    {
        if (dir != 1 && dir != -1)
            return;

        if (direction != dir)
        {
            direction = dir;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir == 1 ? 1f : -1f);
            transform.localScale = scale;
        }
    }





    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint == null)
        {
            return;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    void Death()
    {
        Debug.Log(this.transform.name + " died.");
        
        // Increment global counter and check if every 2nd enemy
        totalEnemiesKilledGlobal++;
        if (totalEnemiesKilledGlobal % 2 == 0)
        {
            // Spawn a coin at this enemy's position
            if (coinPrefab != null)
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
                Debug.Log("Coin spawned at: " + transform.position);
            }
            else
            {
                Debug.LogWarning("EnemyScript: coinPrefab is not assigned on " + gameObject.name);
            }
        }
        
        // notify listeners before destroying
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }

    //Verifica daca jocul este activ ( daca playerul a murit)
    void IsActive()
    {
        if (FindObjectOfType<GameManager>().isGameActive == false)
        {
            return;
        }



    }

}
