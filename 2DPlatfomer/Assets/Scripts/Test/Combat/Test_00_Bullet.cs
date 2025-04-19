using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_Bullet : TestBase
{
    public GameObject bulletObject;

    public Transform spawnPosition;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if(spawnPosition != null)
        {
            Bullet bullet = Instantiate(bulletObject, spawnPosition.position, Quaternion.identity).GetComponent<Bullet>();
            bullet.Initialize(null, this.transform.right, 10f, 1, 3, 0.25f, 10f);
        }
    }
}