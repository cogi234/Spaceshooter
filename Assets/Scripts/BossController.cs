using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    GameManager gameManager;

    [SerializeField] Vector3 mainPosition = new Vector3(0, 3, 0);
    [SerializeField] float movementSpeed = 4;


    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.pauseEnemySpawning = true;
    }

    private IEnumerator Start()
    {
        



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
    }

    private void OnDestroy()
    {
        gameManager.pauseEnemySpawning = false;
    }


}
