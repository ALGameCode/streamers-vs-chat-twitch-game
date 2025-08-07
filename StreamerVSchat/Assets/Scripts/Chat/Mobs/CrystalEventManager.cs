using System;
using UnityEngine;

public static class CrystalEventManager
{
    // Evento disparado quando um cristal é destruido
    // O parametro é o numero de cristais destruidos até o momento
    public static event Action<int> OnCrystalDestroyed;

    public static void CrystalDestroyed (int totalCrystalsDestroyed)
    {
        OnCrystalDestroyed?.Invoke(totalCrystalsDestroyed);
    }
}
