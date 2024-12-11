using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BossSlider : MonoBehaviour
{
      private Slider slider;
      public Image fillImage;

      private bool isInitHealth = false;
      private float maxBossHealth = 0;
      private float bossHealth = 0;
      public float BossHealth
      {
            get => bossHealth;
            set {
                  if(bossHealth != value) {
                        bossHealth = value;
                        if (!isInitHealth) {
                              isInitHealth = true;
                              maxBossHealth = value;
                        }
                        else {
                              if(flashCouroutine != null) StopCoroutine(flashCouroutine);
                              flashCouroutine = StartCoroutine(FlashSlider(0.05f));
                        }
                  }
            }
      }

      Coroutine flashCouroutine = null;
      private IEnumerator FlashSlider(float time)
      {
            fillImage.color = Color.white;

            yield return new WaitForSeconds(time);

            fillImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
      }




      private void Awake()
      {
            slider = GetComponent<Slider>();
            if (fillImage == null) fillImage = GetComponentsInChildren<Image>()[^1];
      }

      private void Update()
      {
            if (bossHealth <= 0) return;
            slider.value = bossHealth / maxBossHealth;
      }
}
