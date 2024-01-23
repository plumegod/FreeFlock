using UnityEngine;
using Random = UnityEngine.Random;

public class FlockPool : MonoBehaviour
{
    #region 属性
    [SerializeField] private FlockUnit prefab; // 预制体
    [SerializeField] private Vector3 maxPos;
    [SerializeField] private Vector3 minPos;
    [SerializeField] private int poolSize = 10; // 初始对象池大小

    [Header("速度")]
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    
    [Header("距离")]
    [Range(0,10)][SerializeField] private float cohesionDistance;
    [Range(0,10)][SerializeField] private float avoidanceDistance;
    [Range(0,10)][SerializeField] private float aligementDistance;
    [Range(0,100)][SerializeField] private float boundsDistance;
    [Range(0,10)][SerializeField] private float obstacleDistance;
    
    
    [Header("权重")]
    [Range(0,10)][SerializeField] private float cohesionWeight;
    [Range(0,10)][SerializeField] private float avoidanceWeight;
    [Range(0,10)][SerializeField] private float alignmentWeight;
    [Range(0,100)][SerializeField] private float boundsWeight;
    [Range(0,100)][SerializeField] private float obstacleWeight;

    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;
    public float CohesionDistance => cohesionDistance;
    public float AvoidanceDistance => avoidanceDistance;
    public float AligementDistance => aligementDistance;
    public float BoundsDistance => boundsDistance;
    public float ObstacleDistance => obstacleDistance;
    public float CohesionWeight => cohesionWeight;
    public float AvoidanceWeight => avoidanceWeight;
    public float AlignmentWeight => alignmentWeight;
    public float ObstacleWeight => obstacleWeight;
    public float BoundsWeight => boundsWeight;
    
    public FlockUnit[] AllUnits { get; set; }

    private Transform tf;

    #endregion
    private void Start()
    {
        tf = this.transform;
        AllUnits = new FlockUnit[poolSize];
        for(var i = 0; i < poolSize; i++)
            CreateFlock(i);
    }

    private void Update()
    {
        foreach (var t in AllUnits)
        {
            t.MoveUnit();
        }
    }

    /// <summary>
    /// 生成预制体
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private FlockUnit CreateFlock(int i)
    {
        // 随机位置
        var randomPos = new Vector3(Random.Range(tf.position.x + maxPos.x, tf.position.x + minPos.x), Random.Range(tf.position.y + maxPos.x, tf.position.y + minPos.y),
            Random.Range(tf.position.z + maxPos.z, tf.position.z + minPos.z));
        // 随机旋转
        AllUnits[i] = Instantiate(prefab, randomPos,
            Quaternion.Euler
                (Random.Range(-90, 90), Random.Range(-90, 90), Random.Range(-90, 90)));
        // 设置父物体
        AllUnits[i].transform.SetParent(tf);
        // 设置父脚本
        AllUnits[i].AssignFlock(this);
        // 设置速度
        AllUnits[i].InitializeSpeed(Random.Range(minSpeed, maxSpeed));
        return AllUnits[i];
    }
}