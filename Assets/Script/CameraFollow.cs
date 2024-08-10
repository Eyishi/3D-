using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//摄像机跟随
public class CameraFollow : MonoBehaviour
{
    public Transform camTarget;

    public float startDelay;
    public float height ;
    public float distance ;
    public float rotationDamping ;//旋转的阻尼
    public float heightDamping ;//高度的阻尼

    
    float originalRotationDamping;
    bool canSwitch;
    
    void Start(){
        
        originalRotationDamping = rotationDamping;
   
        rotationDamping = 0.1f;
        
        StartCoroutine(SwitchAngle());
    }
    void Update()
    {
        //
        if ((Input.GetMouseButtonDown(0) || Input.GetAxis("Horizontal") != 0) && rotationDamping == 0.1f && canSwitch)
            rotationDamping = originalRotationDamping;
    }

    void LateUpdate()
    {
        if (!camTarget )
        {
            return;
        }
        //取一些值，将要旋转和定位的值
        float wantedRotationAngle = camTarget.eulerAngles.y;
        float wantedHeight = camTarget.position.y + height;
        float currentRotaionAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;
        
        //插值      
        currentRotaionAngle = Mathf.LerpAngle(currentRotaionAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotaionAngle, 0);
        
        //第一步把摄像机移动到被观察者位置
        transform.position = camTarget.position;
        //第二步在被观察者位置的基础上向后面移动
        transform.position -= currentRotation * Vector3.forward * distance;
        //设置 相机的高度
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        
        //摄像机看向被观察的位置
        transform.LookAt(camTarget);
    }
    IEnumerator SwitchAngle(){
        //转换旋转阻尼
        yield return new WaitForSeconds(startDelay);
		
        canSwitch = true;
    }
}
