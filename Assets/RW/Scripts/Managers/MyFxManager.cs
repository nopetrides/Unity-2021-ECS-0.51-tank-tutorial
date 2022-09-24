
using UnityEngine;


public class MyFxManager : MonoBehaviour
{
    // singleton reference
    public static MyFxManager Instance;

    // explosion to pool
    public GameObject explosionPrefab;

    // simple Singleton
    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        FXSpawnerSystem.prefab = explosionPrefab;
    }

}
