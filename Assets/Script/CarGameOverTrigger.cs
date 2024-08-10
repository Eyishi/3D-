using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGameOverTrigger : MonoBehaviour {
	
    //管理器
    GameManager manager;
	
    void Start(){
      
        manager = GameObject.FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter(Collider other){
        //顶部碰到墙壁结束游戏
        if(other.gameObject.name == "World piece")
            manager.GameOver();
    }
}