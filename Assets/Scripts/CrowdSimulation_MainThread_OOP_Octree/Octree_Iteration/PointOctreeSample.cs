using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Octree_Points;
using Octree_Bounds;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PointOctreeSample : MonoBehaviour
{
    // public PointOctree<GameObject> pointOctree;

    // public float Size = 20f;

    // public int SpawnCount = 100;
    // public Vector2 SpawnBounds;
    // public List<GameObject> worldObjects;

    // private void Start()
    // {
    //     pointOctree = new PointOctree<GameObject>(Size, transform.position, 1f);

    //     for (int i = 0; i < worldObjects.Count; i++)
    //     {

    //         worldObjects[i].GetComponent<OctreeInstance>().Init(pointOctree);
    //         pointOctree.Add(worldObjects[i], worldObjects[i].transform.position);
    //         //boundsOctree.Add(worldObjects[i], worldObjects[i].GetComponent<Collider>().bounds);
    //     }
    // }

    // public void SpawnCubes()
    // {
    //     for (int i = 0; i < SpawnCount; i++)
    //     {
    //         float x = Random.Range(-SpawnBounds.x, SpawnBounds.x) + transform.position.x;
    //         float y = Random.Range(-SpawnBounds.y, SpawnBounds.y) + transform.position.y;
    //         float z = Random.Range(-SpawnBounds.x, SpawnBounds.y) + transform.position.z;

    //         float size = Random.Range(3f, 8f);

    //         GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //         cube.AddComponent<BoxCollider>();
    //         cube.AddComponent<OctreeInstance>();
    //         worldObjects.Add(cube);
    //         cube.transform.position = new Vector3(x, y, z);
    //         cube.transform.localScale = Vector3.one * size;
    //     }
    // }

    // public void DestroyCubes()
    // {
    //     for (int i = 0; i < SpawnCount; i++)
    //     {
    //         DestroyImmediate(worldObjects[i]);
    //     }
    //     worldObjects.Clear();
    // }

    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying) return;

    //     pointOctree.DrawAllBounds();
    //     //boundsOctree.DrawAllBounds();
    // }
}

// #if UNITY_EDITOR

// [UnityEditor.CustomEditor(typeof(PointOctreeSample))]
// public class CreateOctreeEditor : UnityEditor.Editor
// {
//     private PointOctreeSample _octreeCreator;
    
//     GameObject[] neighbors;
//     float perceptionRadius = 5f;

//     private void OnEnable()
//     {
//         _octreeCreator = target as PointOctreeSample;
//     }

//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         EditorGUILayout.BeginHorizontal();
//         if (GUILayout.Button("Spawn Cubes"))
//             _octreeCreator.SpawnCubes();

//         if (GUILayout.Button("Delete Cubes"))
//             _octreeCreator.DestroyCubes();
//         EditorGUILayout.EndHorizontal();

//         EditorGUILayout.Space();
//     }
// }
// #endif