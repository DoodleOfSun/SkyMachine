using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractiveWithBullet : MonoBehaviour
{
    public void Damaged()
    {
        Player.instance.PlayerDamaged(0.1f);
    }
}