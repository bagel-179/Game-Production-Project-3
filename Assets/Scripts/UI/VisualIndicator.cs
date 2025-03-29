using UnityEngine;

public class VisualIndicator : MonoBehaviour
{
    public GameObject UIIndicator;
    private void Start()
    {
        UIIndicator.SetActive(false);    
    }

    public void EnableIndicator(bool enable)
    {
        UIIndicator.SetActive(enable);
    }
}
