using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

namespace RentIsDue.Core
{
    public class MainMenuManager : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            CreateUI();
        }

        private void CreateUI()
        {
            // 1. Create EventSystem if it doesn't exist
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 2. Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // 3. Create Background Panel
            GameObject panelObj = new GameObject("Background");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 4. Create Title Text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "RENT IS DUE";
            titleText.fontSize = 100;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.sizeDelta = new Vector2(800, 150);
            titleRect.anchoredPosition = Vector2.zero;

            // 5. Create Buttons
            float startY = 100f;
            float spacing = 120f;

            string savePath = Application.persistentDataPath + "/save.json";
            bool hasSave = File.Exists(savePath);

            Button continueBtn = CreateButton("CONTINUE", panelObj.transform, new Vector2(0, startY));
            continueBtn.interactable = hasSave;
            continueBtn.onClick.AddListener(() => {
                SceneManager.LoadScene("SampleScene");
            });

            Button newGameBtn = CreateButton("NEW GAME", panelObj.transform, new Vector2(0, startY - spacing));
            newGameBtn.onClick.AddListener(() => {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                SceneManager.LoadScene("SampleScene");
            });

            Button quitBtn = CreateButton("QUIT", panelObj.transform, new Vector2(0, startY - spacing * 2));
            quitBtn.onClick.AddListener(() => {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }

        private Button CreateButton(string text, Transform parent, Vector2 anchoredPos)
        {
            GameObject btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);
            
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            
            // Set ColorBlock for hover effect
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            cb.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            cb.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            btn.colors = cb;

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = new Vector2(400, 80);
            btnRect.anchoredPosition = anchoredPos;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.fontSize = 40;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }
    }
}


