using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject winPanel;        // Win UI panel here
    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v) * moveSpeed;
        cc.Move(move * Time.deltaTime);
    }

    public void TriggerWin()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TriggerLose()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}