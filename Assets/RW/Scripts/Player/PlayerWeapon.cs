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
using Unity.Entities;
using Unity.Transforms;


/// <summary>
/// Handles player shooting
/// </summary>
public class PlayerWeapon : MonoBehaviour
{

    [Header("Specs")]

    // time between shots
    [SerializeField] private float rateOfFire = 0.15f;

    // where the weapon's bullet appears
    [SerializeField] private Transform muzzleTransform;

    // GameObject prefab 
    [SerializeField] private GameObject bulletPrefab;

    [Header("Effects")]
    [SerializeField] private AudioSource soundFXSource;
    // reference to the current World's EntityManager
    private EntityManager entityManager;

    // prefab converted into an entityPrefab
    private Entity bulletEntityPrefab;

    // timer until weapon and shoot again
    private float shotTimer;

    // is the fire button held down?
    private bool isFireButtonDown;
    public bool IsFireButtonDown { get { return isFireButtonDown; } set { isFireButtonDown = value; } }

    protected virtual void Start()
    {
        // get reference to current EntityManager
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // create entity prefab from the game object prefab, using default conversion settings
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        bulletEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);
    }

    public void FireBulletNonECS()
    {
        // instantiates a GameObject prefab to fire a bullet
        GameObject instance = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation, null);

        // plays one-shot sound (pew pew pew!)
        soundFXSource?.Play();
    }

    public virtual void FireBullet()
    {
        // create an entity based on the entity prefab
        Entity bullet = entityManager.Instantiate(bulletEntityPrefab);

        // set it to the muzzle angle and position
        entityManager.SetComponentData(bullet, new Translation { Value = muzzleTransform.position });
        entityManager.SetComponentData(bullet, new Rotation { Value = muzzleTransform.rotation });

        // plays one-shot sound (pew pew pew!)
        soundFXSource?.Play();
    }

    protected virtual void Update()
    {
        // ignore if the player is dead
        if (GameManager.IsGameOver())
        {
            return;
        }

        // count up to the next time we can shoot
        shotTimer += Time.deltaTime;
        if (shotTimer >= rateOfFire && isFireButtonDown)
        {
            // fire and reset the timer
            FireBullet();
            shotTimer = 0f;
        }
    }
}
