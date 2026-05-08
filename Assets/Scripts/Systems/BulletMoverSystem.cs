using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SocialPlatforms;

partial struct BulletMoverSystem : ISystem
{


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = 
        SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);



        foreach ((RefRW<LocalTransform> localTransform,
                RefRO<Bullet> bullet,
                RefRO<Target> target,
                Entity entity)
                in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
        {
            if(target.ValueRO.targetEntity == Entity.Null)
            {
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }
            LocalTransform targetLocalPosition = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            ShootVictim shootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetPosition = targetLocalPosition.TransformPoint(shootVictim.hitLocalPosition);

            float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetLocalPosition.Position);


            float3 moveDirection = math.normalize(targetLocalPosition.Position - localTransform.ValueRO.Position);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetLocalPosition.Position);

            if(distanceAfterSq > distanceBeforeSq)
            {
                localTransform.ValueRW.Position = targetLocalPosition.Position;
            }

            float destroyDistanceSq = 0.0002f;
            if (math.distancesq(localTransform.ValueRO.Position, targetLocalPosition.Position) < destroyDistanceSq)
            {
                RefRW<Health> health = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                health.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                health.ValueRW.onHealthChanged = true;

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }


}
