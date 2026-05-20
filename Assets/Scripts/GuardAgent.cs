using UnityEngine;
using UnityEngine.AI;
public class GuardAgent : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MoveAgentToClickedPoint();
        }
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
