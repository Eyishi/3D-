using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class Car : MonoBehaviour
{
    public Transform[] wheelMeshes;//四个轮子

    public WheelCollider[] wheelColliders;//轮子碰撞器

    public int rotateSpeed;//汽车转动速度

    public int rotationAngle;//汽车左右 旋转角度

    public int wheelRotateSpeed;//轮子的转动速度

    public Rigidbody rb;//小车刚体
    public Transform back;//给汽车的力
    public float constantBackForce;//摩檫力的参数
    

    
    public float skidMarkSize;//打滑的大小
    public GameObject skidMark;//打滑特效
    
    public float skidMarkDelay; //检测打滑的  时间
    
    public Transform[] grassEffects;//粒子特效  
    public Transform[] skidMarkPivots;//打滑的点
    
    
    public float grassEffectOffset;//偏移量，用来检测轮子到草地的最大距离

    public float minRotationDifference;//汽车判定为旋转的最小角度
    
    public GameObject ragdoll;
    
    private int targetRotation; //汽车左右旋转的角度
    private float lastRotation;//上一帧的角度
    private bool skidMarkRoutine;//检测是否应该有痕迹
    private WorldGenerator generator; //获取生成的地图

    // public float lightTime;//灯的时间
    // private float curtime;//当前灯
    public Light leftCarLight;//左边车灯

    public Light rightCarLight;//右边车灯
    void Start()
    {
        // leftCarLight.enabled = false;
        // rightCarLight.enabled = false;
        generator = GameObject.FindObjectOfType<WorldGenerator>();
        StartCoroutine(SkidMark());//异步线程观察是否应该有打滑
    }
   

    //小车破碎
    public void FallApart(){
        //位置
        Instantiate(ragdoll, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }
    
    //检测打滑
    IEnumerator SkidMark()
    {
        while (true)
        {
            yield return new WaitForSeconds(skidMarkDelay);
            if (skidMarkRoutine)
            {
                for (int i = 0; i < skidMarkPivots.Length; i++)
                {
                    //汽车轮胎的划痕
                    GameObject newskidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
                    Destroy(newskidMark,1);
                    newskidMark.transform.parent = generator.GetWorldPiece();
                    newskidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
                }    
            }
        }
    }

    private void FixedUpdate()
    {
        //更新车轮痕迹和粒子特效
        UpdateEffects();
    }

    /// <summary>
    /// 更新特效
    /// </summary>
    private void UpdateEffects()
    {
        //轮子在地面上，不加力，不在加上力
        bool addForcd = true;
        //判断是不是在旋转,上一帧的角度减去当前的角度
        bool rotated = Mathf.Abs(lastRotation - transform.rotation.y) > minRotationDifference;
        for (int i = 0; i < 2; i++)
        {
            //获取车轮的位置
            Transform wheelMesh =  wheelMeshes[i + 2];//表示车的后两个轮子
            
            //物理射线检测车子是否在地面上
            if (Physics.Raycast(wheelMesh.position,Vector3.down,grassEffectOffset * 1.5f))
            {
                //如果粒子特效没有显示，让他显示
                if (!grassEffects[i].gameObject.activeSelf)
                {
                    grassEffects[i].gameObject.SetActive(true);
                }
                //更新粒子的高度还有痕迹的位置
                float effectHigh = wheelMesh.position.y - grassEffectOffset ;
                Vector3 targetPostion = new Vector3(grassEffects[i].position.x, effectHigh, wheelMesh.position.z);
                grassEffects[i].position = targetPostion;
                //skidMarkPivots[i].position = targetPostion;

                addForcd = false;
            }
            //轮胎不在地面上，应该不有粒子特效
            else if (grassEffects[i].gameObject.activeSelf)
            {
                grassEffects[i].gameObject.SetActive(false);
            }
        }

        //不在地面上
        if (addForcd) 
        {
            rb.AddForceAtPosition(back.position,Vector3.down * constantBackForce);
            skidMarkRoutine = false;
        }
        else
        {
            //转弯了
            if (targetRotation !=0)
            {
                if (rotated && !skidMarkRoutine)//旋转并且没有添加打滑
                {
                    skidMarkRoutine = true;
                }
                else if (!rotated && skidMarkRoutine)//没旋转并且打滑
                {
                    skidMarkRoutine = false;
                }
            }
            else
            {
                //直走不需要
                skidMarkRoutine = false;
            }
        }

        lastRotation = transform.localEulerAngles.y;
    }
    
    void LateUpdate()
    {
        //控制轮子旋转
        for (int i = 0; i < wheelMeshes.Length; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos,out quat);
            wheelMeshes[i].position = pos;
            wheelMeshes[i].Rotate(Vector3.right * (Time.deltaTime * wheelRotateSpeed) );
        }
        //汽车的转向
        if (Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0)
        {
            UpdateTargetRotation();
        }
        //没有输入
        else if (targetRotation !=0 )
        {
            targetRotation = 0;
        }
        //旋转汽车
        Vector3 rotation = new Vector3(transform.localRotation.x, targetRotation, transform.localRotation.z);
        //旋转到指定角度
        transform.rotation = 
            Quaternion.RotateTowards(transform.rotation,Quaternion.Euler(rotation),
                rotateSpeed * Time.deltaTime);
    }
    //通过输入  更新车的角度
    void UpdateTargetRotation()
    {
        if ( Input.GetAxis("Horizontal") == 0)
        {
            //鼠标在屏幕右边
            if (Input.mousePosition.x > Screen.width*0.5f)
            {
                //右转
                targetRotation = rotationAngle;
            }
            else
            {
                //左转
                targetRotation = - rotationAngle;
            }
        }
        else
        {
            // A  D    
            targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
        }
    }
}
