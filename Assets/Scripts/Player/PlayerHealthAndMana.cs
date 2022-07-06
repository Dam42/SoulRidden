using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthAndMana : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBar;
    private float playerHealth;
    private float playerMaxHealth = 300;

    [Header("Mana Bar")]
    [SerializeField] private Image manaBar;
    private float playerMana;
    private float playerMaxMana = 60;
	// Temporary Mana
	private float playerTempMana;
	private float manaUsedForLastHeal;

	// Healing timer
	private float healStartTimer;
	private bool CanHeal = true;

	[Header("References")]
	[SerializeField] PlayerMovementAndJump playerMovement;
	[SerializeField] Animator anim;

    private void Awake()
    {
		playerMana = playerMaxMana;
		playerHealth = playerMaxHealth;	
    }

    private void Update()
    {
		healStartTimer += Time.deltaTime;

		if (Input.GetButton("heal")) InitHealing();
		//if (Input.GetButtonUp("heal")) EndHeal();
	}

	// Healing oh shit
	private void InitHealing()
	{
		playerMovement.CanMove = false;
		// or get access to this in movement and set CanMove in the function if the timer is less than like .2f;
		if (healStartTimer > 1f) healStartTimer = 0;
		else if (healStartTimer > .1f) HealPlayer(1);
	}

	private void HealPlayer(int manaCost)
	{
		if (playerMana >= manaCost)
		{
			playerTakeDamage(-2);
			UpdateHealthBar();
			playerUseMana(manaCost);
			UpdateManaBar();
			healStartTimer = 0;
		}
		else EndHeal();
	}

	private void EndHeal()
	{
		anim.SetBool("healing", false);
		playerMovement.CanMove = true;
		StartCoroutine("DoStopAbilityToHeal");
		// here we’ll block the healing for 5s but make the mana used usable again for skills.
	}

	private IEnumerator DoStopabilityToHeal()
    {
		CanHeal = false;
		yield return new WaitForSecondsRealtime(5f);
		CanHeal = true;
    }

	public bool playerUseMana(int manaUsed)
	{
		if (CanHeal == false) return playerUseTempMana(manaUsed);
		else
        {
			if (manaUsed > playerMana)
			{
				return false;
			}
			else
			{
				playerMana -= manaUsed;
			}
			UpdateManaBar();
			return true;
		}		
	}

    private bool playerUseTempMana(int manaUsed)
    {
		if (manaUsed > playerTempMana)
		{
			return false;
		}
		else
		{
			playerTempMana -= manaUsed;
		}
		UpdateTempManaBar();
		return true;
	}

    public void playerTakeDamage(int damageTaken)
	{
		if (damageTaken > 0) anim.SetTrigger("hurt");

		if ((playerHealth -= damageTaken) >= playerMaxHealth) playerHealth = playerMaxHealth;
		else if (damageTaken >= playerHealth)
		{
			playerHealth = 0;
			Die();
		}
		UpdateHealthBar();
	}

	private void UpdateHealthBar()
	{
		healthBar.fillAmount = (playerHealth / (playerMaxHealth / 100)) / 100;
	}

	private void UpdateManaBar()
	{
		manaBar.fillAmount = (playerMana / (playerMaxMana / 100)) / 100;
	}
	private void UpdateTempManaBar()
	{
		//manaBar.fillAmount = (playerMana / (playerMaxMana / 100)) / 100;
	}

	private void Die()
	{
		Debug.Log("oh shit");
	}

}