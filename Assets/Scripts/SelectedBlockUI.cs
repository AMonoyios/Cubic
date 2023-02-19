using UnityEngine;
using TMPro;

public class SelectedBlockUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI block;

    private void FixedUpdate()
    {
        block.text = World.Instance.GetPlayer.SelectedBlockName;
    }
}
