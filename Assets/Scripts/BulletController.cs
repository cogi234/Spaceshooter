using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    /// <summary>
    /// Les tags qui vont pouvoir etre endommager par cette balle
    /// </summary>
    public List<string> targetTags;
    /// <summary>
    /// Combien de dommage fait-on?
    /// </summary>
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Est-ce que l'autre objet a un tag qu'on cible?
        if (targetTags.Contains(collision.gameObject.tag))
        {
            //On essaie de prendre le HealthComponent de l'autre objet pour l'endommager
            HealthComponent health;
            if (collision.gameObject.TryGetComponent<HealthComponent>(out health))
                health.TakeDamage(damage);
            //On desactive la balle
            gameObject.SetActive(false);
        }
    }
}
