using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public List<string> targetTags;
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetTags.Contains(collision.gameObject.tag))
        {
            EnemyController enemyComp;
            if (collision.gameObject.TryGetComponent<EnemyController>(out enemyComp))
                enemyComp.TakeDamage(damage);
            PlayerController playerComp;
            if (collision.gameObject.TryGetComponent<PlayerController>(out playerComp))
                playerComp.TakeDamage();
            gameObject.SetActive(false);
        }
    }
}
