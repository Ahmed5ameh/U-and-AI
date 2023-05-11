using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    public class MyGameManager : MonoBehaviour
    {
        private static MyGameManager _instance;

        [SerializeField] public float totalGameTime = 180;
        private float totalGameTimeLeft;

        public static MyGameManager Instance => _instance;

        private void Awake()
        {
            _instance ??= this;
        }

        private void Start()
        {
            totalGameTimeLeft = totalGameTime;
        }

        private void Update()
        {
            totalGameTimeLeft -= Time.deltaTime;
            if (totalGameTimeLeft == 0)
            {
                //TODO Lose
            }
            MyUIManager.Instance.SetTimeLeftUI(totalGameTime -= Time.deltaTime);
        }
    }
}
