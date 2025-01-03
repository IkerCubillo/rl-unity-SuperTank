using UnityEngine;

public class RiverTrigger : MonoBehaviour
{
    private RiverArea riverArea;

    public void Initialize(RiverArea area)
    {
        riverArea = area;
    }

    private void OnTriggerEnter(Collider other)
    {
        SuperTank superTank = other.GetComponent<SuperTank>();
        if (superTank != null && riverArea != null)
        {
            Debug.Log($"RiverTrigger: Tanque entró en {gameObject.name}");
            riverArea.HandlePlayerEnter(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SuperTank superTank = other.GetComponent<SuperTank>();
        if (superTank != null && riverArea != null)
        {
            Debug.Log($"RiverTrigger: Tanque salió de {gameObject.name}");
            riverArea.HandlePlayerExit(other.gameObject);
        }
    }
} 