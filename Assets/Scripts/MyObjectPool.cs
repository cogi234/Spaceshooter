using System.Collections.Generic;
using UnityEngine;

public class MyObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject poolable;
    private List<GameObject> pool = new List<GameObject>();

    public GameObject GetElement()
    {
        GameObject gameObject = pool.Find(obj => obj.activeInHierarchy == false);
        if (gameObject == null)
        {
            gameObject = Instantiate(poolable, transform);
            pool.Add(gameObject);
        }
        return gameObject;
    }
}
