using UnityEngine;
using UnityEngine.InputSystem;

public class flee : MonoBehaviour
{
  
    [Header("Configuración del Agente")]
    public float maxSpeed = 5f;
    public float maxForce = 0.1f;
    public float mass = 1f;

    [Header("Configuración de Flee (Mouse)")]
    [Tooltip("Distancia a la cual el agente detectará el cursor y comenzará a huir")]
    public float panicRadius = 5f;

    [Header("Configuración de Obstáculos")]
    [Tooltip("Lista de obstáculos")]
    public Transform[] obstacles;
    [Tooltip("Radio de detección")]
    public float obstacleAvoidanceRadius = 3f;
    [Tooltip("Intensidad de reacción de los obstáculos")]
    public float obstacleWeight = 2f;

    private Vector2 velocity;
    private bool isActive = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            isActive = !isActive;
        }

        if (!isActive) return;

        //Validación por si no hay un mouse conectado o activo
        if (Mouse.current == null) return;

        //Obtener la posición del mouse
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        //Calcular el vector desde el mouse hacia el agente
        Vector2 targetOffset = (Vector2)transform.position - mouseWorldPos;
        float distance = targetOffset.magnitude;

        //Si el cursor está fuera del radio de pánico, el agente frena gradualmente
        if (distance > panicRadius)
        {
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, maxForce * Time.deltaTime);
            transform.position += (Vector3)(velocity * Time.deltaTime);
            return;
        }

        //Calcular la velocidad deseada a máxima velocidad en dirección opuesta al mouse
        Vector2 desiredVelocity = targetOffset.normalized * maxSpeed;

        //Calcular la fuerza de dirección (Steering Force) para huir del mouse
        Vector2 steering = desiredVelocity - velocity;
        steering = Vector2.ClampMagnitude(steering, maxForce);
        steering /= mass;

        //Obtener la fuerza de los obstáculos y multiplicarla por su peso
        Vector2 avoidanceForce = CalculateObstacleAvoidance() * obstacleWeight;

        //Combinar la fuerza de huida con la de evasión de obstáculos
        Vector2 totalForce = steering + avoidanceForce;
        totalForce = Vector2.ClampMagnitude(totalForce, maxForce);

        //Aplicar la fuerza a la velocidad y actualizar la posición 2D
        velocity = Vector2.ClampMagnitude(velocity + totalForce, maxSpeed);
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

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

                float strength = 1f - (distanceToObstacle / obstacleAvoidanceRadius);

                //Acumulamos la fuerza de repulsión
                totalAvoidanceForce += repulseDir * strength * maxSpeed;
            }
        }

        return totalAvoidanceForce;
    }

    //Dibujar el área de pánico y los obstáculos en el editor 2D
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicRadius);

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
