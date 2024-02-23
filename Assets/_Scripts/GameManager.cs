using System;
using System.Collections;
using LootLocker.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]private UnityEvent playerConnected;
        private IEnumerator Start()
        {
            bool connected = false;
            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (!response.success)
                {
                    Debug.Log("Error starting LootLocker session");
                    return;
                }

                Debug.Log("Successfully LootLocker Session");
                connected = true;
            });

            yield return new WaitUntil(() => connected);
            playerConnected.Invoke();
        }
    }
}