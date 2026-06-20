using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("X")]
    public float lookAheadDistance;
    public float smoothTimeX = 0.2f;

    [Header("y")]
    public float lookUpDistance = 2f;
    public float smoothTimeY = 0.2f;
    //[Header("Y")]
    public float verticalThreshold = 3f;

    private float currentOffsetX;
    private float velocityX;
    private float lookVelocityY;
    private float currentOffsetY;
    private float baseY;

    private float targetBaseY;
    private float deadZoneVelocity;



    public float debugWidth = 8f;
    private void Start()
    {
        baseY = player.position.y;


    }

    void Update()
    {
        Vector3 basePosition = player.position;

        //LOOKAHEAD (Moviemiento en X)
        float inputX = GetInputX();

        float targetOffsetX = inputX * lookAheadDistance;
        currentOffsetX = Mathf.SmoothDamp(currentOffsetX, targetOffsetX, ref velocityX, smoothTimeX);

        basePosition.x += currentOffsetX;


        //DEADZONE JUMP (Movimietno en y)

        float deadZoneY = player.position.y - targetBaseY;

        if (deadZoneY > verticalThreshold)
        {
            targetBaseY = player.position.y - verticalThreshold;
        }
        else if (deadZoneY < -verticalThreshold)
        {
            targetBaseY = player.position.y + verticalThreshold;
        }
        baseY = Mathf.SmoothDamp(baseY, targetBaseY, ref deadZoneVelocity, 0.12f);

        //LOOKUP/DOWN (Moviemiento en y)

        float inputY = GetInputY();
        
        float targetOffsetY = inputY * lookUpDistance;
        currentOffsetY = Mathf.SmoothDamp(currentOffsetY, targetOffsetY, ref lookVelocityY, smoothTimeY);

        basePosition.y = baseY + currentOffsetY;
        transform.position = basePosition;

    }

    public float GetInputX()
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

    public float GetInputY()
    {
        float value = 0f;

        if (Keyboard.current.sKey.isPressed)
        {
            value = -1;  
        }
        else if (Keyboard.current.wKey.isPressed)
        {
            value = 1; ;
        }

        return value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3 (player.position.x, baseY, transform.position.z);

        Vector3 size = new Vector3(debugWidth, verticalThreshold * 2f, 0f);

        Gizmos.DrawWireCube(center, size);
    }
}
