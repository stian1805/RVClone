using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem.UI;
#endif

namespace Unity.Multiplayer.Center.NetcodeForGameObjectsExample
{
    /// <summary>
    /// A basic example of a UI to start a host or client.
    /// If you want to modify this Script please copy it into your own project and add it to your copied UI Prefab.
    /// </summary>
    public class TemporaryUI : MonoBehaviour
    {
        [SerializeField]
        Button m_StartHostButton;
        [SerializeField]
        Button m_StartClientButton;
        [SerializeField]
        GameObject m_ButtonContainer;
        [SerializeField]
        GameObject m_PlayerInventoryPanel;
        [SerializeField]
        InventoryUIController m_InventoryUIController;

        void Awake()
        {
            m_PlayerInventoryPanel.SetActive(false);
            if (!FindAnyObjectByType<EventSystem>())
            {
                var inputType = typeof(StandaloneInputModule);
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
                inputType = typeof(InputSystemUIInputModule);                
#endif
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), inputType);
                eventSystem.transform.SetParent(transform);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_StartHostButton.onClick.AddListener(StartHost);
            m_StartClientButton.onClick.AddListener(StartClient);
        }

        void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            DeactivateButtons();
        }

        void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            DeactivateButtons();
        }

        void DeactivateButtons()
        {
            m_ButtonContainer.SetActive(false);

            if (m_InventoryUIController != null)
            {
                m_InventoryUIController.StartGameplay();
            }
        }
    }
}