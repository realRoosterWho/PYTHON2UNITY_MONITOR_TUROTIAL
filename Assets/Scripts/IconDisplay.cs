using System.Collections;
using UnityEngine;

public class IconDisplay : MonoBehaviour
{
    public UdpBroadcastReceiver udpBroadcastReceiver; // 引用 UdpBroadcastReceiver
    public SpriteRenderer spriteRenderer; // 引用 SpriteRenderer 组件
    public Color color1 = new Color(0.3686f, 0.3686f, 0.3686f); // 5e5e5e
    public Color color2 = new Color(1f, 1f, 1f); // ffffff
    private int lastCounter = 0;
    private Vector3 initialScale; // 初始 scale
    private Vector3 scale2; // 目标 scale

    // Start is called before the first frame update
    void Start()
    {
        if (udpBroadcastReceiver == null)
        {
            udpBroadcastReceiver = FindObjectOfType<UdpBroadcastReceiver>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        initialScale = transform.localScale; // 获取初始 scale
        scale2 = initialScale * 1.2f; // 目标 scale
    }

    // Update is called once per frame
    void Update()
    {
        if (udpBroadcastReceiver != null && udpBroadcastReceiver.counter != lastCounter)
        {
            lastCounter = udpBroadcastReceiver.counter;
            StartCoroutine(ChangeColorAndScale());
        }
    }

    private IEnumerator ChangeColorAndScale()
    {
        float duration = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            spriteRenderer.color = Color.Lerp(color1, color2, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(initialScale, scale2, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = color2;
        transform.localScale = scale2;

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            spriteRenderer.color = Color.Lerp(color2, color1, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(scale2, initialScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = color1;
        transform.localScale = initialScale;
    }
}