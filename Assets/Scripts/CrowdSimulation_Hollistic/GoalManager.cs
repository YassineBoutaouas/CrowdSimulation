using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hollistic_Crowd
{
    public class GoalManager : MonoBehaviour
    {
        public static GoalManager Instance;

        public List<GameObject> Goals = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
        }
    }
}