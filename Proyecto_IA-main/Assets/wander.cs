using UnityEngine;
using UnityEngine.InputSystem;

public class wander : MonoBehaviour
{
    [Header("Configuración del Agente")]
    public float maxSpeed = 5f;
    public float maxForce = 0.1f;
    public float mass = 1f;

    [Header("Configuración de Wander")]
    [Tooltip("Distancia del círculo de wander por delante del agente")]
    public float wanderCircleDistance = 3f;

    [Tooltip("Radio del círculo dentro del cual se elige la dirección aleatoria")]
    public float wanderCircleRadius = 1.5f;

    [Tooltip("Grado máximo de cambio aleatorio por segundo (en radianes)")]
    public float wanderJitter = 5f;

    [Header("Configuración de Obstáculos")]
    [Tooltip("Lista de obstáculos")]
    public Transform[] obstacles;
    [Tooltip("Radio de detección")]
    public float obstacleAvoidanceRadius = 3f;
    [Tooltip("Intensidad de reacción")]
    public float obstacleWeight = 2f;

    private Vector2 velocity;
    private float wanderAngle;
    private bool isActive = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            isActive = !isActive;
        }

        if (!isActive) return;

        //Obtener la fuerza de Wander
        Vector2 wanderForce = CalculateWanderForce();

        //Obtener la fuerza de Evasión de Obstáculos
        Vector2 obstacleForce = CalculateObstacleAvoidance();

        //Combinar las fuerzas damos prioridad a la evasión de obstáculos para que no choque
        Vector2 totalSteering = wanderForce + (obstacleForce * obstacleWeight);

        //Limitar la fuerza total de dirección
        totalSteering = Vector2.ClampMagnitude(totalSteering, maxForce);
        totalSteering /= mass;

        //Aplicar la fuerza a la velocidad y actualizar la posición
        velocity = Vector2.ClampMagnitude(velocity + totalSteering, maxSpeed);
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private Vector2 CalculateWanderForce()
    {
        //Modificar el ángulo aleatoriamente
        wanderAngle += Random.Range(-1f, 1f) * wanderJitter * Time.deltaTime;

        //Si la velocidad es cero, usamos transform.right como dirección por defecto
        Vector2 currentVelocity = velocity == Vector2.zero ? (Vector2)transform.right * maxSpeed : velocity;

        //Calcular el centro del círculo por delante de la posición actual del agente
        Vector2 circleCenter = (Vector2)transform.position + currentVelocity.normalized * wanderCircleDistance;

        //Desplazamiento dentro del círculo proyectado
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderCircleRadius;

        //Posición objetivo final del wander
        Vector2 targetPosition = circleCenter + displacement;

        //Velocidad deseada hacia el punto objetivo
        Vector2 desiredVelocity = (targetPosition - (Vector2)transform.position).normalized * maxSpeed;

        //Fuerza de dirección 
        Vector2 steering = desiredVelocity - velocity;
        return steering;
    }

    //Método que calcula la repulsión matemática de los obstáculos
    private Vector2 CalculateObstacleAvoidance()
    {
        Vector2 totalAvoidanceForce = Vector2.zero;

        if (obstacles == null || obstacles.Length == 0) return totalAvoidanceForce;

        foreach (Transform obs in obstacles)
        {
            if (obs == null) continue;

            Vector2 toObstacle = (Vector2)obs.position - (Vector2)transform.position;
            float distanceToObstacle = toObstacle.magnitude;

            //Si el obstáculo está dentro del radio de influencia
            if (distanceToObstacle < obstacleAvoidanceRadius)
            {
                //Vector que apunta en sentido contrario del obstáculo
                Vector2 repulseDir = ((Vector2)transform.position - (Vector2)obs.position).normalized;

                // Intensidad basada en la cercanía (más cerca = más fuerza)
                float strength = 1f - (distanceToObstacle / obstacleAvoidanceRadius);

                //Acumulamos la fuerza de repulsión
                totalAvoidanceForce += repulseDir * strength * maxSpeed;
            }
        }

        return totalAvoidanceForce;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 currentVelocity = velocity == Vector2.zero ? (Vector2)transform.right * maxSpeed : velocity;
        Vector2 circleCenter = (Vector2)transform.position + currentVelocity.normalized * wanderCircleDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(circleCenter, wanderCircleRadius);

        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderCircleRadius;
        Vector2 targetPosition = circleCenter + displacement;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.15f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPosition);

        //Dibujar radios de los obstáculos en la escena
        if (obstacles != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform obs in obstacles)
            {
                if (obs != null)
                {
                    Gizmos.DrawWireSphere(obs.position, obstacleAvoidanceRadius);
                }
            }
        }
    }
}
