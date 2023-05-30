using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdSimulation_OT_OOP;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Octree_Points
{
    public class OctreeInstance : MonoBehaviour
    {
        public FlockAgent[] neighbors;

        public PointOctree pointOctree;

        public float perceptionRadius = 5f;

        public void Init(PointOctree octree)
        {
            pointOctree = octree;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, perceptionRadius);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OctreeInstance)), CanEditMultipleObjects()]
    public class OctreeInstanceEditor : Editor
    {
        private OctreeInstance instance;

        private void OnEnable()
        {
            instance = target as OctreeInstance;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //if (GUILayout.Button("Get neighbors"))
            //{
            //    instance.neighbors = instance.pointOctree.GetNearbyToArray(, instance.transform.position, instance.perceptionRadius);
            //}
//
            //for (int i = 0; i < instance.neighbors.Length; i++)
            //{
            //    Debug.DrawLine(instance.transform.position, instance.neighbors[i].transform.position, Color.red);
            //}
        }
    }
#endif
}