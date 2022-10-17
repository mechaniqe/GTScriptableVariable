using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GTVariable
{

    public class BarWidget : MonoBehaviour
    {
        [SerializeField] private ReadOnlyFloatVariable currentValue;
        [SerializeField] private ReadOnlyFloatVariable maxValue;
        [SerializeField] private Image fillBar;
        [SerializeField] private UpdateMode updateMode = UpdateMode.Event;
        [SerializeField] private bool UpdateOnEnable;

        private void OnEnable()
        {
            if (UpdateOnEnable)
            {
                UpdateWidget();
            }

            if(updateMode == UpdateMode.Event)
            {
                currentValue.OnValueChanged.AddListener(UpdateWidget);
                maxValue.OnValueChanged.AddListener(UpdateWidget);
            }
        }

        private void OnDisable()
        {
            if (updateMode == UpdateMode.Event)
            {
                currentValue.OnValueChanged.RemoveListener(UpdateWidget);
                maxValue.OnValueChanged.RemoveListener(UpdateWidget);
            }
        }

        private void Update()
        {
            if(updateMode == UpdateMode.Update)
            {
                UpdateWidget();
            }
        }

        public void UpdateWidget()
        {
            fillBar.fillAmount = currentValue.GetValue() / maxValue.GetValue();
        }
    }
}