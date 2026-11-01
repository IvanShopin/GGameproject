using UnityEngine;

public class CameraSweep : MonoBehaviour
{
    [SerializeField] private float amplitude = 40f; // на сколько градусов качается в каждую сторону
    [SerializeField] private float speed = 0.8f;

    private float baseYaw;

    private void Start()
    {
        baseYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        float yaw = baseYaw + Mathf.Sin(Time.time * speed) * amplitude;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}