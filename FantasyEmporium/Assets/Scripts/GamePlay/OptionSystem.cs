using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionSystem : MonoBehaviour
{
    public bool IsTitle = false;

    [System.Serializable]
    public struct OptionUI
    {
        [Header("Option")]
        public GameObject OptionPannel;
        public GameObject ConfirmPannel;
        public GameObject RoomCodePannel;
        public GameObject MainPannel;
        public TextMeshProUGUI ConfirmText;

        [Space(10f)]
        [Header("Video")]
        public TMP_Dropdown DisplayDropdown;
        public Toggle FullScreenToggle;

        // 이후 사운드 관련 추가 예정

        [Space(10f)]
        [Header("RoomCode")]
        public Button CodeVisiableButton;
        public Sprite OpenEyes;
        public Sprite CloseEyes;
        public TextMeshProUGUI RoomCodeText;
    }

    public OptionUI optionUI;

    // private Resolution Variables
    private Resolution pendingResolution;
    private FullScreenMode pendingFullscreenMode;

    private List<Resolution> CustomResolutions = new List<Resolution>()
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 1920, height = 1200 },
        new Resolution { width = 2560, height = 1440 },
        new Resolution { width = 3440, height = 1440 },
        new Resolution { width = 3840, height = 2160 }
    };

    private Keyboard keyboard;
    private string HideCode = "* * * * * *";
    private bool IsOpen = false;

    #region Resoltuion Functions
    void SetResoltionDropdown()
    {
        // 현재 모니터 해상도가 List (지원 해상도 목록)에 없을 경우 index 0번에 추가
        int currentIndex = -1;

        for (int i = 0; i < CustomResolutions.Count; i++)
        {
            if (CustomResolutions[i].width == GameDataManager.resolution.width && CustomResolutions[i].height == GameDataManager.resolution.height)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            CustomResolutions.Insert(0, new Resolution { width = Screen.currentResolution.width, height = Screen.currentResolution.height });
            currentIndex = 0;
        }

        // 드롭다운 옵션 생성
        optionUI.DisplayDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var res in CustomResolutions)
        {
            options.Add($"{res.width}x{res.height}");
        }
        optionUI.DisplayDropdown.AddOptions(options);

        optionUI.DisplayDropdown.SetValueWithoutNotify(currentIndex);
        optionUI.DisplayDropdown.RefreshShownValue();

        optionUI.DisplayDropdown.onValueChanged.RemoveListener(OnChangeResolution);
        optionUI.FullScreenToggle.onValueChanged.RemoveListener(OnChangeFullScreen);
        optionUI.DisplayDropdown.onValueChanged.AddListener(OnChangeResolution);
        optionUI.FullScreenToggle.onValueChanged.AddListener(OnChangeFullScreen);

        optionUI.DisplayDropdown.interactable = (GameDataManager.fullScreenMode == FullScreenMode.Windowed);
        optionUI.FullScreenToggle.SetIsOnWithoutNotify(GameDataManager.fullScreenMode == FullScreenMode.FullScreenWindow);
    }

    void OnChangeResolution(int index)
    {
        if (GameDataManager.fullScreenMode == FullScreenMode.FullScreenWindow) { return; }

        pendingResolution = CustomResolutions[index];
        pendingFullscreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(pendingResolution.width, pendingResolution.height, pendingFullscreenMode);
        optionUI.DisplayDropdown.SetValueWithoutNotify(index);
        optionUI.DisplayDropdown.RefreshShownValue();
        optionUI.ConfirmPannel.SetActive(true);
    }

    void OnChangeFullScreen(bool IsOn)
    {
        pendingResolution = GameDataManager.resolution;
        pendingFullscreenMode = IsOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        optionUI.DisplayDropdown.interactable = !IsOn;
        Screen.SetResolution(pendingResolution.width, pendingResolution.height, pendingFullscreenMode);
        optionUI.ConfirmPannel.SetActive(true);
    }
    #endregion

    #region Option Buttons
    public void OptionButton()
    {
        optionUI.MainPannel.SetActive(false);
        optionUI.OptionPannel.SetActive(true);
    }

    public void OptionCloseButton()
    {
        optionUI.OptionPannel.SetActive(false);
        
        if (IsTitle)
        {
            optionUI.MainPannel.SetActive(true);
        }
    }

    public void ExitButton()
    {
        SceneManager.LoadScene("Title");
    }

    public void ConfirmButton()
    {
        GameDataManager.resolution = pendingResolution;
        GameDataManager.fullScreenMode = pendingFullscreenMode;
        GameDataManager.SaveResolution(pendingResolution, pendingFullscreenMode);
        int newIndex = CustomResolutions.FindIndex(r => r.width == GameDataManager.resolution.width && r.height == GameDataManager.resolution.height);
        if (newIndex >= 0)
        {
            optionUI.DisplayDropdown.SetValueWithoutNotify(newIndex);
            optionUI.DisplayDropdown.RefreshShownValue();
        }
        optionUI.ConfirmPannel.SetActive(false);
    }

    public void ReserveButton()
    {
        Screen.SetResolution(GameDataManager.resolution.width, GameDataManager.resolution.height, GameDataManager.fullScreenMode);
        int newIndex = CustomResolutions.FindIndex(r => r.width == GameDataManager.resolution.width && r.height == GameDataManager.resolution.height);
        if (newIndex >= 0)
        {
            optionUI.DisplayDropdown.SetValueWithoutNotify(newIndex);
            optionUI.DisplayDropdown.RefreshShownValue();
        }
        optionUI.DisplayDropdown.interactable = (GameDataManager.fullScreenMode == FullScreenMode.Windowed);
        optionUI.FullScreenToggle.isOn = (GameDataManager.fullScreenMode != FullScreenMode.Windowed);
        optionUI.ConfirmPannel.SetActive(false);
    }

    public void HideButton()
    {
        if (IsOpen)
        {
            optionUI.CodeVisiableButton.image.sprite = optionUI.CloseEyes;
            optionUI.RoomCodeText.text = HideCode;
        }
        else
        {
            optionUI.CodeVisiableButton.image.sprite = optionUI.OpenEyes;
            optionUI.RoomCodeText.text = RelayInfo.joinCode;
        }

        IsOpen = !IsOpen;
    }

    public void CopyCodeButton()
    {
        GUIUtility.systemCopyBuffer = RelayInfo.joinCode;
    }
    #endregion

    void Awake()
    {
        if (IsTitle)
        {
            Application.targetFrameRate = 60;
            GameDataManager.LoadResolution();
            Screen.SetResolution(GameDataManager.resolution.width, GameDataManager.resolution.height, GameDataManager.fullScreenMode);
        }
    }

    void Start()
    {
        optionUI.OptionPannel.SetActive(false);
        optionUI.ConfirmPannel.SetActive(false);
        optionUI.RoomCodePannel.SetActive(!IsTitle);

        keyboard = Keyboard.current;
        optionUI.RoomCodeText.text = HideCode;

        SetResoltionDropdown();
    }

    void Update()
    {
        if (!IsTitle)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                if (optionUI.OptionPannel.activeSelf == true)
                {
                    optionUI.RoomCodePannel.SetActive(true);

                }
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                optionUI.OptionPannel.SetActive(true);
            }
        }   
    }
}
