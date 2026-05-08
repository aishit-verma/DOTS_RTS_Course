using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct FindTargetSystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        foreach((RefRW<FindTarget> findTarget, RefRO<LocalTransform> transform, RefRW<Target> target)
         in SystemAPI.Query<RefRW<FindTarget>, RefRO<LocalTransform>, RefRW<Target>>())
        {

            findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if(findTarget.ValueRO.timer >0)
            {
                continue;
            }
            findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

            distanceHitList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0
            };
            if(collisionWorld.OverlapSphere(transform.ValueRO.Position,
                 findTarget.ValueRO.range, ref distanceHitList, collisionFilter))
            {
                foreach(DistanceHit distanceHit in distanceHitList)
                {
                    if(!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity))
                    {
                        continue;
                    }
                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                    if(targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        // We found a target of the correct faction, we can stop looking for more targets
                        
                        target.ValueRW.targetEntity = distanceHit.Entity;
                        break;
                        
                    }
                }
            }
        }
    }

    
}
