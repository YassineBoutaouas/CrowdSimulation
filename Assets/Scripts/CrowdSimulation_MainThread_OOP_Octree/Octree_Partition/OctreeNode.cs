using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Octree_Hollistic
{
    public class OctreeNode
    {
        private Bounds _bounds;
        private float _minSize;

        private Bounds[] _childBounds;

        private OctreeNode[] _children;

        public OctreeNode(Bounds b, float minSize)
        {
            _bounds = b;
            _minSize = minSize;

            float quater = _bounds.size.y / 4f;
            float childLength = _bounds.size.y / 2f;

            Vector3 childSize = Vector3.one * childLength;
            _childBounds = new Bounds[8];

            _childBounds[0] = new Bounds(_bounds.center + new Vector3(-quater, quater, -quater), childSize);
            _childBounds[1] = new Bounds(_bounds.center + new Vector3(quater, quater, -quater), childSize);
            _childBounds[2] = new Bounds(_bounds.center + new Vector3(-quater, quater, quater), childSize);
            _childBounds[3] = new Bounds(_bounds.center + new Vector3(quater, quater, quater), childSize);

            _childBounds[4] = new Bounds(_bounds.center + new Vector3(-quater, -quater, -quater), childSize);
            _childBounds[5] = new Bounds(_bounds.center + new Vector3(quater, -quater, -quater), childSize);
            _childBounds[6] = new Bounds(_bounds.center + new Vector3(-quater, -quater, quater), childSize);
            _childBounds[7] = new Bounds(_bounds.center + new Vector3(quater, -quater, quater), childSize);
        }

        public void AddObject(GameObject g)
        {
            DivideAndAdd(g);
        }

        public void DivideAndAdd(GameObject g)
        {
            if (_bounds.size.y <= _minSize) return;

            if (_children == null)
                _children = new OctreeNode[8];

            bool dividing = false;

            for (int i = 0; i < 8; i++)
            {
                if (_children[i] == null)
                {
                    _children[i] = new OctreeNode(_childBounds[i], _minSize);
                }

                if (_childBounds[i].Intersects(g.gameObject.GetComponent<Collider>().bounds))
                {
                    dividing = true;
                    _children[i].DivideAndAdd(g);
                }
            }

            if(dividing == false){
                _children = null;
            }
        }

        public void Draw()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);

            Gizmos.color = Color.red * 0.2f;
            Gizmos.DrawCube(_bounds.center, _bounds.size);

            if(_children != null){
                for(int i = 0; i < 8; i++){
                    if(_children[i] != null)
                        _children[i].Draw();
                }
            }
        }
    }
}