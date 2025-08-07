using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controller geral do menu. Fica ativo apenas na cena de Menu.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Bot�o padr�o de sele��o")]
    [SerializeField] private Button _defaultButton;

    private void Start()
    {
        // Garante que ao entrar na cena de Menu j� haja um bot�o selecionado
        EventSystem.current.SetSelectedGameObject(_defaultButton.gameObject);
    }
}
