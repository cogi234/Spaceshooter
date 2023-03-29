using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    //References necessaires
    GameManager gameManager;
    SpriteRenderer coreSpriteRenderer;
    [SerializeField] List<Sprite> coreSprites;
    [SerializeField] HealthComponent myHealth, leftShieldGen, rightShieldGen;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] Transform leftGun, centerGun, rightGun;
    Slider healthBar;

    [SerializeField] Vector3 mainPosition = new Vector3(0, 3, 0);
    [SerializeField] float movementSpeed = 4;
    /// <summary>
    /// Le nombre de points donnes par le boss
    /// </summary>
    [SerializeField] int points = 1000;
    /// <summary>
    /// Chaque KeyValuePair a la fonction d'attaque comme clee et comme valeur a partir de quelle phase elle peut etre utilisee
    /// </summary>
    Dictionary<Func<IEnumerator>, int> attacks;

    // phases 1,2,3,4
    int phase = 1;
    //Est-ce qu'on peut commencer une nouvelle attaque?
    bool canAttack = false;

    private void Awake()
    {
        //Tout initialiser
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.pauseEnemySpawning = true;
        coreSpriteRenderer = myHealth.gameObject.GetComponent<SpriteRenderer>();
        healthBar = GetComponentInChildren<Slider>();
        healthBar.maxValue = myHealth.maxHealth;
        healthBar.value = myHealth.Health;
    }

    private IEnumerator Start()
    {
        //Le boss ne peut pas prendre de dommage quand il apparait
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        //Bouger vers la position principale
        Vector3 direction = (mainPosition - transform.position).normalized;
        float distance = float.PositiveInfinity;
        //Tant qu'on est pas rendu la, on continue a bouger
        while (transform.position != mainPosition)
        {
            //On attends une frame
            yield return null;
            //On bouge a la vitesse voulue
            transform.Translate(direction * Time.deltaTime * movementSpeed * 0.5f);
            //Si on est rendu plus loin, c'est qu'on a depasser. Dans ce cas on se met a la position principale
            if (Vector3.Distance(transform.position, mainPosition) > distance)
                transform.position = mainPosition;
            //On calcule la nouvelle distance
            distance = Vector3.Distance(transform.position, mainPosition);
        }

        //Le boss peut maintenant prendre du dommage
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }
        canAttack = true;
    }

    private void OnDestroy()
    {
        gameManager.pauseEnemySpawning = false;
    }

    private void Update()
    {
        if (canAttack)
        {
            //On trouve les attaques possibles dans la phase dans laquelle on est
            List<Func<IEnumerator>> possibleAttacks = new List<Func<IEnumerator>>();
            foreach (KeyValuePair<Func<IEnumerator>, int> kv in attacks)
            {
                if (kv.Value <= phase)
                    possibleAttacks.Add(kv.Key);
            }

            //Puis on commence une attaque aleatoire parmis celles-ci
            StartCoroutine(possibleAttacks[UnityEngine.Random.Range(0, possibleAttacks.Count)]());

            //On ne peut plus commencer une attaque
            canAttack = false;
        }
    }


    public void OnCoreDamage(int damage, int health)
    {
        healthBar.value = myHealth.Health;
        float healthpercent = (float)health / (float)myHealth.maxHealth;

        int newPhase = 4 - Mathf.FloorToInt(healthpercent * 4);
        if (newPhase > phase)
        {
            phase = newPhase;
            ChangePhase();
        }

        coreSpriteRenderer.sprite = coreSprites[Mathf.FloorToInt(healthpercent * 4)];
    }

    void ChangePhase()
    {
        //On change pas de phase si on est mort
        if (myHealth.Health > 0)
        {
            //On reactive les generateur de bouclier
            leftShieldGen.Heal(int.MaxValue);
            rightShieldGen.Heal(int.MaxValue);
        }
    }

    public void OnDeath()
    {
        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        //On donne les points
        gameManager.Score += points;
        //On desactive les collisions
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
        //Combien de temps la sequence de mort va-t-elle prendre
        float timer = 5;
        float explosionChancePerSecond = 10;
        while (timer >= 0)
        {
            if (UnityEngine.Random.value <= explosionChancePerSecond * Time.deltaTime)
            {
                Vector3 explosionPosition = transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).position;
                Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
