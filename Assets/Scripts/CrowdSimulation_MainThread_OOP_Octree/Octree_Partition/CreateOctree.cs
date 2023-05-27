using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Octree_Hollistic
{
    public class CreateOctree : MonoBehaviour
    {
        public List<GameObject> worldObjects;
        public int MinSize = 5;

        [Header("Cubicles"), Space()]
        public int SpawnCount = 20;
        public Vector2 SpawnBounds;

        private Octree _octree;

        private void Start()
        {
            _octree = new Octree(worldObjects.ToArray(), MinSize);
        }

        public void SpawnCubes()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                float x = Random.Range(-SpawnBounds.x, SpawnBounds.x) + transform.position.x;
                float y = Random.Range(-SpawnBounds.y, SpawnBounds.y) + transform.position.y;
                float z = Random.Range(-SpawnBounds.x, SpawnBounds.y) + transform.position.z;

                float size = Random.Range(3f, 8f);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.AddComponent<BoxCollider>();
                worldObjects.Add(cube);
                cube.transform.position = new Vector3(x, y, z);
                cube.transform.localScale = Vector3.one * size;
            }
        }

        public void DestroyCubes()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                DestroyImmediate(worldObjects[i]);
            }
            worldObjects.Clear();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            _octree.RootNode.Draw();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CreateOctree))]
    public class CreateOctreeEditor : UnityEditor.Editor
    {
        private CreateOctree _octreeCreator;

        private void OnEnable()
        {
            _octreeCreator = target as CreateOctree;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Spawn Cubes"))
                _octreeCreator.SpawnCubes();

            if (GUILayout.Button("Delete Cubes"))
                _octreeCreator.DestroyCubes();
        }
    }
#endif
}