using UnityEngine;

public class DoNotDestroyOnLoad : MonoBehaviour
{
  private void Awake()
  {
    DontDestroyOnLoad(transform.gameObject);
  }
}
