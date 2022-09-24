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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



/// <summary>
/// Manages the demo game.
/// </summary>
public class GameManagerNonECS : MonoBehaviour
{
    // singleton reference
    public static GameManagerNonECS Instance;

    // simple UI elements to transition and show score/time
    [SerializeField] private Image screenFader;
    [SerializeField] private float delay = 2f;
    [SerializeField] TextMeshPro scoreText;
    [SerializeField] TextMeshPro timeText;

    // current game state
    private GameState gameState;

    private Transform player;
    private EnemySpawnerNonECS enemySpawner;

    private float score;
    private float timeElapsed;

    // simple Singleton
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }


        player = FindObjectOfType<PlayerManager>().transform;
        enemySpawner = FindObjectOfType<EnemySpawnerNonECS>();

        gameState = GameState.Ready;
    }

    private void Start()
    {
        gameState = GameState.Starting;
        StartCoroutine(MainGameLoopRoutine());
    }

    // main game loop
    private IEnumerator MainGameLoopRoutine()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(EndGameRoutine());
    }


    private IEnumerator StartGameRoutine()
    {
        timeElapsed = 0f;

        PlayerManager.EnablePlayer(true);
        screenFader?.CrossFadeAlpha(0f, delay, true);


        yield return new WaitForSeconds(delay);
        gameState = GameState.Playing;
    }

    private IEnumerator PlayGameRoutine()
    {

        enemySpawner?.StartSpawn();

        while (gameState == GameState.Playing)
        {
            timeElapsed += Time.deltaTime;

            if (timeText != null)
            {
                timeText.text = timeElapsed.ToString("0.##");
            }
            yield return null;
        }
    }

    private IEnumerator EndGameRoutine()
    {

        // fade to black and wait
        screenFader?.CrossFadeAlpha(1f, delay, true);
        yield return new WaitForSeconds(delay);

        gameState = GameState.Ready;

        // restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // static methods to send messages to Entities
    public static Vector3 GetPlayerPosition()
    {
        if (GameManagerNonECS.Instance == null)
        {
            return Vector3.zero;
        }

        return (Instance.player != null) ? GameManagerNonECS.Instance.player.position : Vector3.zero;
    }

    // end the game (player has died)
    public static void EndGame()
    {
        if (GameManagerNonECS.Instance == null)
        {
            return;
        }

        PlayerManager.EnablePlayer(false);
        Instance.gameState = GameState.Over;
    }

    // is the game over?
    public static bool IsGameOver()
    {
        if (GameManagerNonECS.Instance == null)
        {
            return false;
        }

        return (Instance.gameState == GameState.Over);
    }

    // score points
    public static void AddScore(int scoreValue)
    {
        Instance.score += scoreValue;

        if (Instance.scoreText != null)
        {
            Instance.scoreText.text = Instance.score.ToString();
        }
    }

    // display the time text
    public static void ShowTime(int timeValue)
    {
        if (Instance.timeText != null)
        {
            Instance.timeText.text = timeValue.ToString();
        }
    }
}
