using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controller geral do menu. Fica ativo apenas na cena de Menu.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Botão padrão de seleção")]
    [SerializeField] private Button _defaultButton;

    private void Start()
    {
        // Garante que ao entrar na cena de Menu já haja um botão selecionado
        EventSystem.current.SetSelectedGameObject(_defaultButton.gameObject);
    }
}
