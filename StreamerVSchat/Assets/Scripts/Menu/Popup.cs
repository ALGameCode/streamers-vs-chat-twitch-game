using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que o pr�prio bot�o �Abrir� ou �Fechar� do pop-up chama.
/// </summary>
public class Popup : MonoBehaviour
{
    [Tooltip("O bot�o que deve ficar selecionado ao abrir este pop-up")]
    [SerializeField] private Button _firstSelectedButton;

    /// <summary>
    /// Deve ser atribu�do no onClick do bot�o que abre este pop-up.
    /// </summary>
    public void Open()
    {
        PopupManager.Instance.ShowPopup(gameObject, _firstSelectedButton);
    }

    /// <summary>
    /// Deve ser atribu�do no onClick do bot�o que fecha este pop-up.
    /// </summary>
    public void Close()
    {
        PopupManager.Instance.HidePopup(gameObject);
    }
}
