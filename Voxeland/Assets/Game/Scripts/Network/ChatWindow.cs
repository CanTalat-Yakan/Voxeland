using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mirror
{
    public class ChatWindow : MonoBehaviour
    {
        public static ChatWindow Instance { get; private set; }

        public GameObject chatBox;
        public TMP_InputField chatMessage;
        public TextMeshProUGUI chatHistory;
        public TextMeshProUGUI chatTmp;
        public Scrollbar scrollbar;

        void Awake()
        {
            Player.OnMessage += OnPlayerMessage;

            if (Instance is null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        void OnDestroy()
        {
            Player.OnMessage -= OnPlayerMessage;

            Instance = null;
        }
        void Start()
        {
            SetChatAcitve(false);
        }
        void Update()
        {
            HandleInput();
        }

        //Is called by the player when receiving or sending a message
        void OnPlayerMessage(Player player, string message)
        {
            string prettyMessage = player.isLocalPlayer ?
                $"<color={player.playerColor}>{player.playerName}: </color> {message}" :
                $"<color={player.playerColor}>{player.playerName}: </color> {message}";
            AppendMessage(prettyMessage);

            // Debug.Log(message);
        }
        //For Server Calls
        public void OnServerMessage(string message)
        {
            string prettyMessage = $"Server: {message}";
            AppendMessage(prettyMessage);
        }

        //Is used for sending a Chatmessage with given UI Elements Information
        public void OnSend()
        {
            if (chatMessage.text.Trim() == "")
                return;

            // get our player
            Player player = GameManager.Instance.m_Player.GetComponent<Player>();
            // Player player = NetworkClient.connection.identity.GetComponent<Player>();

            // send a message
            string s = chatMessage.text.Trim();
            player.CmdSend(s.Length <= 200 ? s : s.Substring(0, 200) + "...");

            chatMessage.text = "";

            // Debug.Log("OnSend");
        }

        //Checks Input if now writing in input field, sending, defocus or hide
        void HandleInput()
        {
            if ((!chatMessage.interactable && Input.GetKeyDown(KeyCode.Y)))
            {
                SetChatAcitve(false);
                chatBox.SetActive(!chatBox.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!chatMessage.interactable)
                {
                    SetChatAcitve(true);
                    return;
                }

                if (!string.IsNullOrEmpty(chatMessage.text))
                    OnSend();

                SetChatAcitve(false);
            }
        }
        void SetChatAcitve(bool _b)
        {
            if (_b)
            {
                GameManager.Instance.LOCKED = true;

                chatTmp.text = "Enter Text...";

                chatBox.SetActive(true);

                chatMessage.interactable = true;
                chatMessage.ActivateInputField();
                chatMessage.Select();
            }
            else
            {
                GameManager.Instance.LOCKED = false;
                chatTmp.text = "Press Return to Chat";

                chatMessage.interactable = false;
                chatMessage.DeactivateInputField();
            }
        }

        internal void AppendMessage(string message)
        {
            StartCoroutine(AppendAndScroll(message));
        }

        //Sets the TMPro of the Protocol by appending and updating the ui
        IEnumerator AppendAndScroll(string message)
        {
            chatHistory.text += message + "\n";

            chatBox.SetActive(false);
            chatBox.SetActive(true);

            // it takes 2 frames for the UI to update ?!?!
            yield return null;
            yield return null;

            // slam the scrollbar down
            scrollbar.value = 0;
        }
    }
}
