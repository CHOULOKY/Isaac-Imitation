using UnityEngine;

public class MonstroTear : Tear
{
      private void OnTriggerEnter2D(Collider2D collision)
      {
            if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle")) {
                  DisableTear();
            }
            else if (collision.CompareTag("Player")) {
                  DisableTear();

                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (player.IsHurt) return;
                        player.Health -= tearDamage;
                        player.IsHurt = true;
                        GameManager.Instance.uiManager.setKilledPlayer = "Monstro";
                  }
                  else {
                        Debug.LogWarning("Player(IsaacBody)를 찾을 수 없습니다.");
                  }
            }
      }


      private float tearActiveTimeDefault;

      private void Start()
      {
            tearActiveTimeDefault = tearActiveTime;
            RandomizeTearSet();
      }

      protected override void OnEnable()
      {
            RandomizeTearSet();
            base.OnEnable();
      }

      private void RandomizeTearSet()
      {
            // 눈물 크기
            this.transform.localScale = Vector2.one * UnityEngine.Random.Range(0.5f, 1.25f);

            // 눈물 활성 시간
            tearActiveTime = UnityEngine.Random.Range(tearActiveTimeDefault, tearActiveTimeDefault + 3f);

            // 중력 크기 및 활성 시간
            gravityScale = 0.25f;
      }
}
