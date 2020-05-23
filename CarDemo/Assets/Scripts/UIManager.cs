using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool gameOver = true;
    public Button[] buttons;
    public Button pauseBtn;
    public Text text;

    int score = 0;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateScore", 0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateScore()
    {
        if (!gameOver && Time.timeScale != 0){
            score += 1;
        }
        text.text = "Score: " + score;
    }

    public void GameOver()
    {
        gameOver = true;
        score = 0;
        UpdateScore();
        Debug.Log("game over");
        foreach (Button btn in buttons){
            btn.gameObject.SetActive(true);
        }
        pauseBtn.gameObject.SetActive(false);
    }


    public void Replay() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Pause(){
        if (Time.timeScale == 1){
            Time.timeScale = 0;
        } else if (Time.timeScale == 0){
            Time.timeScale = 1;
        }
    }

}
