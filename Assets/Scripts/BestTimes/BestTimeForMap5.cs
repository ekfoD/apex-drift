using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.BestTimes
{
    public class BestTimeForMap5 : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI bestTimeText;

        [Header("Display Settings")]
        public string prefixText = "BEST TIME: ";
        public string noTimeText = "BEST TIME: --:--";

        private string ghostFileName;

        void Start()
        {
            ghostFileName = "world_05_ghost.json";

            UpdateBestTimeDisplay();
        }

        void UpdateBestTimeDisplay()
        {
            if (bestTimeText == null)
            {
                Debug.LogError("TextMeshProUGUI reference is not assigned!");
                return;
            }

            float bestTime = LoadBestTime();

            if (bestTime == float.MaxValue)
            {
                bestTimeText.text = noTimeText;
            }
            else
            {
                bestTimeText.text = prefixText + FormatTime(bestTime);
            }
        }

        float LoadBestTime()
        {
            string path = Path.Combine(Application.persistentDataPath, ghostFileName);

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    GhostData data = JsonUtility.FromJson<GhostData>(json);
                    return data.bestTime;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading ghost data: {e.Message}");
                    return float.MaxValue;
                }
            }

            return float.MaxValue;
        }

        string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);

            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }

        // Call this method to refresh the display (e.g., after a new best time is set)
        public void RefreshDisplay()
        {
            UpdateBestTimeDisplay();
        }
    }
}
