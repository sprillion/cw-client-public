using UnityEngine;

namespace tools
{
    public class DisableTracker : MonoBehaviour
    {
        private void OnDisable()
        {
            Debug.LogError($"[DisableTracker] {gameObject.name} was disabled!\n{System.Environment.StackTrace}");
        }
    }
}