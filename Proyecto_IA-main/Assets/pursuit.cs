using UnityEngine;
using UnityEngine.InputSystem;

public class pursuit : MonoBehaviour
{
    [Header("Configuración del Agente")]
    public Transform target;
    public float maxSpeed = 5f;
    public float maxForce = 0.1f;
    public float mass = 1f;

    [Header("Configuración de Pursuit")]
    [Tooltip("Límite máximo de segundos hacia el futuro que intentará predecir")]
    public float maxPredictionTime = 2f;

    [Header("Configuración de Obstáculos")]
    [Tooltip("Lista de obstáculos")]
    public Transform[] obstacles;
    [Tooltip("Radio de detección")]
    public float obstacleAvoidanceRadius = 3f;
    [Tooltip("Intensidad de reacción")]
    public float obstacleWeight = 2f;

    private Vector3 velocity;
    private Vector3 lastTargetPosition;
    private bool isActive = false;

    void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            isActive = !isActive;
        }

        if (!isActive) return;

        if (target == null) return;

        // Estimar la velocidad del objetivo
        Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
        lastTargetPosition = target.position;

        // Calcular la distancia hacia el objetivo
        Vector3 targetOffset = target.position - transform.position;
        float distance = targetOffset.magnitude;

        // Calcular el tiempo de predicción proporcional a la distancia
        float speed = velocity.magnitude;
        float predictionTime;

        if (speed <= distance / maxPredictionTime)
        {
            predictionTime = maxPredictionTime;
        }
        else
        {
            predictionTime = distance / speed;
        }

        //Calcular la posición futura predicha del objetivo
        Vector3 predictedPosition = target.position + targetVelocity * predictionTime;

        //Calcular la velocidad deseada 
        Vector3 desiredVelocity = (predictedPosition - transform.position).normalized * maxSpeed;

        //Calcular la fuerza de dirección base (Pursuit)
        Vector3 steering = desiredVelocity - velocity;

        //Obtener y sumar la fuerza de evasión de obstáculos multiplicada por su peso
        Vector3 avoidanceForce = CalculateObstacleAvoidance();
        steering += avoidanceForce;

        //Limitar y aplicar la masa a la fuerza total resultante
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        //Actualizar la velocidad y la posición del agente
        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    private Vector3 CalculateObstacleAvoidance()
    {
        Vector3 totalAvoidanceForce = Vector3.zero;

        if (obstacles == null || obstacles.Length == 0) return totalAvoidanceForce;

        foreach (Transform obs in obstacles)
        {
            if (obs == null) continue;

            Vector3 toObstacle = obs.position - transform.position;
            float distanceToObstacle = toObstacle.magnitude;

            //Si el obstáculo está dentro del radio de influencia
            if (distanceToObstacle < obstacleAvoidanceRadius)
            {
                //Vector que apunta en sentido contrario del obstáculo
                Vector3 repulseDir = (transform.position - obs.position).normalized;

                //Intensidad basada en la cercanía (más cerca = más fuerte)
                float strength = 1f - (distanceToObstacle / obstacleAvoidanceRadius);

                //Acumulamos la fuerza aplicando el peso de configuración
                totalAvoidanceForce += repulseDir * strength * obstacleWeight;
            }
        }

        return totalAvoidanceForce;
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null && Application.isPlaying)
        {
            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
            float distance = Vector3.Distance(transform.position, target.position);
            float speed = velocity.magnitude;
            float prediction = (speed <= 0) ? maxPredictionTime : Mathf.Min(maxPredictionTime, distance / speed);

            Vector3 predictedPos = target.position + targetVelocity * prediction;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(predictedPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, predictedPos);
        }

        //Dibujar el radio de evasión de los obstáculos 
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
