using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemVR
{
    public string itemID;
    public string nombre;
    public Sprite icono;
}

public class InventarioVR : MonoBehaviour
{
    public static InventarioVR Instance { get; private set; }

    [SerializeField] private int capacidadMaxima = 20;

    private readonly List<ItemVR> items = new List<ItemVR>();

    public IReadOnlyList<ItemVR> Items => items;
    public event Action<ItemVR> OnItemAgregado;
    public event Action<ItemVR> OnItemRemovido;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AgregarItem(ItemVR item)
    {
        if (item == null || items.Count >= capacidadMaxima) return false;
        items.Add(item);
        OnItemAgregado?.Invoke(item);

        if (GameManagerVR.Instance != null && !string.IsNullOrEmpty(item.itemID))
            GameManagerVR.Instance.ActivarFlag("item_" + item.itemID);

        return true;
    }

    public bool RemoverItem(string itemID)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemID == itemID)
            {
                var item = items[i];
                items.RemoveAt(i);
                OnItemRemovido?.Invoke(item);
                return true;
            }
        }
        return false;
    }

    public bool TieneItem(string itemID)
    {
        foreach (var item in items)
            if (item.itemID == itemID) return true;
        return false;
    }
}
