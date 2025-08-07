using System;
using UnityEngine;

public static class CrystalEventManager
{
    // Evento disparado quando um cristal � destruido
    // O parametro � o numero de cristais destruidos at� o momento
    public static event Action<int> OnCrystalDestroyed;

    public static void CrystalDestroyed (int totalCrystalsDestroyed)
    {
        OnCrystalDestroyed?.Invoke(totalCrystalsDestroyed);
    }
}
