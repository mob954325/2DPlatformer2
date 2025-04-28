using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    Player player;
    Slider hpSlider;

    public void Initialize(Player player)
    {
        this.player = player;
        hpSlider = GetComponent<Slider>();
        player.OnHpChange += SetSliderValue;
        SetSliderValue(player.MaxHp); // slider value 초기화
    }

    public void SetSliderValue(float value)
    {
        StopAllCoroutines();
        StartCoroutine(ValueReduceProcess(value / player.MaxHp));
    }

    /// <summary>
    /// 플레이어 체력 변화 보간용 코루틴 ( 초당 체력이 줄어듬 )
    /// </summary>
    /// <param name="goalValue">도달할 체력</param>
    private IEnumerator ValueReduceProcess(float goalValue)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;
            hpSlider.value = Mathf.Lerp(hpSlider.value, goalValue, timeElapsed);
            yield return null;
        }

        hpSlider.value = goalValue;
    }
}
