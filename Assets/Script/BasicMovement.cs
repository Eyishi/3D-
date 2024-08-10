using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  /// <summary>
  /// 挂在地图上  （汽车旋转的效果是因为圆柱体随着汽车旋转 ）  或灯光上
  /// </summary>
public class BasicMovement : MonoBehaviour
{
    public float moveSpeed = 30.0f;

    public float rotateSpeed = 30.0f;

    private Car car;

    private Transform carTransform;

    private WorldGenerator generator;

    public bool lamp;//灯光
    // Start is called before the first frame update
    void Start()
    {
        car = GameObject.FindObjectOfType<Car>();
        generator = GameObject.FindObjectOfType<WorldGenerator>();
        if (car != null)
        {
            carTransform = car.transform;
        }
    }


    void Update()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        if (car != null)
        {
            CheckRotation();
        }
    }

    private void CheckRotation()
    {
        //不同的物体旋转不同
        Vector3 direction = (lamp) ? Vector3.right : Vector3.forward;
        float carRotation = carTransform.localEulerAngles.y;
        if(carRotation > car.rotationAngle * 2f)
            carRotation = (360 - carRotation) * -1f;

        //根据方向值、速度值、汽车旋转和世界尺寸旋转该对象
        transform.Rotate(direction * (-rotateSpeed * (carRotation/(float)car.rotationAngle) * 
                         (36f/generator.dimensions.x) * Time.deltaTime));
    }
}
