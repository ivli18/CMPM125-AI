using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum GuardState
{
    Roaming,
    Suspicious,
    Alerted,
    Confused
}
public class GuardAgent : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private NavMeshAgent agent;
    [Header("Patrol")]
    [SerializeField] private List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    [Header("Vision")]
    [SerializeField] private float coneRange = 10f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField] private float circleRadius = 2f;

    [Header("Detection")]
    [SerializeField] private float suspicionFillRate = 25f;
    [SerializeField] private float circleFillMult = 1.5f;
    [SerializeField] private float suspicionDrainRate = 15f;
    [SerializeField] private float confusedDuration = 3f;

    [Header("State Colors")]
    [SerializeField] private Color roamingColor = Color.green;
    [SerializeField] private Color suspiciousColor = Color.yellow;
    [SerializeField] private Color alertedColor = Color.red;
    [SerializeField] private Color confusedColor = Color.blue;

    // Internal
    private GuardState state = GuardState.Roaming;
    private Transform player;
    private Renderer rend;

    private float suspicionMeter = 0f;  // 0 to 100
    private float confusedTimer = 0f;
    private Vector3 lastKnownPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponent<Renderer>();
        player = GameObject.FindWithTag("Player").transform;

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        SetState(GuardState.Roaming);
    }

    private void Update()
    {
        bool canSee = CanSeePlayer();

        UpdateSuspicionMeter(canSee);
        UpdateStateTransitions(canSee);
        UpdateStateBehavior();
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

    // --- DETECTION ---
    private bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        bool inCone = distance <= coneRange &&
                      Vector3.Angle(transform.forward, toPlayer) <= coneAngle / 2f;
        bool inRadius = distance <= circleRadius;

        if (!inCone && !inRadius) return false;

        // Raycast — walls block vision
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, toPlayer.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, coneRange))
            return hit.collider.CompareTag("Player");

        return false;
    }
    private void UpdateSuspicionMeter(bool canSee)
    {
        if (canSee)
        {
            float rate = suspicionFillRate;
            if (IsInCloseRadius()) rate *= circleFillMult;
            suspicionMeter += rate * Time.deltaTime;
            suspicionMeter = Mathf.Clamp(suspicionMeter, 0f, 100f);
            lastKnownPosition = player.position;
        }
        else if (state == GuardState.Suspicious)
        {
            suspicionMeter -= suspicionDrainRate * Time.deltaTime;
        }
    }

    private bool IsInCloseRadius()
    {
        return Vector3.Distance(transform.position, player.position) <= circleRadius;
    }

    private void UpdateStateTransitions(bool canSee)
    {
        switch (state)
        {
            case GuardState.Roaming:
                if (suspicionMeter > 0) SetState(GuardState.Suspicious);
                break;

            case GuardState.Suspicious:
                if (suspicionMeter >= 100) SetState(GuardState.Alerted);
                else if (suspicionMeter <= 0) SetState(GuardState.Confused);
                break;

            case GuardState.Confused:
                confusedTimer -= Time.deltaTime;
                if (canSee) SetState(GuardState.Suspicious);
                else if (confusedTimer <= 0) SetState(GuardState.Roaming);
                break;

            case GuardState.Alerted:
                // Game over handled by GameManager
                break;
        }
    }

    /*
    === STATES ===
    Roaming	no other states active	other state conditions met		
    Suspicious	player within sights	player out of sight for x duration OR reaches alerted state		works with stages? 0-100%
    Alerted	player stays within sight for x duration			
    Confused	guard suspicious for x duration/stage and player exits sight	brief delay, returns to roaming		
    */

    private void UpdateStateBehavior()
    {
        switch (state)
        {
            case GuardState.Roaming:
                // Original patrol logic from your teammate
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextWaypoint();
                break;

            case GuardState.Suspicious:
                // Slowly move toward last known position
                agent.SetDestination(lastKnownPosition);
                break;

            case GuardState.Confused:
                // Stand at last known position, look around
                agent.SetDestination(lastKnownPosition);
                break;

            case GuardState.Alerted:
                // Actively chase
                agent.SetDestination(player.position);
                break;
        }
    }
     private void SetState(GuardState newState)
    {
        state = newState;

        switch (newState)
        {
            // currently walks to player location
            case GuardState.Roaming:
                rend.material.color = roamingColor;
                suspicionMeter = 0f;
                if (waypoints.Count > 0)
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                break;
            case GuardState.Suspicious:
                rend.material.color = suspiciousColor;
                break;
            case GuardState.Confused:
                rend.material.color = confusedColor;
                confusedTimer = confusedDuration;
                suspicionMeter = 0f;
                break;
            case GuardState.Alerted:
                rend.material.color = alertedColor;
                break;
        }
    }


    // --- DEBUG GIZMOS ---

    private void OnDrawGizmos()
    {
        // Close radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, circleRadius);

        // Vision cone
        Gizmos.color = Color.yellow;
        Vector3 left  = Quaternion.Euler(0, -coneAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0,  coneAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left  * coneRange);
        Gizmos.DrawRay(transform.position, right * coneRange);
        Gizmos.DrawRay(transform.position, transform.forward * coneRange);
    }
}
