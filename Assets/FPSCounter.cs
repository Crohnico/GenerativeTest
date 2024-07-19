using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TMPro.TMP_Text text;
    private float deltaTime = 0.0f;

    public float updateTime = 0.5f;
    private float updateIndex;

    private void Start()
    {
        updateIndex = Time.time + updateTime;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (updateIndex < Time.time)
        {
            updateIndex = Time.time + updateTime;
            float fps = 1.0f / deltaTime;

            if (text != null)
            {
                text.text = string.Format("{0:0.}", fps);
            }
        }
    }


}
