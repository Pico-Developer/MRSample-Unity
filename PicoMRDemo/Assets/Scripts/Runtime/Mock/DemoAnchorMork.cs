using System;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Decoration;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Mock;
using PicoMRDemo.Runtime.Utils;
using UnityEngine.Rendering;
public class DemoAnchorMork : MonoBehaviour
{
    public GameObject deviceSimulator;
    private LifetimeScope _rootLifetimeScope;
    public Material RoomEntityMaterial;
    public GameObject anchorPrefab;
    public GameObject descriptionRoot;
    private IObjectResolver Container => _rootLifetimeScope.Container;
    // Start is called before the first frame update
    async void Start()
    {
#if UNITY_EDITOR
        Instantiate(deviceSimulator);
        _rootLifetimeScope = LifetimeScope.Create(InitMorkEntitys);
        await InitAsync();
        ShowRoomEntities();
#endif
        //await InitDescription();
    }

    private async UniTask InitDescription()
    {
        if (Camera.main != null && descriptionRoot != null)
        {
            var cameraTransform = Camera.main.transform;
            var layoutRotation = Quaternion.Euler(0, -25, 0);
            var direction = layoutRotation * cameraTransform.forward;
            var position = cameraTransform.position;
            Vector3 pos = position + direction * 1f;
            pos.y = position.y;

            descriptionRoot.transform.position = pos;
            descriptionRoot.transform.rotation = Quaternion.LookRotation (descriptionRoot.transform.position - position);
        }
    }

    private void Update()
    {
        InitDescription();
    }

    private async UniTask InitAsync()
    {
        var entityManager = Container.Resolve<IEntityManager>();
        await entityManager.LoadRoomEntities();
    }
    private void InitMorkEntitys(IContainerBuilder builder)
    {
        builder.Register<IEntityManager, MockEntityManager>(Lifetime.Singleton);
        builder.Register<IPersistentLoader, PersistentLoader>(Lifetime.Singleton);
        
    }
    public void ShowRoomEntities()
    {
        var entityManager = Container.Resolve<IEntityManager>();
        var roomEntities = entityManager.GetRoomEntities();
        foreach (var roomEntity in roomEntities)
        {
            var label = roomEntity.GetRoomLabel();
            if (label == PxrSemanticLabel.Table || label == PxrSemanticLabel.Wall || label == PxrSemanticLabel.Ceiling
                || label == PxrSemanticLabel.Floor || label == PxrSemanticLabel.Door || label == PxrSemanticLabel.Window
                || label == PxrSemanticLabel.Sofa || label == PxrSemanticLabel.Opening || label == PxrSemanticLabel.Chair 
                || label == PxrSemanticLabel.VirtualWall || label == PxrSemanticLabel.Human)
            {
                var meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer == null)
                {
                    DrawRoomEntity(roomEntity);
                    meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                }
                meshRenderer.enabled = true;
            }
        }
    }
    private void DrawRoomEntity(IEntity entity)
    {
        var anchorObject = entity.GameObject;
        var anchorData = entity.AnchorData;
        var roomEntityMaterial = RoomEntityMaterial;
        if (anchorData.SceneLabel == PxrSemanticLabel.Table 
            || anchorData.SceneLabel == PxrSemanticLabel.Sofa 
            || anchorData.SceneLabel == PxrSemanticLabel.Chair
            || anchorData.SceneLabel == PxrSemanticLabel.Human)
        {
            var box3DInfo = entity.AnchorData.SceneBox3DData;
            var roomObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomObject.transform.parent = anchorObject.transform;
            var localPosition = box3DInfo.Center;
            roomObject.transform.localRotation = Quaternion.identity;
            roomObject.transform.localPosition = localPosition;
            roomObject.transform.localScale = box3DInfo.Extent;

            var meshRenderer = roomObject.GetComponent<MeshRenderer>();
            
            meshRenderer.material = roomEntityMaterial;
            roomObject.layer = 11;
            GameObject meshAnchor = Instantiate(anchorPrefab, roomObject.transform, true);
            meshAnchor.transform.localPosition = roomObject.transform.localPosition;
            Anchor anchor = meshAnchor.GetComponent<Anchor>();
            if(!anchor)
                meshAnchor.AddComponent<Anchor>();
            anchor.UpdateMeshLabel(anchorData.SceneLabel == PxrSemanticLabel.Table?"Table":"Sofa", roomObject.transform.position);
        }

        if (anchorData.SceneLabel == PxrSemanticLabel.Wall || anchorData.SceneLabel == PxrSemanticLabel.VirtualWall)
        {
            var box2DInfo = entity.AnchorData.SceneBox2DData;
            var extent = box2DInfo.Extent;
            var wall = MeshGenerator.GenerateQuadMesh(box2DInfo.Center, box2DInfo.Extent, roomEntityMaterial);
            wall.transform.parent = anchorObject.transform;
            wall.transform.localRotation = Quaternion.identity;
            wall.transform.localPosition = Vector3.zero;
            wall.transform.localScale = Vector3.one;
            wall.AddComponent<MeshCollider>();
            GameObject meshAnchor = Instantiate(anchorPrefab, wall.transform, true);
            meshAnchor.transform.localPosition = wall.transform.localPosition;
            Anchor anchor = meshAnchor.GetComponent<Anchor>();
            if(!anchor)
                meshAnchor.AddComponent<Anchor>();
            anchor.UpdateMeshLabel("Wall", wall.transform.position);
            wall.layer = 9;
            // generate skirting line
            var skirtingLine = MeshGenerator.GenerateSkirtingLine(box2DInfo.Center, box2DInfo.Extent, roomEntityMaterial);
            skirtingLine.transform.parent = anchorObject.transform;
            skirtingLine.transform.localRotation = Quaternion.identity;
            skirtingLine.transform.localPosition = Vector3.zero;
            skirtingLine.transform.localScale = Vector3.one;
            skirtingLine.AddComponent<MeshCollider>();
            skirtingLine.layer = 9;
        }

        if (anchorData.SceneLabel == PxrSemanticLabel.Ceiling || anchorData.SceneLabel == PxrSemanticLabel.Floor)
        {
            var scenePolygonInfo = entity.AnchorData.ScenePolygonData;
            var roomObject = MeshGenerator.GeneratePolygonMesh(scenePolygonInfo.Vertices, roomEntityMaterial);
            roomObject.transform.parent = anchorObject.transform;
            roomObject.transform.localRotation = Quaternion.identity;
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.transform.localScale = Vector3.one;
            GameObject meshAnchor = Instantiate(anchorPrefab, roomObject.transform, true);
            meshAnchor.transform.localPosition = roomObject.transform.localPosition;
            Anchor anchor = meshAnchor.GetComponent<Anchor>();
            if(!anchor)
                meshAnchor.AddComponent<Anchor>();
            anchor.UpdateMeshLabel(anchorData.SceneLabel == PxrSemanticLabel.Ceiling?"Ceiling":"Floor", roomObject.transform.position);
            var meshCollider = roomObject.AddComponent<MeshCollider>();
            meshCollider.convex = false;
            meshCollider.enabled = true;
            var boxCollider = roomObject.AddComponent<BoxCollider>();
            var oldSize = boxCollider.size;
            boxCollider.size = new Vector3(oldSize.x, oldSize.y, 0.02f);
            if (anchorData.SceneLabel == PxrSemanticLabel.Floor)
            {
                var oldCenter = boxCollider.center;
                boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, -0.01f);
            }
            else
            {
                var oldCenter = boxCollider.center;
                boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, 0.01f);
            }
            boxCollider.enabled = false;
            roomObject.AddComponent<MeshCollider>();
            if (anchorData.SceneLabel == PxrSemanticLabel.Floor)
            {
                roomObject.layer = 10;
                var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            else if (anchorData.SceneLabel == PxrSemanticLabel.Ceiling)
            {
                roomObject.layer = 8;
                var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        if (anchorData.SceneLabel == PxrSemanticLabel.Door 
            || anchorData.SceneLabel == PxrSemanticLabel.Window
            || anchorData.SceneLabel == PxrSemanticLabel.Opening)
        {
            var box2DInfo = entity.AnchorData.SceneBox2DData;
            var extent = box2DInfo.Extent;
            var doorOrWindow = MeshGenerator.GenerateQuadMesh(box2DInfo.Center, box2DInfo.Extent, roomEntityMaterial);
            doorOrWindow.transform.parent = anchorObject.transform;
            doorOrWindow.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            doorOrWindow.transform.localRotation = Quaternion.identity;
            var meshRenderer = doorOrWindow.GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = roomEntityMaterial;
            doorOrWindow.layer = 7;
            GameObject meshAnchor = Instantiate(anchorPrefab, doorOrWindow.transform, true);
            meshAnchor.transform.localPosition = doorOrWindow.transform.localPosition;
            Anchor anchor = meshAnchor.GetComponent<Anchor>();
            if(!anchor)
                meshAnchor.AddComponent<Anchor>();
            if (anchorData.SceneLabel == PxrSemanticLabel.Door)
            {
                anchor.UpdateMeshLabel("Door", doorOrWindow.transform.position);

            }
            else if (anchorData.SceneLabel == PxrSemanticLabel.Window)
            {
                anchor.UpdateMeshLabel("Window", doorOrWindow.transform.position);
            }
            else
            {
                anchor.UpdateMeshLabel("Opening", doorOrWindow.transform.position);
            }
        }
    }
}
