using UnityEngine;

public class SortRendererBy
{
      private int originalNumber = 99999;

      public void SortBy(SpriteRenderer curObject, SpriteRenderer byTarget, bool wantBackOriginal = false)
      {
            if (originalNumber == 99999) originalNumber = curObject.sortingOrder;

            if (wantBackOriginal) {
                  curObject.sortingOrder = originalNumber;
            }
            else if (curObject.transform.position.y > byTarget.transform.position.y) {
                  curObject.sortingOrder = byTarget.sortingOrder - 2;
            }
            else {
                  curObject.sortingOrder = byTarget.sortingOrder + 2;
            }
      }
}
