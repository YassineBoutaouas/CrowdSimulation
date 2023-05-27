using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Octree_Hollistic
{
    public class Octree
    {
        public OctreeNode RootNode;

        public Octree(GameObject[] objects, float minSize)
        {
            Bounds bounds = new Bounds();
            foreach (GameObject g in objects)
            {
                bounds.Encapsulate(g.GetComponent<Collider>().bounds);
            }

            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            Vector3 halfExtends = Vector3.one * maxSize * 0.5f;
            bounds.SetMinMax(bounds.center - halfExtends, bounds.center + halfExtends);

            RootNode = new OctreeNode(bounds, minSize);

            RegisterObjects(objects);
        }

        public void RegisterObjects(GameObject[] worldObjects)
        {
            foreach (GameObject obj in worldObjects)
                RootNode.AddObject(obj);
        }
    }
}