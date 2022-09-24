/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

// DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Rendering;

// Components
using Unity.Transforms;
using Unity.Collections;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;
using System;
using System.Threading;

/// <summary>
/// spawns a swarm of enemy entities offscreen, encircling the player
/// </summary>
public class MyEnemySpawner : MonoBehaviour
{

    [Header("Spawner")]
    // number of enemies generated per interval
    [SerializeField] private int spawnCount = 30;

    // time between spawns
    [SerializeField] private float spawnInterval = 3f;

    // enemies spawned on a circle of this radius
    [SerializeField] private float spawnRadius = 30f;

    // extra enemy increase each wave
    [SerializeField] private int difficultyBonus = 5;

    [Header("Enemy")]
    // random speed range
    [SerializeField] float minSpeed = 4f;
    [SerializeField] float maxSpeed = 12f;

    // counter
    private float waveSpawnTimer;
    private float spawnAccumulator;

    // flag from GameManager to enable spawning
    private bool canSpawn;

    // DOTS
    private EntityManager dotsEntityManager;

    [SerializeField]
    private GameObject enemyPrefab;

    private Entity enemyEntityFromPrefab;

    private void Start()
    {
        // Get the World
        dotsEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        enemyEntityFromPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);

        StartSpawn();

        #region Unity Example
        /*
        // Define the components that the entity will use
        var entityDescriptor = new RenderMeshDescription(
            enemyMesh, enemyMaterial, shadowCastingMode: ShadowCastingMode.Off, receiveShadows: false);

        // Create a template entity
        Entity myFirstEntity = dotsEntityManager.CreateEntity();
        
        // Add components to the renderer
        RenderMeshUtility.AddComponents(
            myFirstEntity,
            dotsEntityManager,
            entityDescriptor);
        // Add component data to the manager
        dotsEntityManager.AddComponentData(myFirstEntity, new LocalToWorld());
        // Setup a command buffer
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        // Create a Job
        var spawnJob = new SpawnJob
        {
            Prototype = myFirstEntity,
            Ecb = ecb.AsParallelWriter(),
            EntityCount = 1,
        };
        // Schedule the job
        var spawnHandle = spawnJob.Schedule(1, 128);
        // Execute the job
        spawnHandle.Complete();
        // Cleanup
        ecb.Playback(dotsEntityManager);
        ecb.Dispose();
        // Destroy the template
        dotsEntityManager.DestroyEntity(myFirstEntity);
        */
        #endregion
    }

    #region Unity Example Spawn
    /*
    // Example Burst job that creates many entities
    [BurstCompatible]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity Prototype;
        public int EntityCount;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(int index)
        {
            // Clone the Prototype entity to create a new entity.
            var e = Ecb.Instantiate(index, Prototype);
            // Prototype has all correct components up front, can use SetComponent to
            // set values unique to the newly created entity, such as the transform.
            Ecb.SetComponent(index, e, new LocalToWorld { Value = ComputeTransform(index) });
        }

        public float4x4 ComputeTransform(int index)
        {
            return float4x4.Translate(new float3(index, 0, 0));
        }
    }
    */
    #endregion


    private void SpawnWaveFraction(int enemiesToSpawn)
    {
        NativeArray<Entity> enemyArray = new NativeArray<Entity>(enemiesToSpawn, Allocator.Temp);

        for (int i = 0; i < enemyArray.Length; i++)
        {
            enemyArray[i] = dotsEntityManager.Instantiate(enemyEntityFromPrefab);

            dotsEntityManager.SetComponentData(enemyArray[i], new Translation { Value = RandomPointOutsideViewport(spawnRadius) });
            dotsEntityManager.SetComponentData(enemyArray[i], new MoveForwardComp { speed = Random.Range(minSpeed, maxSpeed) });
            dotsEntityManager.SetComponentData(enemyArray[i], new QuadrantTag { unitType = QuadrantTag.QuadrantUnitType.Enemy });
        }

        enemyArray.Dispose();
    }

    // get a random point on a circle with given radius
    private float3 RandomPointOutsideViewport(float radius)
    {
        var playerPos = GameManager.GetPlayerPosition();
        var frustumDistance = 2.0f * (Camera.main.transform.position.y+5) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumDistance * Camera.main.aspect;
        var frustumHeight = frustumWidth / Camera.main.aspect;

        var positionOnBounds = PointOnBounds(
            new Bounds(Vector3.zero, new Vector3(frustumWidth, frustumHeight, 0)), 
            new Vector2(Random.Range(-1f,1f), Random.Range(-1f, 1f)));

        var circlePos = RandomPointOnCircle(radius, playerPos);


        if (math.distance(new Vector3(positionOnBounds.x, 0, positionOnBounds.y), playerPos) < math.distance(circlePos, playerPos))
        {
            Random.Range(0, -1);
            Random.Range(0, -1);
            // return random point on circle, centered around the player position
            return new float3(positionOnBounds.x * (Random.Range(0, 2) * 2 - 1),
                0.5f,
                positionOnBounds.y * (Random.Range(0, 2) * 2 - 1)) + (float3)playerPos;
        }
        else
        {
            // return random point on circle, centered around the player position
            return new float3(circlePos.x, 0.5f, circlePos.z);
        }
    }

    public static Vector2 PointOnBounds(Bounds bounds, Vector2 aDirection)
    {
        var e = bounds.extents;
        var v = aDirection;
        float y = e.x * v.y / v.x;
        if (y < e.y)
            return new Vector2(e.x, y);
        return new Vector2(e.y * v.x / v.y, e.y);
    }
    public static Vector2 PointOnBounds(Bounds bounds, float aAngle)
    {
        float a = aAngle * Mathf.Deg2Rad;
        return PointOnBounds(bounds, new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
    }

    // get a random point on a circle with given radius
    private float3 RandomPointOnCircle(float radius, float3 playerPos)
    {
        var angle = Random.Range(0, 360);
        
        var randomPoint = Random.onUnitSphere * radius;
        // return random point on circle, centered around the player position
        return new float3 (playerPos.x + radius * math.cos(angle), 0, playerPos.z + radius * math.sin(angle));
    }

    // signal from GameManager to begin spawning
    public void StartSpawn()
    {
        canSpawn = true;
    }

    private void Update()
    {
        // disable if the game has just started or if player is dead
        if (!canSpawn || GameManager.IsGameOver())
        {
            return;
        }

        // count up until next spawn
        waveSpawnTimer += Time.deltaTime;
        spawnAccumulator += (Time.deltaTime / spawnInterval) * spawnCount;
        if (spawnAccumulator > 1)
        {
            SpawnWaveFraction(Mathf.FloorToInt(spawnAccumulator));
            spawnAccumulator = 0;
        }

        // spawn and reset timer
        if (waveSpawnTimer > spawnInterval)
        {
            StartSpawnNextWave();
        }
    }

    // reset the spawn wave
    private void StartSpawnNextWave()
    {
        spawnCount += difficultyBonus;

        waveSpawnTimer = 0;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(GameManager.GetPlayerPosition(), spawnRadius);
    }
}
