using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    //地图的材质
    public Material meshMaterial;
    
    //横截面  和向里延申的尺寸 尺寸
    public Vector2 dimensions;

    public float perlinScale;//柏林噪声的系数
    public float offset;//偏移量   y=kx + b
    
    public float scale;//系数

    public float waveHeight;//波浪高度

    private GameObject[] pieces = new GameObject[2];//地图块，地图是按照  0 -1  -0 不断生成

    public float globalSpeed;
    
    public float randomness;
    
    public int startTransitionLength;
    
    public BasicMovement lampMovement;
    
    public int obstacleChanceAcceleration;

    public int showItemDistance;
    
    public float shadowHeight;
    
    private Vector3[] beginPoints;
    
    public GameObject[] obstacles;//障碍数组也可以自己制作新的障碍，加到数组里随机。
    public GameObject gate;// 门，也可以自己定制的去换
    public int startObstacleChance;//障碍生成概率，值越大意味着障碍越少，值越小意味着障碍越多。
    public int gateChance;//生成门的概率

    private GameObject currentCylinder;//车子当前跑的圆柱体

    void Start()
    {
        beginPoints = new Vector3[(int)dimensions.x + 1];
        
        for (int i = 0; i < 2; i++)
        {
            GenerateWorldPieces(i);
        }
    }

    private void LateUpdate()
    {
        if (pieces[1] && pieces[1].transform.position.z <=0)
        {
            StartCoroutine(UpdateWorldPieces());
        }
        //更新 场景上的 门 或障碍物
        UpdateAllItems();
    }

    //更新一下地图快
    IEnumerator UpdateWorldPieces()
    {
        //第一个已经过去
        Destroy(pieces[0]);
        //把当前片段往前串一段
        pieces[0] = pieces[1];
        //再重新生成一个
        pieces[1] = CreateCylinder();
        
        //设置新的块的位置
        pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
        pieces[1].transform.rotation = pieces[0].transform.rotation;
        
        UpdateSinglePieces(pieces[1]);
        yield return 0;
    }
    
    //生成地图快
    private void GenerateWorldPieces(int i)
    {
        //生成圆柱体，保存数组
        pieces[i] = CreateCylinder();
        //根据索引，摆正圆柱体的位置
        pieces[i].transform.Translate(Vector3.forward * (dimensions.y * scale *  Mathf.PI) * i);
        
        //标记尾部的位置，移动调用
        UpdateSinglePieces(pieces[i]);
    }

    //给地图快设置参数
    void UpdateSinglePieces(GameObject piece)
    {
        //增加移动
        BasicMovement movement = piece.AddComponent<BasicMovement>();
        movement.moveSpeed = -globalSpeed;
        
        //将转速设置为  光照的转速 
        if(lampMovement != null)
            movement.rotateSpeed = lampMovement.rotateSpeed;
        
        //创建结束点,并且设置他的位置
        GameObject endPoint = new GameObject();
        endPoint.transform.position = piece.transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
        endPoint.transform.parent = piece.transform;
        endPoint.name = "End Point";
        
        //偏移这个圆柱
        offset += randomness;
        
        //障碍物  会   随着时间变化而增多
        if(startObstacleChance > 5)
            startObstacleChance -= obstacleChanceAcceleration;
    }
    //生成一个圆柱体的对象  并且设置它的网格 碰撞器等
    private GameObject CreateCylinder()
    {
        //mesh通过网格绘制，
        //meshfilter持有mesh的引用
        //meshRender
        
        //圆柱体
        GameObject newCylinder = new GameObject();
        newCylinder.name = "World piece";
        currentCylinder = newCylinder;//当前使用的圆柱体
        
        //添加meshfilter meshRender组件
        MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();
        
        //设置  材质 和  网格
        meshRenderer.material = meshMaterial;
        meshFilter.mesh = Generate();
        
        //创建网格后，添加网格碰撞器，适配新的网格
        newCylinder.AddComponent<MeshCollider>();

        return newCylinder;
    }

    //生成地图的网格
    private Mesh Generate()
    {
        //网格
        Mesh mesh = new Mesh();
        mesh.name = "MESH";
        
        //需要uv，顶点，三角形等数据
        Vector3[] vertices = null;
        Vector2[] uvs = null;
        int[] triangles = null;
        
        //创建形状
        CreateShape(ref vertices, ref uvs, ref triangles);
        
        //赋值给mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles; 
        
        //从三角形和顶点重新计算网格的法线。
        mesh.RecalculateNormals();
        return mesh;
    }
    
    //创建顶点  uv  三角形v
    private void CreateShape(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
    {
        //向z轴立面延申，x是横截面
        int xCount = (int)dimensions.x;
        int zCount = (int)dimensions.y;
        
        //通过定义的尺寸  初始化顶点和uv
        vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        uvs = new Vector2[(xCount + 1) * (zCount + 1)];
        
        //半径
        float radius = xCount * scale * 0.5f;

        int index = 0;
        
        //设置顶点和uv      xCount 表示横截面  为什么是两层for？  因为  z表示深度是延申的，所以需要不断向前加
        for (int x = 0; x <= xCount; x++)
        {
            for (int z = 0; z <= zCount; z++)
            {
                //根据x的位置获取当前位置的角度
                float angle = x * Mathf.PI * 2f / xCount;  
                //通过不同的角度计算  每个顶点的位置  
                vertices[index] = new Vector3(Mathf.Cos(angle) * radius,Mathf.Sin(angle) * radius,z * scale * Mathf.PI);
                
                //uv
                uvs[index]= new Vector2(x * scale, z * scale);
                
                //使用柏林噪声
                
                float pX = (vertices[index].x * perlinScale) + offset;//平滑度
                float pZ = (vertices[index].z * perlinScale) + offset;
                
                //需要一个中心点与当前顶点做减法然后归一化，再去计算柏林噪声
                Vector3 centor = new Vector3(0, 0, vertices[index].z);
               
                vertices[index] += (centor - vertices[index]).normalized * (Mathf.PerlinNoise(pX, pZ) * waveHeight);
                
                //用于解决      地图生成的时候拼接的问题
                if(z < startTransitionLength && beginPoints[0] != Vector3.zero)
                {
                    float perlinPercentage = z * (1f/startTransitionLength);
                    
                    Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
                    
                    vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
                }
                else if(z == zCount){
                    beginPoints[x] = vertices[index];
                }
                
                //随机生成障碍物
                if(Random.Range(0, startObstacleChance) == 0 && !(gate == null && obstacles.Length == 0))
                    CreateItem(vertices[index], x);
                index++;
            }
        }
        
        //初始化三角形数组(顶点索引)，x *  z 这样一个总数，1个矩形包含两个三角形，总共是6个顶点 
        triangles = new int[xCount * zCount * 6];
        
        //创建一个数组，6个三角形顶点，方便调用
        int[] boxBase = new int[6];
        
        //三角形用的索引
        int current = 0;
        for (int x = 0; x < xCount; x++)
        {
            //每次根据x的变化重新赋值
            boxBase = new int[]
            {
                x * (zCount + 1),
                x * (zCount + 1) + 1,
                (x + 1) * (zCount + 1),
                x * (zCount + 1) + 1,
                (x + 1) * (zCount + 1) + 1,
                (x + 1) * (zCount + 1),
            };
            //这里是计算横截面的 其中 一个向里面延申的 一个矩形
            for (int z = 0; z < zCount; z++)
            {
                //增长一下这个索引，方便计算下一个正方形
                for (int i = 0; i < 6; i++)
                {
                    boxBase[i] = boxBase[i] + 1;
                }
                //把6个顶点填充到triangle里面
                for (int j = 0; j < 6; j++)
                {
                    triangles[current + j] = boxBase[j] - 1;
                }

                current += 6;
            }
        }
    }
    //生成障碍物
    private void CreateItem(Vector3 vert, int x)
    {
        //圆柱体的中心
        Vector3 zCenter = new Vector3(0, 0, vert.z);

        //检测中心和垂直角度是否正确
        if(zCenter - vert == Vector3.zero || x == (int)dimensions.x/4 || x == (int)dimensions.x/4 * 3)
            return;

        //实例化一个对象  成为门 或障碍
        GameObject newItem = Instantiate((Random.Range(0, gateChance) == 0) ? 
            gate : 
            obstacles[Random.Range(0, obstacles.Length)]);

        //旋转到中心位置
        newItem.transform.rotation = Quaternion.LookRotation(zCenter - vert, Vector3.up);
        //物品放在垂直位置
        newItem.transform.position = vert;

        //设置这个对象为当前使用的圆柱的子物体
        newItem.transform.SetParent(currentCylinder.transform, false);
    }
    //获取当前所在的地图上
    public Transform GetWorldPiece()
    {
        return pieces[0].transform;
    }
    void UpdateAllItems(){
        //获取所有item
         GameObject[] items = GameObject.FindGameObjectsWithTag("item");
		      
         //遍历items
         for(int i = 0; i < items.Length; i++){
             //获取item上的  MeshRenderer
             foreach(MeshRenderer renderer in items[i].GetComponentsInChildren<MeshRenderer>()){
                 //距离 车子  近才显示
                 bool show = items[i].transform.position.z < showItemDistance;
				    
                 //只有在  圆柱体下方的 物体才能显示其阴影
                 if(show)
                     renderer.shadowCastingMode = (items[i].transform.position.y < shadowHeight) ?
                         ShadowCastingMode.On : 
                         ShadowCastingMode.Off;
                 renderer.enabled = show;
             }
         }
    }
}
