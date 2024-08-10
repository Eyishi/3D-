using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Music : MonoBehaviour {
	
    
    private static Music instance;
    
    void Awake(){
       
        if(!instance){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
		
        
        DontDestroyOnLoad(this.gameObject);
    }
}