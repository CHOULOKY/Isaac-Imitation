using UnityEngine;
using UnityEngine.UI;

namespace UISpace {
      public class DeadMonster : MonoBehaviour
      {
            public Sprite[] monsters; // { Charger, Gaper, Pooter, Monstro, Spike, Bomb }

            private Image image;

            private void Awake()
            {
                  image = GetComponent<Image>();
            }

            private void OnEnable()
            {
                  //Debug.Log("Instance " + GameManager.Instance);
                  //Debug.Log("uiManager " + GameManager.Instance.uiManager);
                  //Debug.Log("killedPlayer " + GameManager.Instance.uiManager.killedPlayer);

                  image.sprite = null;
                  if (GameManager.Instance.uiManager.killedPlayer >= monsters.Length) return;
                  else image.sprite = monsters[GameManager.Instance.uiManager.killedPlayer];
            }
      }
}
