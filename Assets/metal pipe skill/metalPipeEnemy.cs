using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class metalPipeEnemy : MonoBehaviour
{
    public int pipeDamage = 50;
    private bool hadHitPlayer;
    private AudioManager sound;

    private void Awake()
    {
        sound = FindObjectOfType<AudioManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !hadHitPlayer)
        {
            collision.gameObject.GetComponent<PlayerHealthAndMana>().playerTakeDamage(pipeDamage);
            hadHitPlayer = true;
        }
        else if(collision.gameObject.CompareTag("Ground")) hadHitPlayer = true;

        sound.Play("metalPipeHit");
        Destroy(gameObject, 2f);
    }
}
