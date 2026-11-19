using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HorrorObjectiveHUD : MonoBehaviour
{
    [System.Serializable]
    public class ObjectiveItem
    {
        public string id;
        [TextArea] public string description;
        public bool isCompleted;
    }

    [Header("Referencias UI (TextMeshPro)")]
    public TextMeshProUGUI locationHeaderText;
    public TextMeshProUGUI objectivesListText;

    [Header("Configuracion de Escena")]
    public string sectorName = "SUBSUELO 01 // CORREDOR DE CONTENCIÓN";
    public List<ObjectiveItem> objectives = new List<ObjectiveItem>()
    {
        new ObjectiveItem { id = "find_rack", description = "Localiza la sala de servidores con brecha de energía" },
        new ObjectiveItem { id = "read_error", description = "Revisa el registro de anomalías en la terminal" },
        new ObjectiveItem { id = "reboot_rack", description = "Fuerza el reinicio antes del apagón total" }
    };

    [Header("Colores HUD")]
    public string pendingColorHex = "#E5E5E5";
    public string completedColorHex = "#7A2020";

    void Start()
    {
        if (locationHeaderText != null)
            locationHeaderText.text = sectorName;

        RefreshHUD();
    }

    public void CompleteObjective(string objectiveId)
    {
        ObjectiveItem item = objectives.Find(o => o.id == objectiveId);
        if (item != null && !item.isCompleted)
        {
            item.isCompleted = true;
            RefreshHUD();
        }
    }

    public void AddNewObjective(string id, string description)
    {
        objectives.Add(new ObjectiveItem { id = id, description = description, isCompleted = false });
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (objectivesListText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var obj in objectives)
        {
            if (obj.isCompleted)
            {
                sb.AppendLine($"<color={completedColorHex}>[X] <s>{obj.description}</s></color>");
            }
            else
            {
                sb.AppendLine($"<color={pendingColorHex}>[ ] {obj.description}</color>");
            }
        }

        objectivesListText.text = sb.ToString();
    }
}
