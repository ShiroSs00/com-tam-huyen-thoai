using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; // Bắt buộc cài package TextMeshPro (nếu máy có sẵn thì không sao)

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Settings (âm thanh)")]
    [Tooltip("Gắn Audio Mixer chính của dự án vào đây. Phải expose parameters: MasterVolume, MusicVolume, SFXVolume.")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics Settings (đồ họa)")]
    [Tooltip("Các Dropdown dùng TMP (TextMeshPro)")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        // 1. Tự động lấy danh sách độ phân giải màn hình hỗ trợ (Resolution)
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                // Format: Width x Height (Vd: 1920 x 1080)
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                // Tìm xem độ phân giải nào đang sử dụng thì lưu lại chỉ số (index)
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        // 2. Tự động lấy danh sách cấu hình đồ họa (Quality) từ Unity Settings
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // Gắn listener cho checkbox và slider
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // 3. Tải cài đặt đã lưu bằng PlayerPrefs
        LoadSettings(currentResolutionIndex);
    }

    // ================= GRAPHICS (ĐỒ HỌA) =================

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityIndex", qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // ================= AUDIO (ÂM THANH) =================
    // Lưu ý: AudioMixer của Unity dùng thang đo dB (từ -80dB đến 0dB hoặc +20dB).
    // Vì vậy Slider nên setup Min Value = 0.0001 và Max Value = 1. Hàm Log10 sẽ đổi về số đo dB.

    public void SetMasterVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    // ================= LƯU VÀ TẢI (SAVE/LOAD) =================

    private void LoadSettings(int defaultResIndex)
    {
        // Phục hồi chỉ số độ phân giải (nếu chưa có thì lấy theo độ phân giải màn hình hiện tại defaultResIndex)
        if (resolutionDropdown != null)
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex);
            resolutionDropdown.value = resIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // Phục hồi mức đồ họa (nếu chưa có thì lấy mức đồ họa mặc định của Unity)
        if (qualityDropdown != null)
        {
            int qualityIndex = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
            qualityDropdown.value = qualityIndex;
            qualityDropdown.RefreshShownValue();
        }

        // Phục hồi chế độ Fullscreen
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = isFullscreen;
        }

        // Phục hồi âm lượng
        if (masterVolumeSlider != null)
        {
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.value = masterVol;
            SetMasterVolume(masterVol); // Ép chạy volume update sau khi lấy từ storage
        }

        if (musicVolumeSlider != null)
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.value = musicVol;
            SetMusicVolume(musicVol);
        }

        if (sfxVolumeSlider != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.value = sfxVol;
            SetSFXVolume(sfxVol);
        }
    }
}
