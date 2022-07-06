using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private AudioManager audioManager;

    [SerializeField] private float attackCooldown;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int playerAttackDamage;
    private float lastAttack;

    public Vector2 attackHitbox = new Vector2(.2f, 0);
    private float attackRange = 0.4f;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        // Timers
        lastAttack += Time.deltaTime;

        // Attack
        if (Input.GetKeyDown(KeyCode.X) && lastAttack > attackCooldown) BeginAttack();
        else anim.ResetTrigger("attack");
    }

    private void BeginAttack()
    {
        // play animation
        anim.SetTrigger("attack");

        // make cooldown
        lastAttack = 0;
    }


    private void Attack() // This function is called by animation event on the frame of the sword swing
    {
        audioManager.Play("PlayerAttack");

        // scan for objects to attack
        Collider2D[] hitEnemies = (Physics2D.OverlapCircleAll((Vector2)transform.position + attackHitbox, attackRange, enemyLayer));

        // deal damage
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<enemyTest>().TakeDamage(playerAttackDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere((Vector2)transform.position + attackHitbox, attackRange);
    }
}
