using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class RandomWalkingAuthoring : MonoBehaviour
{
    
    public float3 targetPosition;
    public float3 originPosition;
    public float distanceMin;
    public float distanceMax;
    public uint RandomSeed;

    public class Baker : Baker<RandomWalkingAuthoring>
    {
        public override void Bake(RandomWalkingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RandomWalking
            {
                targetPosition = authoring.targetPosition,
                originPosition = authoring.originPosition,
                distanceMin = authoring.distanceMin,
                distanceMax = authoring.distanceMax,
                random = new Unity.Mathematics.Random(authoring.RandomSeed)
            });
        }
    } 
}
public struct RandomWalking : IComponentData
{
    public float3 targetPosition;
    public float3 originPosition;
    public float distanceMin;
    public float distanceMax;
    public Unity.Mathematics.Random random;
}
