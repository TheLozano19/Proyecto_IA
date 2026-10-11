using UnityEngine;
using UnityEngine.InputSystem;

public class evade : MonoBehaviour
{
    /* [Header("Configuración del Agente")]
     public Transform target;
     public float maxSpeed = 5f;
     public float maxForce = 0.1f;
     public float mass = 1f;

     [Header("Configuración de Evade")]
     [Tooltip("Distancia máxima a la que el agente detecta al wey que lo sigue")]
     public float evadeRadius = 10f;
     [Tooltip("Que tanto ve al furo para correr recio")]
     public float maxPrediction = 2f;

     private Vector3 velocity;
     private Vector3 previousTargetPosition;

     void Start()
     {
         if (target != null)
         {
             previousTargetPosition = target.position;
         }
     }

     void Update()
     {
         if (target == null) return;

         //Calcular la velocidad del objetivo 
         Vector3 targetVelocity = (target.position - previousTargetPosition) / Time.deltaTime;
         previousTargetPosition = target.position;

         Vector3 targetOffset = target.position - transform.position;
         float distance = targetOffset.magnitude;

         //Si el objetivo está fuera del rango de evasión
         if (distance > evadeRadius)
         {
             //frenar
             velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 2f);
             transform.position += velocity * Time.deltaTime;
             return;
         }

         //Predecir la futura posición del objetivo
         float speed = velocity.magnitude;
         float prediction;

         //Si la velocidad es muy baja o el objetivo está muy cerca, la predicción es directa
         if (speed <= distance / maxPrediction)
         {
             prediction = maxPrediction;
         }
         else
         {
             prediction = distance / speed;
         }

         Vector3 predictedPosition = target.position + targetVelocity * prediction;

         //Calcular la velocidad deseada para HUIR (Flee) de la posición predicha
         //Flee invierte el vector: (transform.position - predictedPosition)
         Vector3 desiredVelocity = (transform.position - predictedPosition).normalized * maxSpeed;

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
         Gizmos.DrawWireSphere(transform.position, evadeRadius);
     }*/
    [Header("Configuración del Agente")]
    public Transform target;
    public float maxSpeed = 5f;
    public float maxForce = 0.1f;
    public float mass = 1f;

    [Header("Configuración de Evade")]
    [Tooltip("Distancia máxima a la que el agente detecta al objetivo que lo sigue")]
    public float evadeRadius = 10f;
    [Tooltip("Qué tanto ve al futuro para correr recio")]
    public float maxPrediction = 2f;

    [Header("Configuración de Obstáculos")]
    [Tooltip("Lista de obstáculos")]
    public Transform[] obstacles;
    [Tooltip("Radio de detección")]
    public float obstacleAvoidanceRadius = 3f;
    [Tooltip("intensidad de reaccion")]
    public float obstacleWeight = 2f;

    private Vector3 velocity;
    private Vector3 previousTargetPosition;
    private bool isActive = false;

    void Start()
    {
        if (target != null)
        {
            previousTargetPosition = target.position;
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

        //Calcular la velocidad del objetivo 
        Vector3 targetVelocity = (target.position - previousTargetPosition) / Time.deltaTime;
        previousTargetPosition = target.position;

        Vector3 targetOffset = target.position - transform.position;
        float distance = targetOffset.magnitude;

        Vector3 desiredVelocity = Vector3.zero;

        // Si el objetivo está dentro del rango de evasión
        if (distance <= evadeRadius)
        {
            // Predecir la futura posición del objetivo
            float speed = velocity.magnitude;
            float prediction;

            if (speed <= distance / maxPrediction)
            {
                prediction = maxPrediction;
            }
            else
            {
                prediction = distance / speed;
            }

            Vector3 predictedPosition = target.position + targetVelocity * prediction;

            // Velocidad deseada para correr
            desiredVelocity = (transform.position - predictedPosition).normalized * maxSpeed;
        }
        else
        {
            // Frenar suavemente si el objetivo está lejos
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 2f);
            transform.position += velocity * Time.deltaTime;
            return;
        }

        //Calcular la fuerza de dirección base
        Vector3 steering = desiredVelocity - velocity;

        // Fuerza para esquivar obstaculos 
        Vector3 obstacleSteering = CalculateObstacleAvoidance();
        steering += obstacleSteering * obstacleWeight;

        //Limitar y aplicar la fuerza
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        //Aplicar la fuerza a la velocidad y actualizar la posición
        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    //Método que calcula la repulsión matemática de los obstáculos
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

                float strength = 1f - (distanceToObstacle / obstacleAvoidanceRadius);

                // Acumulamos la fuerza de repulsión
                totalAvoidanceForce += repulseDir * strength * maxSpeed;
            }
        }

        return totalAvoidanceForce;
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de evasión del objetivo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);

        // Dibujar radios de los obstáculos en la escena
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
