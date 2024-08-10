using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLight : MonoBehaviour
{
    public Car car;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(openLight());
        }
    }

    IEnumerator openLight()
    {
        car.leftCarLight.enabled = true;
        car.rightCarLight.enabled = true;

        yield return new WaitForSeconds(8);
        car.leftCarLight.enabled = false;
        car.rightCarLight.enabled = false;
    }
}
