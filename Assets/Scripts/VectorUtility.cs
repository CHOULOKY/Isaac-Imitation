using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorUtilities
{
      public static class VectorUtility
      {
            public static Vector2 Rotate(this Vector2 v, float angle)
            {
                  // 1. 각도를 라디안으로 변환
                  float radian = angle * Mathf.Deg2Rad;

                  // 2. 회전 행렬의 요소 계산
                  float cos = Mathf.Cos(radian);
                  float sin = Mathf.Sin(radian);

                  // 3. 회전 행렬 적용
                  return new Vector2(
                      v.x * cos - v.y * sin,
                      v.x * sin + v.y * cos
                  );
            }
      }
}
