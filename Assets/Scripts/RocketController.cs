using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    [SerializeField] float vitesse = 100;
    Vector3 initialPosition;
    float time;
    GameObject Joueur;
    Vector3 targetRotation;
    // Start is called before the first frame update
    void Start()
    {
        Joueur = GameObject.FindGameObjectWithTag("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        targetRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), Joueur.transform.position - transform.position).eulerAngles;
        if (Mathf.Abs(targetRotation.z - transform.rotation.eulerAngles.z) > 1)
        {
            transform.Translate(Vector3.up * 10 * Time.deltaTime, Space.Self);
            transform.Rotate(new Vector3(0,0,180 * Time.deltaTime));
        }
        time += Time.deltaTime;
    }
    private void OnEnable()
    {
        initialPosition = transform.position;
        time = 0;
    }
}
