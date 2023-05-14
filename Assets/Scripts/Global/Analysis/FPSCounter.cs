using UnityEngine;
using UnityEngine.UI;

namespace Global.Analysis
{

    public class FPSCounter : MonoBehaviour
    {
        int framesPassed = 0;
        float fpsTotal = 0f;
        float minFPSValue = Mathf.Infinity;
        float maxFPSValue = 0f;

        string currentFPS;
        string averageFPS;
        string maxFPS;
        string minFPS;

        GUIStyle style = new GUIStyle();

        // Start is called before the first frame update
        void Start()
        {
            //  Application.targetFrameRate = 60;
            style.fontSize = 40;
        }

        // Update is called once per frame
        void Update()
        {
            // Current FPS value
            float fps = 1f / Time.unscaledDeltaTime;
            currentFPS = "Cur. FPS: " + (int)fps;

            // Calculate average
            fpsTotal += fps;
            framesPassed++;
            averageFPS = "Avg. FPS: " + (int)(fpsTotal / framesPassed);

            // Max FPS
            if (fps > maxFPSValue && framesPassed > 10)
            {
                maxFPSValue = fps;
                maxFPS = "Max FPS: " + (int)maxFPSValue;
            }

            // Min FPS
            if (fps < minFPSValue && framesPassed > 10)
            {
                minFPSValue = fps;
                minFPS = "Min. FPS: " + (int)minFPSValue;
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));

            GUILayout.Label("\n" + string.Join("\n", currentFPS), style);
            GUILayout.Label("\n" + string.Join("\n", averageFPS), style);
            GUILayout.Label("\n" + string.Join("\n", maxFPS), style);
            GUILayout.Label("\n" + string.Join("\n", minFPS), style);
            GUILayout.EndArea();
        }
    }
}