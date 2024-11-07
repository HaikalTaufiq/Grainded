using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastUIController : MonoBehaviour
{
    [SerializeField] private GameObject sliderGO;
    [SerializeField] private Slider slider;
    [SerializeField] private Text text;

    bool isActive = false;

    private void Start()
    {
        sliderGO.SetActive(false);
    }

    public void SetCastSliderActive(bool isActive)
    {
        if (!isActive)
        {
            isActive = false;
            sliderGO.SetActive(false);
            slider.value = 0f;
            text.text = "0";
            return;
        }

        sliderGO.SetActive(true);

    }

    public void SetCastPowerSlider(float val)
    {
        slider.value = val / 100f;
        text.text = ((int)(val)).ToString();
    }
}
