using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

partial struct UnitMoverSystem : ISystem
{

    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2.2f;
   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        unitMoverJob.ScheduleParallel();
        /*foreach(
            (RefRW<LocalTransform> localTransform, RefRO<UnitMover> unitMover, RefRW<PhysicsVelocity> physicsVelocity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<UnitMover>, RefRW<PhysicsVelocity>>())
        {
            
            float3 moveDirection = math.normalize(unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position);
            localTransform.ValueRW.Rotation = 
            math.slerp(localTransform.ValueRO.Rotation,
             quaternion.LookRotationSafe(moveDirection, math.up()), 
             unitMover.ValueRO.rotationSpeed * SystemAPI.Time.DeltaTime);
             
            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }*/
    }

}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;

    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = math.normalize(unitMover.targetPosition - localTransform.Position);
        float reachedTargetDistance = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.distance(localTransform.Position, unitMover.targetPosition) <= reachedTargetDistance)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        localTransform.Rotation = 
            math.slerp(localTransform.Rotation,
             quaternion.LookRotationSafe(moveDirection, math.up()), 
             unitMover.rotationSpeed * deltaTime);
             
        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
