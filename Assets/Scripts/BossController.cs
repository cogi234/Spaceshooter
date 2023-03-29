using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    //References necessaires
    GameManager gameManager;
    SpriteRenderer coreSpriteRenderer;
    [SerializeField] List<Sprite> coreSprites;
    [SerializeField] HealthComponent myHealth, leftShieldGen, rightShieldGen;

    [SerializeField] Vector3 mainPosition = new Vector3(0, 3, 0);
    [SerializeField] float movementSpeed = 4;
    /// <summary>
    /// Chaque KeyValuePair a la fonction d'attaque comme clee et comme valeur a partir de quelle phase elle peut etre utilisee
    /// </summary>
    Dictionary<Func<IEnumerator>, int> attacks;
    int phase = 1;


    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.pauseEnemySpawning = true;
        coreSpriteRenderer = myHealth.gameObject.GetComponent<SpriteRenderer>();
    }

    private IEnumerator Start()
    {
        //Le boss peut pas prendre de dommage quand il apparait
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

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }
    }

    //Le boss peut maintenant prendre du dommage
    private void OnDestroy()
    {
        gameManager.pauseEnemySpawning = false;
    }



    public void OnCoreDamage(int damage, int health)
    {
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
}
