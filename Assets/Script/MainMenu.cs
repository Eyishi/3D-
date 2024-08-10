using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{

    public Animator UIAnimator;

    //控制声音
    public Button btnMusic;

    //开启声音
    public AudioSource bkMusic;

    public Slider sliMusic;
    
    //红线
    public Image line;
    //声音是否开启
    private bool isMusic = true;
    private void Start()
    {
        //监听按钮
        btnMusic.onClick.AddListener((() =>
        {
            isMusic = !isMusic;
            //声音开启
            if (isMusic)
            {
                line.gameObject.SetActive(false);
                bkMusic.Play();
            }
            else
            {
                line.gameObject.SetActive(true);
                bkMusic.Stop();
            }
        }));
        sliMusic.onValueChanged.AddListener((value =>
        {
            bkMusic.volume = value;
        }));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) ||
            (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()))
        {
            if (!(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began &&
                  EventSystem.current.IsPointerOverGameObject((Input.GetTouch(0).fingerId))))
            {
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        UIAnimator.SetTrigger("Start");
        StartCoroutine(LoadScene("Game"));
    }
    //加载场景
    IEnumerator LoadScene(string scene)
    {
        yield return new WaitForSeconds(0.6f);

        SceneManager.LoadScene(scene);
    }
}