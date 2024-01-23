using System.Collections.Generic;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{
    #region 变量

    [Header("鱼群数据")][SerializeField] private FlockData data;
    
    private readonly List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private readonly List<FlockUnit> avoidanceNeighbours = new List<FlockUnit>();
    private readonly List<FlockUnit> aligementNeighbours = new List<FlockUnit>();
    private FlockPool assignedFlock;
    private Vector3 currentVelocity;
    private Vector3 currentObstacleVelocity;
    private float speed;
    
    public Transform ThisTransform { get; set; }

    #endregion

    private void Awake()
    {
        ThisTransform = transform;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="newSpeed">新速度</param>
    public void InitializeSpeed(float newSpeed)
    {
        this.speed = newSpeed;
    }

    /// <summary>
    /// 设置父脚本
    /// </summary>
    /// <param name="flock">脚本</param>
    public void AssignFlock(FlockPool flock)
    {
        assignedFlock = flock;
    }
    
    /// <summary>
    /// 移动
    /// </summary>
    public void MoveUnit()
    {
        FindNeighbours();
        CalculateSpeed();
        
        // 计算向量和添加权重
        var cohesionVector = CalculateCohesionVector() * assignedFlock.CohesionWeight;
        var avoidanceVector = CalculateAvoidanceVector() * assignedFlock.AvoidanceWeight;
        var aligementVector = CalculateAligementVector() * assignedFlock.AlignmentWeight;
        var boundsVector = CalculateBoundsVector() * assignedFlock.BoundsWeight;
        var obstacleVector = CalculateObstacleVector() * assignedFlock.ObstacleWeight;
        
        // 计算移动向量
        var moveVector = cohesionVector + avoidanceVector + aligementVector + boundsVector + obstacleVector;
        moveVector = Vector3.SmoothDamp(ThisTransform.forward, moveVector, ref currentVelocity,data.SmoothDamp);
        moveVector = moveVector.normalized * speed;
        
        ThisTransform.forward = moveVector;
        ThisTransform.position += moveVector * Time.deltaTime; 
    }

    #region 向量计算

    /// <summary>
    /// 计算速度
    /// </summary>
    private void CalculateSpeed()
    {
        if(cohesionNeighbours.Count == 0)
            return;
        
        speed = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            speed += cohesionNeighbours[i].speed;
        }
        speed /= cohesionNeighbours.Count;
        speed = Mathf.Clamp(speed, assignedFlock.MinSpeed, assignedFlock.MaxSpeed);
    }
    
    /// <summary>
    /// 寻找物体
    /// </summary>
    private void FindNeighbours()
    {
        cohesionNeighbours.Clear();
        avoidanceNeighbours.Clear();
        aligementNeighbours.Clear();
        
        var allUnits = assignedFlock.AllUnits;
        for (int i = 0; i < allUnits.Length; i++)
        {
            var currentUnit = allUnits[i];
            if (currentUnit == this) continue;
            var currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - ThisTransform.position);
            if (currentNeighbourDistanceSqr <= assignedFlock.CohesionDistance * assignedFlock.CohesionDistance)
            {
                cohesionNeighbours.Add(currentUnit);
            }
            if(currentNeighbourDistanceSqr <= assignedFlock.AvoidanceDistance * assignedFlock.AvoidanceDistance)
                avoidanceNeighbours.Add(currentUnit);
            if(currentNeighbourDistanceSqr <= assignedFlock.AligementDistance * assignedFlock.AligementDistance)
                aligementNeighbours.Add(currentUnit);
        }
    }

    /// <summary>
    /// 计算聚合向量
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateCohesionVector()
    {
        var cohesionVector = Vector3.zero;
        if (cohesionNeighbours.Count == 0)
            return cohesionVector;

        var neighbourInFOV = 0;
        for(var i = 0; i< cohesionNeighbours.Count; i++)
        {
            if (IsInFOV(cohesionNeighbours[i].ThisTransform.position))
            {
                neighbourInFOV++;
                cohesionVector += cohesionNeighbours[i].ThisTransform.position;
            }
        }
        
        cohesionVector /= neighbourInFOV;
        cohesionVector -= ThisTransform.position;
        cohesionVector = Vector3.Normalize(cohesionVector);
        return cohesionVector;
    }
    
    /// <summary>
    /// 计算对齐向量
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateAligementVector()
    {
        var aligementVector = ThisTransform.forward;
        if(aligementNeighbours.Count == 0)
            return aligementVector;
        var neighboursInFOV = 0;
        for (int i = 0; i < aligementNeighbours.Count; i++)
        {
            if (IsInFOV(aligementNeighbours[i].ThisTransform.forward))
            {
                neighboursInFOV++;
                aligementVector += aligementNeighbours[i].ThisTransform.forward;
            }
        }
        
        aligementVector /= neighboursInFOV;
        aligementVector = Vector3.Normalize(aligementVector);
        aligementVector = aligementVector.normalized;

        return aligementVector;
    }

    /// <summary>
    /// 计算躲避向量
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = ThisTransform.forward;
        if(avoidanceNeighbours.Count == 0)
            return avoidanceVector;
        var neighboursInFOV = 0;
        for (int i = 0; i < avoidanceNeighbours.Count; i++)
        {
            if (IsInFOV(avoidanceNeighbours[i].ThisTransform.forward))
            {
                neighboursInFOV++;
                avoidanceVector += (ThisTransform.position - avoidanceNeighbours[i].ThisTransform.position); 
            }
        }

        if (neighboursInFOV == 0)
            return Vector3.zero;
        
        avoidanceVector /= neighboursInFOV;
        avoidanceVector = Vector3.Normalize(avoidanceVector);
        avoidanceVector = avoidanceVector.normalized;

        return avoidanceVector;
    }
    
    /// <summary>
    /// 计算边界向量
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateBoundsVector()
    {
        var centerOffset = assignedFlock.transform.position - ThisTransform.position;
        var isNearCenter = (centerOffset.magnitude >= assignedFlock.BoundsDistance * 0.9f);
        return isNearCenter ? centerOffset.normalized : Vector3.zero;
    }
    
    /// <summary>
    ///  计算障碍物向量
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateObstacleVector()
    {
        var obstacleVector = Vector3.zero;
        RaycastHit hit;
        if(Physics.Raycast(ThisTransform. position, ThisTransform.forward, out hit, assignedFlock.ObstacleDistance,data.ObstacleLayer))
        {
            obstacleVector = FindBestDirectionToAvoidObstacle();
        }
        else
        {
            currentObstacleVelocity = Vector3.zero;
        }

        return obstacleVector;
    }
    
    /// <summary>
    /// 寻找最佳躲避方法
    /// </summary>
    /// <returns></returns>
    private Vector3 FindBestDirectionToAvoidObstacle()
    {
        if (currentObstacleVelocity != Vector3.zero)
        {
            RaycastHit hit;
            if (Physics.Raycast(ThisTransform.position, ThisTransform.forward, out hit, assignedFlock.ObstacleDistance,
                    data.ObstacleLayer))
            {
                return currentObstacleVelocity;
            }
        }
        float maxDistance = int.MaxValue;
        var selectDirection = Vector3.zero;
        for (int i = 0; i < data.ObstacleDirections.Length; i++)
        {
            RaycastHit hit;
            var currentDirection = ThisTransform.TransformDirection(data.ObstacleDirections[i].normalized);
            if(Physics.Raycast(ThisTransform.position, currentDirection, out hit, assignedFlock.ObstacleDistance,data.ObstacleLayer))
            {
                float currentDistance = (hit.point - ThisTransform.position).sqrMagnitude;
                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    selectDirection = currentDirection;
                }
            }
            else
            {
                selectDirection = currentDirection;
                currentObstacleVelocity = currentDirection.normalized;
                return selectDirection.normalized;
            }
        }

        return selectDirection.normalized;
    }

    #endregion
    
    private bool IsInFOV(Vector3 pos)
    {
        return Vector3.Angle(ThisTransform.forward,pos - ThisTransform.position) <= data.FOVAngle;
    }
}
