using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey { get; private set; }

    // A HUD escuta este evento
    public static event Action<bool> OnKeyChanged;

    public void GiveKey()
    {
        HasKey = true;
        OnKeyChanged?.Invoke(true);
    }

    public void UseKey()
    {
        HasKey = false;
        OnKeyChanged?.Invoke(false);
    }
}