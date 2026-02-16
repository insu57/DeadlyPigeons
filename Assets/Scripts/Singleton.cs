using JetBrains.Annotations;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
   private  static T _instance;

   public static T Instance
   {
      get
      {
         if (!_instance)
         {
            _instance = (T)FindFirstObjectByType(typeof(T));

            if (!_instance)
            {
               GameObject singleton = new GameObject();
               _instance = singleton.AddComponent<T>();
               singleton.name = typeof(T).ToString();
               
               DontDestroyOnLoad(singleton);
            }
         }
         
         return _instance;
      }
   }

   protected void Awake()
   {
      if (!_instance)
      {
         _instance = this as T;
         DontDestroyOnLoad(gameObject);
      }
      else if (this != _instance)
      {
         Destroy(gameObject);
      }
   }
}
