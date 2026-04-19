using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4f; // Velocidad ajustada
    public Rigidbody2D rb;
    public Animator animator;
    
    // Aquí le decimos al código que existe una linterna
    public GameObject linterna; 

    Vector2 movement;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // ¡La magia de la rotación!
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            // Le restamos 90 grados para que el cono apunte hacia adelante y no de lado
            linterna.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}