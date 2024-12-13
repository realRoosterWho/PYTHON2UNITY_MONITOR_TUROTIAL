using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CounterDisplay : MonoBehaviour
{
    public UdpBroadcastReceiver udpBroadcastReceiver; // 引用 UdpBroadcastReceiver
    public TextMeshProUGUI counterText; // 引用 TextMeshProUGUI 组件

    // Start is called before the first frame update
    void Start()
    {
        if (udpBroadcastReceiver == null)
        {
            udpBroadcastReceiver = FindObjectOfType<UdpBroadcastReceiver>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (udpBroadcastReceiver != null && counterText != null)
        {
            counterText.text = "Counter: " + udpBroadcastReceiver.counter.ToString();
        }
    }
}