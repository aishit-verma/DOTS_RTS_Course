using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    private Vector2 selectionStartMousePosition;

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;
            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);
            }




            Rect selectionAreaRect = GetSelectionAreRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionThreshold = 40f;
            if (selectionAreaSize > multipleSelectionThreshold)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform localTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }

                }
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex = 0
                    }
                };
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
       
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }

            }


        }
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);
            for (int i = 0; i < entityArray.Length; i++)
            {

                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = movePositionArray[i];
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }
    public Rect GetSelectionAreRect()
    {
        Vector2 selectionEndMousePostition = Input.mousePosition;
        float x = Mathf.Min(selectionEndMousePostition.x, selectionStartMousePosition.x);
        float y = Mathf.Min(selectionEndMousePostition.y, selectionStartMousePosition.y);
        float width = Mathf.Abs(selectionEndMousePostition.x - selectionStartMousePosition.x);
        float height = Mathf.Abs(selectionEndMousePostition.y - selectionStartMousePosition.y);

        return new Rect(x, y, width, height);
    }
    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        if(positionCount == 0)
        {
            return positionArray;
        }
        positionArray[0] = targetPosition;
        if(positionCount == 1)
        {
            return positionArray;
        }
        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while(positionIndex < positionCount)
        {
            
           
            int ringPositionCount = 3 + ring*2;
            for(int i = 0; i < ringPositionCount; i++)
            {
                float angle = (float)i * (math.PI2/ ringPositionCount );
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0 ));
                float3 ringPosition = targetPosition + ringVector;
                positionArray[positionIndex] = ringPosition;
                positionIndex++;
                if(positionIndex >= positionCount)
                {
                    break;
                }
            }
            ring++;
        }

        return positionArray;
    }
}
