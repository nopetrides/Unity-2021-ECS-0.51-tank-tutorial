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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

// uses a modified version of the ObjectPooler from
// https://www.raywenderlich.com/847-object-pooling-in-unity

public class FXManager : ObjectPooler
{
    // singleton reference
    public static FXManager Instance;

    [Space]

    // explosion to pool
    [SerializeField] private GameObject explosionPrefab;

    // tag is used to find object in pool
    [SerializeField] private string explosionTag = "Explosion";

    // size of the initial object pool
    [SerializeField] private int poolSize = 40;

    // simple Singleton
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // create a new entry in the object pool
        if (explosionPrefab != null)
        {
            ObjectPoolItem explosionPoolItem = new ObjectPoolItem
            {
                objectToPool = explosionPrefab,
                amountToPool = poolSize,
                shouldExpand = true
            };

            // add to the current pool
            itemsToPool.Add(explosionPoolItem);
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    // move the pooled explosion prefab into place (particles are set to Play on Awake)
    public void CreateExplosion(Vector3 pos)
    {
        GameObject instance = GetPooledObject(explosionTag);
        if (instance != null)
        {
            instance.SetActive(false);
            instance.transform.position = pos;
            instance.SetActive(true);
        }

    }
}
