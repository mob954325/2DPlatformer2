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
        player.OnHpChange = SetSliderValue;
        SetSliderValue(player.MaxHp); // slider value �ʱ�ȭ
    }

    public void SetSliderValue(float value)
    {
        StopAllCoroutines();
        StartCoroutine(ValueReduceProcess(value / player.MaxHp));
    }

    /// <summary>
    /// �÷��̾� ü�� ��ȭ ������ �ڷ�ƾ ( �ʴ� ü���� �پ�� )
    /// </summary>
    /// <param name="goalValue">������ ü��</param>
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
