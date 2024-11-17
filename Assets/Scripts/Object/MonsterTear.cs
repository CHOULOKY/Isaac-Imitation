using UnityEngine;

public class MonstroTear : Tear
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            DisableTear();
        }
        else if (collision.CompareTag("Player"))
        {
            DisableTear();

            if (collision.TryGetComponent<IsaacBody>(out var player))
            {
                player.health -= tearDamage;
                player.IsHurt = true;
            }
            else
            {
                Debug.LogWarning("Player(IsaacBody)¸¦ Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            }
        }
    }


    private float tearActiveTimeDefault;

    private void Start()
    {
        tearActiveTimeDefault = tearActiveTime;
    }

    private void RandomizeTearSet()
    {
        this.transform.localScale = Vector2.one * UnityEngine.Random.Range(0.5f, 1f);

        tearActiveTime = UnityEngine.Random.Range(0.25f, tearActiveTimeDefault + 0.5f);
        gravityScale = tearActiveTime - 0.25f;
        gravitySetTime = tearActiveTime - 0.5f;
    }
}
