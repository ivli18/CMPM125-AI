using UnityEngine;

public class GuardAgent : MonoBehaviour
{
    
    /*
    === STATES ===
    Roaming	no other states active	other state conditions met		
    Suspicious	player within sights	player out of sight for x duration OR reaches alerted state		works with stages? 0-100%
    Alerted	player stays within sight for x duration			
    Confused	guard suspicious for x duration/stage and player exits sight	brief delay, returns to roaming		
    */
}
