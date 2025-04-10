using UnityEngine;

public class LevelPrefabSectioner : MonoBehaviour
{
    [System.Serializable]
    public class ObjectToggle
    {
        public GameObject targetObject;
        public bool setActiveOnTrigger = true;
    }

    [Header("Objects to Toggle on Trigger Enter")]
    public ObjectToggle[] objectsToToggle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (ObjectToggle toggle in objectsToToggle)
            {
                if (toggle.targetObject != null)
                {
                    toggle.targetObject.SetActive(toggle.setActiveOnTrigger);
                }
            }
        }
    }
}