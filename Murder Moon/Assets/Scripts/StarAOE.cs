using UnityEngine;

public class StarAOE : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //AudioManager.Instance.PlaySound(soundWhenHittingSomething, transform.position);

        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<RatPlayer>().Kill();
        }
    }
}
