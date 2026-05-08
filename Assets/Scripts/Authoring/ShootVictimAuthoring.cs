using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimAuthoring : MonoBehaviour
{
    public Transform hitLocalPositionTransform;
    public class Baker : Baker<ShootVictimAuthoring>
    {
        public override void Bake(ShootVictimAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {
                hitLocalPosition = authoring.hitLocalPositionTransform.localPosition,
            });
        }
    }
}
public struct ShootVictim : IComponentData
{
    public float3 hitLocalPosition;
}
