   using UnityEngine;

   public class ScreenManager : MonoBehaviour
   {
       void Start()
       {
           // Ana ekranı seç
           if (Display.displays.Length > 1)
           {
               Display.displays[0].Activate();
               Screen.SetResolution(1920, 1080, true);
           }
       }
   }
   