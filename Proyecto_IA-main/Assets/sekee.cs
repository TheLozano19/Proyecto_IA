using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class sekee : MonoBehaviour
{
    [Header("Configuración del Agente")]
    public Transform target;
    public float maxSpeed = 5f;
    public float maxForce = 0.1f;
    public float mass = 1f;

    [Header("Configuración de Arrival")]
    [Tooltip("Distancia a la cual el agente comenzará a frenar")]
    public float slowingRadius = 3f;

    private Vector3 velocity;
    private bool isActive = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            isActive = !isActive;
        }

        if (!isActive) return;

        if (target == null) return;

        //Calcular el vector dirección y la distancia hacia el objetivo
        Vector3 targetOffset = target.position - transform.position;
        float distance = targetOffset.magnitude;

        // Si ya estamos muy cerca
        if (distance < 0.05f)
        {
            velocity = Vector3.zero;
            return;
        }

        //Calcular la velocidad deseada según la distancia 
        float rampedSpeed = maxSpeed * (distance / slowingRadius);
        float targetSpeed = Mathf.Min(rampedSpeed, maxSpeed);

        Vector3 desiredVelocity = (targetOffset / distance) * targetSpeed;

        //Calcular la fuerza de dirección 
        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        //Aplicar la fuerza a la velocidad y actualizar la posición
        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slowingRadius);
    }
}