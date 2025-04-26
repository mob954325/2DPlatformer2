using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillUI : MonoBehaviour
{
    Player player;
    Image image;

    public void Initialize(Player player)
    {
        this.player = player;
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if(player.HasSpecialAttack)
        {
            image.color = new Color(1, 1, 1, 1);
        }
        else
        {
            image.color = new Color(1, 1, 1, 0);
        }
    }
}
