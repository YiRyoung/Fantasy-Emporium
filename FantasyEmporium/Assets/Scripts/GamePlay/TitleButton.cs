using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class TitleButton : MonoBehaviour
{
    #region Variables
    [System.Serializable]
    public struct TitleUI
    {
        [Header("Pannels")]
        public GameObject MenuPannel;
        public GameObject SavePannel;
        public GameObject JoinPannel;
        public GameObject CreatePannel;
        public GameObject WarningPannel;
        public GameObject DeletePannel;
        public GameObject QuitPannel;

        public TextMeshProUGUI WarningText;

        [Space(10)]
        [Header("Save Slots")]
        public GameObject[] SelectButton;
        public GameObject[] CreateButton;
        public GameObject[] TrashButton;
        public GameObject DeleteEnterButton;

        [Space(10)]
        [Header("Input Fields")]
        public TMP_InputField SlotNameInputField;
        [Space(8)]
        public TMP_InputField[] JoinInputFields;
    }

    [System.Serializable]
    public struct FadeUI
    {
        public Image FadeImage;
        public GameObject FadeObject;
        public float Duration;

        [Space(10)]
        public CanvasGroup WarningMessage;
        public float FloatingDuration;
        public float RestDuration;
    }

    [System.Serializable]
    public struct SlotData
    {
        public TextMeshProUGUI[] SlotNameText;
        public TextMeshProUGUI[] SlotDate;
        public TextMeshProUGUI[] SlotCoin;
    }

    [HideInInspector] public ServerManager serverManager;
    [HideInInspector] public static int currentSlotIndex = -1;

    private string SavePath = "";

    public TitleUI titleUI;
    public FadeUI fadeUI;
    public SlotData slotdata;

    #endregion

    #region PlayMode
    public void SingleButton()
    {
        titleUI.MenuPannel.SetActive(false);
        titleUI.SavePannel.SetActive(true);
        GameDataManager.MultiMode = 1;
        CheckSaveSlots();
    }

    public void HostButton()
    {
        titleUI.MenuPannel.SetActive(false);
        titleUI.SavePannel.SetActive(true);
        GameDataManager.MultiMode = 2;
        CheckSaveSlots();
    }

    public void PlayGameButton()
    {
        switch (GameDataManager.MultiMode)
        {
            case -1:
                Debug.LogError("Not Setting Playmode is Multi!");
                return;
            case 1: // 오프라인
                SceneManager.LoadScene("Play");
                break;
            case 2: // 멀티
                serverManager.HostGame();
                SceneManager.LoadScene("Play");
                break;
        }
    }

    public void JoinButton()
    {
        titleUI.MenuPannel.SetActive(false);
        titleUI.JoinPannel.SetActive(true);
    }

    public void JoinEnterButton()
    {
        // 공백 검사
        foreach (var field in titleUI.JoinInputFields)
        {
            if (string.IsNullOrEmpty(field.text))
            {
                Debug.LogError("Join code is incompleted!");
                return;
            }
        }

        string joinCode = GetJoinCode();
        serverManager.JoinGame(joinCode);
    }

    public void JoinCloseButton()
    {
        titleUI.JoinPannel.SetActive(false);
        titleUI.MenuPannel.SetActive(true);

        foreach (var field in titleUI.JoinInputFields)
        {
            field.SetTextWithoutNotify(string.Empty);
        }

        titleUI.JoinInputFields[0].Select();
        titleUI.JoinInputFields[0].ActivateInputField();
    }

    void SetJoinInputFields()
    {
        for (int i = 0; i < titleUI.JoinInputFields.Length; i++)
        {
            int index = i;
            var field = titleUI.JoinInputFields[i];

            field.characterLimit = 0; // 중요: 제한 제거

            field.onValueChanged.AddListener(value =>
            {
                if (string.IsNullOrEmpty(value))
                    return;

                // 붙여넣기
                if (value.Length > 1)
                {
                    DistributePaste(value, index);
                    return;
                }

                // 단일 입력
                field.SetTextWithoutNotify(value[0].ToString());

                if (index < titleUI.JoinInputFields.Length - 1)
                {
                    titleUI.JoinInputFields[index + 1].Select();
                    titleUI.JoinInputFields[index + 1].ActivateInputField();
                }
            });
        }
    }

    void DistributePaste(string pastedText, int startIndex)
    {
        int fieldCount = titleUI.JoinInputFields.Length;
        int charIndex = 0;

        for (int i = startIndex; i < fieldCount && charIndex < pastedText.Length; i++)
        {
            titleUI.JoinInputFields[i]
                .SetTextWithoutNotify(pastedText[charIndex].ToString());
            charIndex++;
        }

        int focusIndex = Mathf.Min(startIndex + charIndex, fieldCount - 1);
        titleUI.JoinInputFields[focusIndex].Select();
        titleUI.JoinInputFields[focusIndex].ActivateInputField();
    }

    public string GetJoinCode()
    {
        string code = "";
        foreach (var field in titleUI.JoinInputFields)
        {
            code += field.text;
        }
        return code;
    }

    public void QuitButton()
    {
        titleUI.QuitPannel.SetActive(true);
    }

    public void QuitEnterButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void QuitCloseButton()
    {
        titleUI.QuitPannel.SetActive(false);
    }
    #endregion

    #region SaveSlot
    // Single & Host에서 바로 SavePannel로 이동
    public void SaveCloseButton()
    {
        GameDataManager.MultiMode = -1;
        titleUI.SavePannel.SetActive(false);
        titleUI.MenuPannel.SetActive(true);
    }

    public void CreateButton()
    {
        titleUI.SavePannel.SetActive(false);
        titleUI.CreatePannel.SetActive(true);
    }
    public void CreateEnterButton()
    {
        // 공백 검사
        if (string.IsNullOrEmpty(titleUI.SlotNameInputField.text))
        {
            titleUI.WarningText.text = "Slot name can't be empty!";
            StartCoroutine(WarningFade());
            return;
        }

        // 중복 검사
        if (GameDataManager.IsDuplicateFileName(titleUI.SlotNameInputField.text, 3))
        {
            titleUI.WarningText.text = "Slot name already exist!";
            StartCoroutine(WarningFade());
            return;
        }

        GameData data = new GameData();
        data.InitFile(titleUI.SlotNameInputField.text, 1, 100);
        GameDataManager.Save(data, currentSlotIndex);
        PlayGameButton();
    }

    public void CreateCloseButton()
    {
        titleUI.CreatePannel.SetActive(false);
        titleUI.SavePannel.SetActive(true);
    }

    public void DeleteButton()
    {
        titleUI.DeletePannel.SetActive(true);
    }

    public void DeleteEnterButton()
    {
        GameDataManager.Delete(currentSlotIndex);
        CheckSaveSlots();
        titleUI.DeletePannel.SetActive(false);
    }

    public void DeleteCloseButton()
    {
        titleUI.DeletePannel.SetActive(false);
    }

    void SetSlotButtonEvent()
    {
        for (int i = 0; i < titleUI.SelectButton.Length; i++)
        {
            int index = i;
            Button btn = titleUI.SelectButton[i].GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                currentSlotIndex = index;
                Debug.Log("Selected Slot : " + currentSlotIndex);
            });
        }

        for (int i = 0; i < titleUI.CreateButton.Length; i++)
        {
            int index = i;
            Button btn = titleUI.CreateButton[i].GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                currentSlotIndex = index;
                Debug.Log("Selected Slot : " + currentSlotIndex);
            });
        }

        for (int i = 0; i < titleUI.TrashButton.Length; i++)
        {
            int index = i;
            Button btn = titleUI.TrashButton[i].GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                currentSlotIndex = index;
                Debug.Log("Selected Slot : " + currentSlotIndex);
            });
        }
    }

    void CheckSaveSlots()
    {
        for (int i = 0; i < titleUI.SelectButton.Length; i++)
        {
            string fileName = Path.Combine(SavePath, $"slot{i}.json");

            if (File.Exists(fileName))
            {
                titleUI.SelectButton[i].SetActive(true);
                titleUI.CreateButton[i].SetActive(false);

                GameData LoadData = GameDataManager.Load(i);
                SetLoadData(LoadData, i);
            }
            else
            {
                titleUI.SelectButton[i].SetActive(false);
                titleUI.CreateButton[i].SetActive(true);
            }
        }
    }

    void SetLoadData(GameData _Data, int _index)
    {
        if (_Data != null)
        {
            slotdata.SlotNameText[_index].text = _Data.FileName;
            slotdata.SlotDate[_index].text = _Data.Date.ToString();
            slotdata.SlotCoin[_index].text = _Data.Coin.ToString();
        }
    }
    #endregion

    #region Fade
    void FadeIn()
    {
        fadeUI.FadeObject.SetActive(true);
        this.Run(TweenUtils.FadeTo(fadeUI.FadeImage, 1f, 0f, fadeUI.Duration, FadeEnd));
    }

    void FadeOut()
    {
        fadeUI.FadeObject.SetActive(true);
        this.Run(TweenUtils.FadeTo(fadeUI.FadeImage, 0f, 1f, fadeUI.Duration, FadeEnd));
    }

    void FadeEnd()
    {
        fadeUI.FadeObject.SetActive(false);
    }

    IEnumerator WarningFade()
    {
        titleUI.WarningPannel.SetActive(true);
        this.Run(TweenUtils.FadeTo(fadeUI.WarningMessage, 0f, 1f, fadeUI.FloatingDuration));

        yield return new WaitForSeconds(fadeUI.RestDuration);

        this.Run(TweenUtils.FadeTo(fadeUI.WarningMessage, 1f, 0f, fadeUI.FloatingDuration, 
        () =>
        {
            titleUI.WarningPannel.SetActive(false);
        }));
    }

    #endregion


    void Start()
    {
        SavePath = Application.persistentDataPath;
        
        titleUI.MenuPannel.SetActive(true);
        titleUI.SavePannel.SetActive(false);
        titleUI.JoinPannel.SetActive(false);
        titleUI.CreatePannel.SetActive(false);
        titleUI.WarningPannel.SetActive(false);
        titleUI.DeletePannel.SetActive(false);
        titleUI.QuitPannel.SetActive(false);

        SetSlotButtonEvent();
        SetJoinInputFields();
        FadeIn();
    }
}