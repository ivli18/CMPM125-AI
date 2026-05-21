using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class GuardAgent : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private NavMeshAgent agent;
    [SerializeField] private List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void Update()
    {
        //Check if the agent is close to the current waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
        
        // Test code
        //if (Input.GetMouseButtonDown(0))
        //{
        //    MoveAgentToClickedPoint();
        //}
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    // Test method to move the agent to a point clicked by the user.
    private void MoveAgentToClickedPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            agent.SetDestination(hit.point);
        }
    }

    /*
    === STATES ===
    Roaming	no other states active	other state conditions met		
    Suspicious	player within sights	player out of sight for x duration OR reaches alerted state		works with stages? 0-100%
    Alerted	player stays within sight for x duration			
    Confused	guard suspicious for x duration/stage and player exits sight	brief delay, returns to roaming		
    */


}
