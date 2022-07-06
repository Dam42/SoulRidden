using UnityEngine;
using UnityEngine.UI;

public class enemyTest : MonoBehaviour
{

    [SerializeField] private float HP;
    [SerializeField] private int MaxHP;
    [SerializeField] Image healthBar;
    [SerializeField] Canvas health;
    private PlayerHealthAndMana playerHealth;
    private BoxCollider2D bossCollider;

    private Animator anim;

    private void Awake()
    {
        HP = MaxHP;
        anim = GetComponent<Animator>();
        bossCollider = GetComponent<BoxCollider2D>();
        playerHealth = FindObjectOfType<PlayerHealthAndMana>();
    }

    public void TakeDamage(int damageTaken)
    {
        anim.SetTrigger("damage");
        if (damageTaken >= HP)
        {
            HP = 0;
            health.enabled = false;
            anim.SetTrigger("dead");
        }
        else HP -= damageTaken;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.fillAmount = (HP / (MaxHP / 100)) / 100;
    }

    private void Die()
    {
        bossCollider.enabled = false; 
        Destroy(gameObject, .5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            playerHealth.playerTakeDamage(22);
        }
    }
}
