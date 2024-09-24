using UnityEngine;

public class ShootingAi : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Логика для смерти персонажа (уничтожение объекта)
        Destroy(gameObject);
    }
}
