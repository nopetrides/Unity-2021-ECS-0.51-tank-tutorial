using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct QuadrantData
{
    public Entity entity;
    public float3 position;
    public QuadrantTag quadrantTag;
    public float allowedDistance;
}

[BurstCompile]
public partial class QuadrantSystem : SystemBase
{
    public static NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
    //public static NativeParallelMultiHashMap<int,QuadrantData> QuadrantMultiHashMap => quadrantMultiHashMap;

    private const int quadrantYMultiplier = 1000;
    private const int quadrantCellSize = 10;
    Plane plane = new Plane(Vector3.up, 0);

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    private static void DebugDrawQuadrant(float3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, 0, math.floor(position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(1,0,0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(0,0, 1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(1, 0, 0) * quadrantCellSize, lowerLeft + new Vector3(1, 0, 1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(0, 0, 1) * quadrantCellSize, lowerLeft + new Vector3(1, 0, 1) * quadrantCellSize);
        Debug.DrawLine(position, Camera.main.ScreenPointToRay(Input.mousePosition).origin);

    }

    private static int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey)
    {
        int count = 0;
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out QuadrantData quadrant, out NativeParallelMultiHashMapIterator<int> iterator))
        {
            do { count++; } while (quadrantMultiHashMap.TryGetNextValue(out quadrant, ref iterator));
        }
        return count;
    }

    [BurstCompile]
    private partial struct SetQuadrantDataHashMapJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMapJob;

        public void Execute(
          [EntityInQueryIndex] int index,
          in Translation translation, in Entity entity, in QuadrantTag tag, in MaxDistanceComp maxDistance)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMapJob.Add(hashMapKey, new QuadrantData
            {
                entity = entity,
                position = translation.Value,
                quadrantTag = tag,
                allowedDistance = maxDistance.allowedDistance
            });
        }
    }

    protected override void OnCreate()
    {
        quadrantMultiHashMap = new NativeParallelMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        quadrantMultiHashMap.Dispose();
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantTag));
        quadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
        {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }
        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMapJob = quadrantMultiHashMap,
        };
        setQuadrantDataHashMapJob.Run();

        /*Entities.ForEach((ref Translation translation, ref Entity entity) =>
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, entity);

        }).Run();*/


        // debug
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            var fieldPoint = ray.GetPoint(distance);
            DebugDrawQuadrant(ray.GetPoint(distance));
            Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(ray.GetPoint(distance))) + " " + ray.GetPoint(distance));
        }

    }
}
