using UnityEngine;
using TMPro;

public class SelectedBlockUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI block;

    private void Start()
    {
        EventsManager.Instance.OnUpdateSelectedBlockUI += UpdateSelectedBlockUI;
    }

    private void UpdateSelectedBlockUI()
    {
        block.text = World.Instance.GetPlayer.SelectedBlockName;
    }
}
