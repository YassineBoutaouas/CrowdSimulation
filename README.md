# CrowdSimulation
 
This project compares different realtime crowd simulation approaches. The proposed behavioral model combines pathfinding with  [intrinsic flocking behaviors](https://doi.org/10.1145/37402.37406) (Cohesion, Alignment, Separation)

It considers a spatial partitioning system ([Octree Implementation](https://github.com/Nition/UnityOctree)), GPU accelerated neighbor search, flow field pathfinding and Unity ECS to improve the overall performance of the crowd simulation.

The project recognizes the neighbor search, the pathfinding and the rendering task to be the most performance intensive in the process.

## Future Considerations

The realism of the simulation may benefit greatly from the inclusion of a proper collision avoidance algorithm such as the [RVO](https://doi.org/10.1109/ROBOT.2008.4543489) as well as a stopping algorithm when reaching a shared target.
Optimization approaches such as a shader accelerated neighbor search as well as a spatial partitioning system are applied to the OOP approach and may be considered for the DOTS approach.

## Script Directories

- OOP - Bruteforce Approach         .../Assets/Scripts/CrowdSimulation_MainThread_OOP
- OOP - Octree Approach             .../Assets/Scripts/CrowdSimulation_MainThread_OOP_Octree
- OOP - Shader Accelerated Approach .../Assets/Scripts/CrowdSimulation_Shader
- OOP - Flow Field Approach         .../Assets/Scripts/CrowdSimulation_FieldBased_OOP
- DOTS - ECS Approach               .../Assets/Scripts/CrowdSimulation_FieldBased_DOTS

## Scene

- Directory: .../Assets/Scenes/SampleScene
- To play a simulation activate the respective game object and press play. (SampleScene/Solutions/Tests/Crowds_(...))
- To play the DOTS simulation an additional game object named FlockSpawner has to be activated (located in the subscene FlowField_DOTS) 

## DOTS program architecture

![alt text](https://github.com/YassineBoutaouas/CrowdSimulation/blob/main/ProgramArchitecture.png?raw=true)