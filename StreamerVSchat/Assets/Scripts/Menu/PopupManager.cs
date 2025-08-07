using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Singleton que vive apenas na cena de Menu e controla a exibi��o de pop-ups.
/// </summary>
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    // Guarda o bot�o que estava selecionado antes de abrir o pop-up
    private GameObject _lastSelected;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Abre o pop-up e seleciona automaticamente o bot�o inicial dentro dele.
    /// </summary>
    public void ShowPopup(GameObject popup, Button firstSelectedButton)
    {
        // salva sele��o atual
        _lastSelected = EventSystem.current.currentSelectedGameObject;

        // ativa o pop-up
        popup.SetActive(true);

        // define o primeiro bot�o dentro do pop-up
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    /// <summary>
    /// Fecha o pop-up e retorna a sele��o para onde estava antes.
    /// </summary>
    public void HidePopup(GameObject popup)
    {
        popup.SetActive(false);

        // restaura sele��o
        EventSystem.current.SetSelectedGameObject(_lastSelected);
    }
}
