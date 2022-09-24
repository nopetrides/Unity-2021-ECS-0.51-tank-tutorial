using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FXSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject explosionPrefab;

    private Entity explosionEntityFromPrefab;

    // DOTS
    private EntityManager dotsEntityManager;

    public static FXSpawner Instance;

    [SerializeField] private AudioSource sfx;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Get the World
        dotsEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        explosionEntityFromPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(explosionPrefab, settings);
    }

    public void SpawnFX(float3 location)
    {
        Entity fx = dotsEntityManager.Instantiate(explosionEntityFromPrefab);

        dotsEntityManager.SetComponentData(fx, new Translation { Value = location });

        sfx.Play();
    }
}

