using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnityEngine.Vector3 cameraForward = UnityEngine.Vector3.zero;
        if(Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        foreach((RefRW<LocalTransform> localTransform, RefRO<HealthBar> healthBar) 
        in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>())
        {
            
            LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            if(localTransform.ValueRO.Scale == 1f)
            {
                localTransform.ValueRW.Rotation = 
                parentLocalTransform.InverseTransformRotation(quaternion.LookRotationSafe(cameraForward, math.up()));
            }
            


            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

            if(!health.onHealthChanged)
            {
                continue;
            }

            float healthNormalized = (float)health.healthAmount / health.maxHealthAmount;

            if(healthNormalized == 1f)
            {
                localTransform.ValueRW.Scale = 0;
            }
            else
            {
                localTransform.ValueRW.Scale = 1;
            }

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix = 
            SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);

            
        }
    }

    
}
