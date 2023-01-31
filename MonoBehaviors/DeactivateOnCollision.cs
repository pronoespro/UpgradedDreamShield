using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Upgraded_Dream_Shield
{
    class DeactivateOnCollision: MonoBehaviour
    {

        public Transform target;
        public LayerMask mask=0;

        public void Start()
        {
            if (target == null)
            {
                target = transform;
            }
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (mask == (mask | (1 << collider.gameObject.layer))){
                target.gameObject.SetActive(false);
            }
        }

    }
}
