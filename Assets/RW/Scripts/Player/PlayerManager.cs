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

/// <summary>
///  Simple controller class for managing the player movement, input and shooting
/// </summary>

[RequireComponent(typeof(PlayerMover), typeof(PlayerInput), typeof(PlayerWeapon))]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;

    private PlayerMover playerMover;
    private PlayerInput playerInput;
    private PlayerWeapon playerWeapon;

    public static string playerTagName = "Player";

    private void Awake()
    {
        playerMover = GetComponent<PlayerMover>();
        playerInput = GetComponent<PlayerInput>();
        playerWeapon = GetComponent<PlayerWeapon>();
        EnablePlayer(true);
    }

    private void Update()
    {
        if (GameManager.IsGameOver())
        {
            return;
        }

        playerWeapon.IsFireButtonDown = playerInput.IsFiring;

    }

    private void FixedUpdate()
    {
        // hide the player if destroyed
        if (GameManager.IsGameOver())
        {
            EnablePlayer(false);
            return;
        }

        // get the keyboard input converted to camera space
        Vector3 input = playerInput.GetCameraSpaceInputDirection(sceneCamera);

        // use input to move the player
        playerMover.MovePlayer(input);

        // aim the turret to the mouse position
        playerMover.AimAtMousePosition(sceneCamera);
    }

    // toggle all GameObjects associated with the Player tag
    public static void EnablePlayer(bool state)
    {
        GameObject[] allPlayerObjects = GameObject.FindGameObjectsWithTag(playerTagName);
        foreach (GameObject go in allPlayerObjects)
        {
            go.SetActive(state);
        }
    }

}
