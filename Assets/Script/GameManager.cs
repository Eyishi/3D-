using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoreLabel;//分数
    public Text timeLabel;//时间
    
    private int score = 0;

    private float time; //时间  

    private bool gameOver;//分数
    
    public Car car;
    public Animator gameOverAnimator;//游戏结束控制动画
    public Animator scoreEffect;//得分动画
    public Animator UIAnimator;
    
    //游戏结束的文本
    public Text gameOverScoreLabel;
    public Text gameOverBestLabel;
    public Text gameOverTimeLable;
    public AudioSource gameOverAudio;
 
    
    void Start()
    {
        UpdateScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            UpdateTime();
        }
        if(gameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))){
            gameOverAnimator.SetTrigger("Restart");
            StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
        }
    }
    /// <summary>
    /// 更新时间
    /// </summary>
    public void UpdateTime()
    {
        time += Time.deltaTime;
        int timer = (int)time;
        int seconds = timer % 60;
        int minutes = timer / 60;

        string secondsBounded = (seconds < 10 ? "0" : "") + seconds;
        string minutesBounded = (minutes < 10 ? "0" : "") + minutes;
        timeLabel.text = minutes + ":" + seconds;
    }
    public void UpdateScore(int point)
    {
        score += point;
        scoreLabel.text = "" + score;
        if ( point !=0)
            scoreEffect.SetTrigger("Score");
    }

    public void GameOver()
    {
        if (gameOver)
        {
            return;
        }
        SetPanel();//设置一下分数
        
        //汽车破碎
        car.FallApart();
        gameOverAudio.Play();
        gameOver = true;
        gameOverAnimator.SetTrigger("GameOver");
        //游戏结束   获取所有移动的组件把他们速度设为0 
        foreach (BasicMovement basicMovement in GameObject.FindObjectsOfType<BasicMovement>())
        {
            basicMovement.moveSpeed = 0;
            basicMovement.rotateSpeed = 0;
        }
    }

    void SetPanel()
    {
        //更新最大的分数
        if (score > PlayerPrefs.GetInt("best"))
            PlayerPrefs.SetInt("best", score);
        gameOverScoreLabel.text = "score:" + score;
        gameOverBestLabel.text = "best:" + PlayerPrefs.GetInt("best");
        gameOverTimeLable.text = "time:" + timeLabel.text;
    }
    IEnumerator LoadScene(string scene){
        yield return new WaitForSeconds(0.6f);
		
        SceneManager.LoadScene(scene);
    }
}
