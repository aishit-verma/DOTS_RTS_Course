using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public int healthAmount;
    public int maxHealthAmount;
    
    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                healthAmount = authoring.healthAmount,
                maxHealthAmount = authoring.maxHealthAmount,
                onHealthChanged = true
            });
        }
    }
}
public struct Health : IComponentData
{
    public int healthAmount;
    public int maxHealthAmount;
    public bool onHealthChanged;
}
