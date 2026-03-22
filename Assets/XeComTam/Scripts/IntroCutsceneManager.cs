using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[System.Serializable]
public class IntroSceneData
{
    public Sprite backgroundImage;
    [TextArea(3, 10)]
    public string dialogueText;
    public AudioClip voiceLine;
}

public class IntroCutsceneManager : MonoBehaviour
{
    public List<IntroSceneData> introScenes;
    public string nextSceneName = "GameScene";

    [Header("UI References")]
    public Image backgroundImageUI;
    public TextMeshProUGUI dialogueTextUI;
    public AudioSource audioSource;
    public float typeWriterSpeed = 0.05f;

    private int currentSceneIndex = 0;
    private bool isTypingText = false;
    private string currentFullText = "";

    void Start()
    {
        if (introScenes.Count > 0)
        {
            ShowScene(0);
        }
    }

    void Update()
    {
        bool isClicked = false;

#if ENABLE_INPUT_SYSTEM
        // Dành cho bản Unity dùng Input System Mới
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) isClicked = true;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) isClicked = true;
#else
        // Dành cho bản Unity dùng Input System Cũ
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) isClicked = true;
#endif

        // Chuyển cảnh hoặc hiện hết chữ khi người chơi click chuột hoặc bấm Space/Enter
        if (isClicked)
        {
            if (isTypingText)
            {
                // Nếu chữ đang chạy mà bấm click -> Hiện hết chữ ngay lập tức
                StopAllCoroutines();
                dialogueTextUI.text = currentFullText;
                isTypingText = false;
            }
            else
            {
                // Nếu chữ đã hiện xong -> Chuyển sang ảnh/câu thoại tiếp theo
                NextScene();
            }
        }
    }

    void ShowScene(int index)
    {
        if (index < 0 || index >= introScenes.Count) return;

        IntroSceneData sceneData = introScenes[index];

        // 1. Đổi hình ảnh nền
        if (sceneData.backgroundImage != null)
        {
            backgroundImageUI.sprite = sceneData.backgroundImage;
        }

        // 2. Phát giọng nói/âm thanh
        if (sceneData.voiceLine != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = sceneData.voiceLine;
            audioSource.Play();
        }

        // 3. Chạy hiệu ứng chữ gõ từng tự (Typewriter effect)
        currentFullText = sceneData.dialogueText;
        StartCoroutine(TypeText(currentFullText));
    }

    void NextScene()
    {
        currentSceneIndex++;

        if (currentSceneIndex < introScenes.Count)
        {
            // Vẫn còn cảnh -> Hiện cảnh tiếp theo
            ShowScene(currentSceneIndex);
        }
        else
        {
            // Đã hết cảnh -> Chuyển sang màn hình chơi game chính
            FinishIntro();
        }
    }

    IEnumerator TypeText(string textToType)
    {
        isTypingText = true;
        dialogueTextUI.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueTextUI.text += letter;
            yield return new WaitForSeconds(typeWriterSpeed);
        }

        isTypingText = false;
    }

    public void FinishIntro()
    {
        // Nhảy sang Scene tiếp theo
        SceneManager.LoadScene(nextSceneName);
    }
}
