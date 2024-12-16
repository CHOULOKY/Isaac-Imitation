using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorUtilities
{
      public static class VectorUtility
      {
            public static Vector2 Rotate(this Vector2 v, float angle)
            {
                  // 1. ������ �������� ��ȯ
                  float radian = angle * Mathf.Deg2Rad;

                  // 2. ȸ�� ����� ��� ���
                  float cos = Mathf.Cos(radian);
                  float sin = Mathf.Sin(radian);

                  // 3. ȸ�� ��� ����
                  return new Vector2(
                      v.x * cos - v.y * sin,
                      v.x * sin + v.y * cos
                  );
            }
      }
}
