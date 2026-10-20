using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void Awake()
    {
        // Obstacles should still collide with the player, but pass through the
        // side boundaries so they cannot snag or bounce off them.
        GameObject walls = GameObject.Find("Walls");
        if (walls == null)
        {
            Debug.LogWarning("Obstacle could not find the 'Walls' object; wall collisions remain enabled.", this);
            return;
        }

        Collider2D[] obstacleColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] wallColliders = walls.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D obstacleCollider in obstacleColliders)
        {
            foreach (Collider2D wallCollider in wallColliders)
            {
                if (obstacleCollider != null && wallCollider != null)
                {
                    Physics2D.IgnoreCollision(obstacleCollider, wallCollider);
                }
            }
        }
    }

    void Update()
    {
        if(transform.position.y<-6f){
            Destroy(gameObject);
        }
    }
}
