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


/// <summary>
/// enemy GameObject holding and data and behaviour in non-ECS example
/// </summary>
public class EnemyNonECS : MonoBehaviour
{
    // forward movement speed
    private float moveSpeed;

    // distance to explode versus bullet or player
    private float collisionDistance = 2f;

    // player invincible by default (toggle in Prefab)
    [SerializeField] bool canHitPlayer;

    void Update()
    {
        // move forward 
        MoveForward();

        // if game is playing, turn to crash into the player
        if (!GameManagerNonECS.IsGameOver())
        {
            FacePlayer();
            CheckPlayerCollision();
            CheckBulletCollisions();

        }
    }

    public void SetMoveSpeed(float speedValue)
    {
        moveSpeed = speedValue;
    }

    // loop through all the bullets and check proximity 
    private void CheckBulletCollisions()
    {
        GameObject[] allBullets = GameObject.FindGameObjectsWithTag("Bullet");

        foreach (GameObject bullet in allBullets)
        {
            if (Vector3.Distance(bullet.transform.position, transform.position) < collisionDistance)
            {
                Destroy(gameObject);
                Destroy(bullet.gameObject);
                FXManager.Instance.CreateExplosion(transform.position);
                GameManagerNonECS.AddScore(1);
            }
        }
    }

    // check proximity to player
    private void CheckPlayerCollision()
    {
        Vector3 playerPosition = GameManagerNonECS.GetPlayerPosition();
        playerPosition.y = transform.position.y;
        if (Vector3.Distance(playerPosition, transform.position) < collisionDistance)
        {
            Destroy(gameObject);
            FXManager.Instance.CreateExplosion(transform.position);

            // player is invincible by default in this non-ECS version of the demo
            if (canHitPlayer)
            {
                FXManager.Instance.CreateExplosion(playerPosition);
                GameManagerNonECS.EndGame();
            }
        }
    }

    // move forward
    private void MoveForward()
    {
        transform.position += Time.deltaTime * moveSpeed * transform.forward;
    }

    // turn to face the player's position
    private void FacePlayer()
    {
        Vector3 playerPosition = GameManagerNonECS.GetPlayerPosition();

        Vector3 direction = playerPosition - transform.position;
        direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}

