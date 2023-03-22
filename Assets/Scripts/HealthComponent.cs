using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    //Les events pour quand on prends du dommage ou meurt
    public UnityEvent onDeath = new UnityEvent();
    public UnityEvent onDamage = new UnityEvent();
    public UnityEvent onHeal = new UnityEvent();

    //Combien de vie on a
    public int startingHealth = 1;
    public int maxHealth = 1;
    int health;
    public int Health { get => health; }

    //Pour le temps d'invincibilite
    public float invincibilityTime = 2;
    float invincibilityTimer;


    public void TakeDamage(int damage)
    {
        //Si on est pas invincible
        if (invincibilityTimer <= 0)
        {
            //On prends du dommage
            health -= damage;
            onDamage.Invoke();

            //On se met le timer d'invincibilite
            invincibilityTimer = invincibilityTime;

            //Si il ne reste plus de vie
            if (health <= 0)
            {
                //On meurt
                onDeath.Invoke();
            }

        }
    }

    public void Heal(int healing)
    {
        //On se guerit, si on est pas au max de vie
        if (health < maxHealth)
        {
            if (health + healing >= maxHealth)
                health = maxHealth;
            else
                health += healing;
            onHeal.Invoke();
        }
    }

    private void Update()
    {
        // Le temps d'invincibilite
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    private void OnEnable()
    {
        //On initialise la vie
        health = startingHealth;
    }
}
