using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FlockSpawner : MonoBehaviour
{
    private EntityManager _entityManager;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _entityManager.CreateEntity();
    }
}
