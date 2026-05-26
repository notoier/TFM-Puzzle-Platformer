using Unity.Cinemachine;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("X")]
    public float lookAheadDistance;
    public float smoothTimeX = 0.2f;

    [Header("Y")]
    public float verticalThreshold = 3f;

    private float currentOffsetX;
    private float velocityX;

    private float baseY;
    private float velocityY;

    private void Start()
    {
        baseY = player.position.y;


    }

    void Update()
    {
        Vector3 basePosition = player.position;

        //LOOKAHEAD (Moviemiento en X)
        float inputX = GetInput();

        float targetOffsetX = inputX * lookAheadDistance;
        currentOffsetX = Mathf.SmoothDamp(currentOffsetX, targetOffsetX, ref velocityX, smoothTimeX);

        basePosition.x += currentOffsetX;


        //DEADZONE JUMP (Movimietno en y)

        float deadZoneY = player.position.y - baseY;

        if (deadZoneY > verticalThreshold)
        {
            baseY = player.position.y - verticalThreshold;
        }
        else if (deadZoneY < -verticalThreshold)
        {
            baseY = player.position.y + verticalThreshold;
        }


        basePosition.y = baseY;
        transform.position = basePosition;

        Debug.DrawLine(new Vector3(player.position.x - 5, baseY, 0), new Vector3(player.position.x + 5, baseY, 0), Color.red, 0.1f);
        Debug.DrawRay(new Vector3(player.position.x, baseY, 0), Vector3.up * 0.5f, Color.green);
    }

    public float GetInput()
    {
        float value = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            value -= 1;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            value += 1; ;
        }

        return value;
    }

}
