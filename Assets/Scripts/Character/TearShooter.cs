using UnityEngine;

public interface TearShooter
{
      public void AttackUsingTear();

      public void SetTearPositionAndGravityTime(GameObject curTear, out float x, out float y);

      public void SetTearVelocity(out Vector2 tearVelocity);

      public void ShootSettedTear(GameObject curTear, Vector2 tearVelocity, float x, float y);
}
