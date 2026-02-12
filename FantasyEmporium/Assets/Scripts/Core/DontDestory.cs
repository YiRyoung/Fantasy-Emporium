using Unity.VisualScripting;
using UnityEngine;

public class DontDestory : MonoBehaviour
{
    public static DontDestory Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
