using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Upgraded_Dream_Shield
{
    public class UpgradedDreamShieldProjectile: MonoBehaviour
    {
        public float timeLeft;
        public float speed = 75;
        public Collider2D[] colliders;

        private static float initTimer = 0.15f;

        public void OnEnable()
        {
            timeLeft = initTimer;
        }

        public void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                gameObject.SetActive(false);
            }

            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 3f, timeLeft / initTimer);

            transform.position += Time.deltaTime * speed * transform.up;

            LayerMask mask = LayerMask.NameToLayer("Enemies") + LayerMask.NameToLayer("Projectiles");

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].IsTouchingLayers(mask))
                {
                    gameObject.SetActive(false);
                    break;
                }
            }
        }

    }
}
