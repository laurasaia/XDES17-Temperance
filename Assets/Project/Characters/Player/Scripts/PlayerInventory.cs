using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private static bool hasKey;   // sobrevive à troca de cena

    public bool HasKey => hasKey;
    public static event Action<bool> OnKeyChanged;

    void Start() => OnKeyChanged?.Invoke(hasKey); // HUD se atualiza ao entrar

    public void GiveKey() { hasKey = true;  OnKeyChanged?.Invoke(true); }
    public void UseKey()  { hasKey = false; OnKeyChanged?.Invoke(false); }
}