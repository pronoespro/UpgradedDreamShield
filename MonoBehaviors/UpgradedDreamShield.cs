using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Upgraded_Dream_Shield
{
    public class UpgradedDreamShield:MonoBehaviour
    {

        public float inputThreshold = 0.2f;
        public float rotationSpeed = 0.5f;

        public float scaleSpeed = 0.35f;

        public float shieldJumpTime = 1f;
        public float slashTimer = 2f;

        private float curSlashTimer;

        public void OnEnable()
        {
            transform.localScale = Vector3.one;
        }

        public void Update()
        {
            if (!PlayerData.instance.GetBool("equippedCharm_38"))
            {
                gameObject.SetActive(false);
                return;
            }

            if (curSlashTimer >= 0)
            {
                curSlashTimer -= Time.deltaTime;
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0.5f, 0.5f, 0.5f), scaleSpeed * Time.deltaTime * 100);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.35f, 1.35f, 1.35f), scaleSpeed * Time.deltaTime * 100);
            }

            transform.position = HeroController.instance.transform.position + new Vector3(0, -0.25f);

            if (HeroController.instance.vertical_input > inputThreshold)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime * 100);
            }
            else if (HeroController.instance.vertical_input < -inputThreshold)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.root.eulerAngles.x, transform.rotation.eulerAngles.y, 180), rotationSpeed * Time.deltaTime * 100);
            }
            else if (HeroController.instance.move_input > inputThreshold)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.root.eulerAngles.x, transform.rotation.eulerAngles.y, 270), rotationSpeed * Time.deltaTime * 100);
            }
            else if (HeroController.instance.move_input < -inputThreshold)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.root.eulerAngles.x, transform.rotation.eulerAngles.y, 90), rotationSpeed * Time.deltaTime * 100);
            }
        }

        public void Slash()
        {
            if (curSlashTimer <= 0)
            {
                curSlashTimer = slashTimer;

                UpgradedShieldMod.Instance.CreateDeeShieldProjectile(transform.position + transform.up * 2, transform.rotation);

            }
        }

    }
}
