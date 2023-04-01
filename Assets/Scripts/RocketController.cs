using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    /// <summary>
    /// La vitesse maximum de rotation
    /// </summary>
    public float rotationSpeed;
    /// <summary>
    /// La vitesse est multiplier par combien lorsque le joueur est trouver?
    /// </summary>
    [SerializeField] float targetLockSpeedMultiplier;
    /// <summary>
    /// La vitesse de rotation est multiplier par combien lorsque le joueur est trouver?
    /// </summary>
    [SerializeField] float targetLockRotationMultiplier;
    ConstantMovement movement;
    Transform player;
    float targetRotationZ;
    bool targetLocked = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        movement = GetComponent<ConstantMovement>();
    }

    void Update()
    {
        //Le joueur est dans quelle direction
        targetRotationZ = Quaternion.LookRotation(new Vector3(0, 0, 1), player.position - transform.position).eulerAngles.z;
        //Je dois tourner dans quelle direction pour aller vers le joueur?
        float diff1 = targetRotationZ - transform.rotation.eulerAngles.z;
        float diff2 = diff1 + (360 * Mathf.Sign(diff1) * -1);
        float direction = Mathf.Abs(diff1) > Mathf.Abs(diff2)? Mathf.Sign(diff2) : Mathf.Sign(diff1);

        //Je tourne a la vitesse desiree dans cette direction
        transform.Rotate(new Vector3(0, 0, direction * rotationSpeed * Time.deltaTime));

        //Si on est pointer vers le joueur, on augmente la vitesse et reduit la vitesse de rotation
        if (!targetLocked && Mathf.Abs(targetRotationZ - transform.rotation.eulerAngles.z) < 1)
        {
            Debug.Log("Target Aquired!");
            targetLocked = true;
            movement.speed *= targetLockSpeedMultiplier;
            rotationSpeed *= targetLockRotationMultiplier;
        }
    }

    private void OnDisable()
    {
        targetLocked = false;
    }
}
