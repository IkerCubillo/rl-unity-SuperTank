using UnityEngine;

public class RiverArea : MonoBehaviour
{
    [SerializeField] private float speedReductionFactor = 0.5f;
    private bool isPlayerInRiver = false;

    private void Start()
    {
        // Verificar que los colliders están configurados correctamente
        foreach (Transform child in transform)
        {
            BoxCollider[] colliders = child.GetComponents<BoxCollider>();
            
            // Verificar si hay múltiples colliders
            if (colliders.Length > 1)
            {
                Debug.LogError($"¡Error! {child.name} tiene múltiples Box Colliders. Debe tener solo uno.");
                continue;
            }
            
            // Verificar si no hay colliders
            if (colliders.Length == 0)
            {
                Debug.LogError($"¡Error! {child.name} no tiene Box Collider.");
                continue;
            }

            BoxCollider col = colliders[0];
            
            // Verificar configuración del collider
            if (!col.isTrigger)
            {
                Debug.LogError($"¡Error! El Box Collider en {child.name} debe tener 'Is Trigger' activado.");
                col.isTrigger = true;
            }

            // Verificar escala negativa
            Vector3 localScale = child.localScale;
            if (localScale.x < 0 || localScale.y < 0 || localScale.z < 0)
            {
                Debug.LogError($"¡Error! {child.name} tiene una escala negativa. Ajusta la rotación en su lugar.");
            }

            // Añadir RiverTrigger
            if (child.GetComponent<RiverTrigger>() == null)
            {
                RiverTrigger trigger = child.gameObject.AddComponent<RiverTrigger>();
                trigger.Initialize(this);
                Debug.Log($"RiverTrigger añadido a {child.name}");
            }
        }
    }

    public void HandlePlayerEnter(GameObject player)
    {
        if (!isPlayerInRiver)
        {
            isPlayerInRiver = true;
            Debug.Log($"¡Tanque {player.name} entrando en el río!");
            
            SuperTank superTank = player.GetComponent<SuperTank>();
            if (superTank != null)
            {
                superTank.SetSpeedMultiplier(speedReductionFactor);
                Debug.Log($"Velocidad del tanque reducida al {speedReductionFactor * 100}%");
            }
            else
            {
                Debug.LogError($"No se encontró el componente SuperTank en {player.name}");
            }
        }
    }

    public void HandlePlayerExit(GameObject player)
    {
        isPlayerInRiver = false;
        Debug.Log("¡Tanque saliendo del río!");
        
        SuperTank superTank = player.GetComponent<SuperTank>();
        if (superTank != null)
        {
            superTank.SetSpeedMultiplier(1f);
            Debug.Log("Velocidad del tanque restaurada a normal");
        }
        else
        {
            Debug.LogError("No se encontró el componente SuperTank en el objeto Player");
        }
    }
}