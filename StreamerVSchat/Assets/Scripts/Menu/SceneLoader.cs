using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Singleton que responde a pedidos de troca de cena.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    /// <summary>
    /// Evento disparado para pedir a troca de cena.
    /// </summary>
    public static event Action<string> OnSceneChangeRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        OnSceneChangeRequested += LoadSceneAsync;
    }

    private void OnDisable()
    {
        OnSceneChangeRequested -= LoadSceneAsync;
    }

    /// <summary>
    /// Chamado por UI buttons: SceneLoader.RequestSceneChange("NomeDaCena");
    /// </summary>
    public static void RequestSceneChange(string sceneName)
    {
        OnSceneChangeRequested?.Invoke(sceneName);
    }

    private void LoadSceneAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
