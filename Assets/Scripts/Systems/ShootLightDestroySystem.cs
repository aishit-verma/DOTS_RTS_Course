using Unity.Burst;
using Unity.Entities;

partial struct ShootLightDestroySystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = 
        SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRW<ShootLight> shootLight, Entity entity)
            in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
        {
            shootLight.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootLight.ValueRW.timer <= 0f)
            {
                ecb.DestroyEntity(entity);
            }
        }
    }

    
}
