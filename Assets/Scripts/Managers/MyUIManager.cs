using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Enemies
{
    public class MyUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreTxt;
        [SerializeField] private TMP_Text timeLeftTxt;

        private static MyUIManager _instance;

        public static MyUIManager Instance => _instance;

        private void Awake()
        {
            _instance ??= this;
        }

        private void Start()
        {
            SetTimeLeftUI(MyGameManager.Instance.totalGameTime);
        }

        private void Update()
        {
            
        }

        public void SetScoreUI(float @value)
        {
            scoreTxt.text = @value.ToString();
        }

        public void SetTimeLeftUI(float @value)
        {
            timeLeftTxt.text = @value.ToString();
        }
    }
}
