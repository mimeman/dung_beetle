
using TMPro;

public class UI_InteractPrompt : UIBase
{
    private TMP_Text _promptText;
    public override void Init()
    {
        _promptText = GetUI<TMP_Text>("PromptText");
    }
    public void SetPrompt(string text) { _promptText.text = text; }
}