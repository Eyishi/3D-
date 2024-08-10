using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    private GameManager manager;

    //这个门是否加过分
    private bool isAddScore = false;
    public AudioSource scoreAudio;
    void Start()
    {
        manager = GameObject.FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("穿过门");
        if (!other.gameObject.transform.root.CompareTag("Player") || isAddScore)
        {
            return;
        }

        isAddScore = true;
        manager.UpdateScore(1);
        scoreAudio.Play();
    }
}
