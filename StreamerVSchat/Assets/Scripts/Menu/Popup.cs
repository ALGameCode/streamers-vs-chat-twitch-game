using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que o próprio botão “Abrir” ou “Fechar” do pop-up chama.
/// </summary>
public class Popup : MonoBehaviour
{
    [Tooltip("O botão que deve ficar selecionado ao abrir este pop-up")]
    [SerializeField] private Button _firstSelectedButton;

    /// <summary>
    /// Deve ser atribuído no onClick do botão que abre este pop-up.
    /// </summary>
    public void Open()
    {
        PopupManager.Instance.ShowPopup(gameObject, _firstSelectedButton);
    }

    /// <summary>
    /// Deve ser atribuído no onClick do botão que fecha este pop-up.
    /// </summary>
    public void Close()
    {
        PopupManager.Instance.HidePopup(gameObject);
    }
}
