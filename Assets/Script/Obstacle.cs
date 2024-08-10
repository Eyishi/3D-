using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private GameManager generator;
    void Start()
    {
        generator = GameObject.FindObjectOfType<GameManager>();//获取游戏的管理
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            generator.GameOver();
        }
    }
}
