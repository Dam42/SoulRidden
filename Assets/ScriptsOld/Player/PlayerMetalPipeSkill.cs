using System.Collections;
using UnityEngine;

public class PlayerMetalPipeSkill : MonoBehaviour
{
    [SerializeField] private GameObject metalPipe;
    [SerializeField] private PlayerHealthAndMana playerHealthAndMana;

    [SerializeField] private int skillManaCost;
    [SerializeField] private int skillDamage;
    [SerializeField] private float skillCooldown;
    [SerializeField] private bool isSkillOnCooldown = false;
       

    private void Update()
    {
        if (Input.GetButtonDown("skill") && !isSkillOnCooldown) UseMetalPipeSkill(skillManaCost);    
    }

    private void UseMetalPipeSkill(int manacost)
    {
        StartCoroutine("DoSetSkillCooldown");
        if (playerHealthAndMana.playerUseMana(manacost) == true)
        {
            metalPipe.GetComponent<metalPipeEnemy>().pipeDamage = skillDamage;
            Instantiate(metalPipe, new Vector2(transform.position.x - 1, transform.position.y + 3), Quaternion.identity);
            Instantiate(metalPipe, new Vector2(transform.position.x + 1, transform.position.y + 3), Quaternion.identity);
        }     
    }

    private IEnumerator DoSetSkillCooldown()
    {
        isSkillOnCooldown = true;
        yield return new WaitForSecondsRealtime(skillCooldown);
        isSkillOnCooldown = false;
    }
}
