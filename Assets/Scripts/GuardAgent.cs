using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum GuardState { Roaming, Suspicious, Alerted, Confused }

public class GuardAgent : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;

    [SerializeField] private float coneRange = 10f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField] private float circleRadius = 2f;

    [SerializeField] private float suspicionFillRate = 25f;
    [SerializeField] private float circleFillMult = 1.5f;
    [SerializeField] private float suspicionDrainRate = 15f;
    [SerializeField] private float confusedDuration = 3f;

    [SerializeField] private Color roamingColor = Color.green;
    [SerializeField] private Color suspiciousColor = Color.yellow;
    [SerializeField] private Color alertedColor = Color.red;
    [SerializeField] private Color confusedColor = Color.blue;

    private NavMeshAgent agent;
    private GuardVisionCone visionCone;
    private Transform player;
    private Renderer rend;

    private GuardState state = GuardState.Roaming;
    private float suspicionMeter = 0f;
    private float confusedTimer = 0f;
    private Vector3 lastKnownPosition;
    private int currentWaypointIndex = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponent<Renderer>();
        visionCone = GetComponent<GuardVisionCone>();
        player = GameObject.FindWithTag("Player").transform;

        if (waypoints.Count > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);

        SetState(GuardState.Roaming);
    }

    private void Update()
    {
        bool canSee = CanSeePlayer();
        UpdateSuspicionMeter(canSee);
        UpdateStateTransitions(canSee);
        UpdateStateBehavior();
        visionCone.UpdateVision(coneRange, coneAngle, circleRadius, rend.material.color, suspicionMeter);
    }

    private bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        bool inCone = distance <= coneRange &&
                      Vector3.Angle(transform.forward, toPlayer) <= coneAngle / 2f;
        bool inRadius = distance <= circleRadius;

        if (!inCone && !inRadius) return false;

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

    private void UpdateStateBehavior()
    {
        switch (state)
        {
            case GuardState.Roaming:
                agent.speed = 3.5f;
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextWaypoint();
                break;

            case GuardState.Suspicious:
                agent.SetDestination(transform.position);
                Vector3 dir = player.position - transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir);
                break;

            case GuardState.Confused:
                agent.SetDestination(lastKnownPosition);
                break;

            case GuardState.Alerted:
                agent.speed = 8f;
                agent.SetDestination(player.position);
                if (Vector3.Distance(transform.position, player.position) < 1.2f)
                    player.GetComponent<PlayerController>().TriggerLose();
                break;
        }
    }

    private void SetState(GuardState newState)
    {
        state = newState;
        switch (newState)
        {
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

    private void GoToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

}