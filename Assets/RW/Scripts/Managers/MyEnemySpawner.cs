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
    private float spawnTimer;

    // flag from GameManager to enable spawning
    private bool canSpawn;

    // DOTS
    private EntityManager dotsEntityManager;

    // Components
    [SerializeField]
    private Mesh enemyMesh;

    [SerializeField]
    private Material enemyMaterial;

    public EntityCommandBuffer.ParallelWriter Ecb;

    private void Start()
    {
        // Get the World
        dotsEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

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

    // spawns enemies in a ring around the player
    private void SpawnWave()
    {

    }

    // get a random point on a circle with given radius
    private float3 RandomPointOnCircle(float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * radius;

        // return random point on circle, centered around the player position
        return new float3(randomPoint.x, 0.5f, randomPoint.y) + (float3)GameManager.GetPlayerPosition();
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
        spawnTimer += Time.deltaTime;

        // spawn and reset timer
        if (spawnTimer > spawnInterval)
        {
            SpawnWave();
            spawnTimer = 0;
        }
    }
}
