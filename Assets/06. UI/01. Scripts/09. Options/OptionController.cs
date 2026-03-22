using Dung.Inputs;
using Michsky.MUIP;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private ModalWindowManager shortcutModal;
    [SerializeField] private GameObject optionPanel;
    public bool OptionOn { get; private set; }
    [SerializeField] private GameObject[] uiPanels;

    private void Awake()
    {
        if (_inputReader == null)
        {
            Debug.LogError("InputReader가 연결되지 않았습니다!");
            return;
        }
        _inputReader.LoadBindings();
    }

    void Start()
    {
        foreach (var panel in uiPanels)
        {
            panel.gameObject.SetActive(false);
        }
        optionPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shortcutModal.isOn)
                shortcutModal.Close();
            else if (-1 != IsOptionEnable())
                uiPanels[IsOptionEnable()].SetActive(false);
            // else if (inExitGameModal.isOn)
            //     inExitGameModal.Close();
            // else if (inEndGameModal.isOn)
            //     inEndGameModal.Close();
            else if (optionPanel.activeSelf)
            {
                optionPanel.SetActive(false);
            }
            else
            {
                optionPanel.SetActive(true);
            }
            OptionOn = optionPanel.activeSelf;
        }
    }

    public void Toggle()
    {
        OptionOn = !OptionOn;
        optionPanel.SetActive(OptionOn);
    }

    private int IsOptionEnable()
    {
        for (int i = 0; i < uiPanels.Length; ++i)
        {
            if (uiPanels[i].activeSelf)
                return i;
        }
        return -1;
    }
}
