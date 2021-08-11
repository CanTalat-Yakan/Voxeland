using UnityEngine;

namespace Mirror
{
    [AddComponentMenu("")]
    public class CustomNetworkManager : NetworkManager
    {
        // Set by UI element UsernameInput OnValueChanged
        private string m_playername;
        public string PlayerName
        {
            get { return !string.IsNullOrEmpty(m_playername) ? m_playername.Length <= 9 ? m_playername : m_playername.Substring(0, 9) : ("Guest_" + Random.Range(1, 2048).ToString()); }
            set => m_playername = value;
        }
        private string m_playercolor;
        public string PlayerColor
        {
            get { return m_playercolor = ColorUtility.ToHtmlStringRGBA(UnityEngine.Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1)); }
            set => m_playercolor = value;
        }
        private int m_playertexture;
        public int PlayerTexture
        {
            get { return m_playertexture; }
            set => m_playertexture = value;
        }


        public struct CreatePlayerMessage : NetworkMessage
        {
            public string name;
            public string color;
            public int texture;
        }

        public override void OnStartServer()
        {
            Debug.Log("OnStartServer");
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("OnClientConnect");
            base.OnClientConnect(conn);

            // tell the server to create a player with this name and color
            conn.Send(new CreatePlayerMessage { name = PlayerName, color = "#" + PlayerColor, texture = PlayerTexture });
        }

        void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
        {
            Debug.Log(createPlayerMessage.name);
            
            // create a gameobject using the name supplied by client
            GameObject playergo = Instantiate(playerPrefab, Vector3.up * 35, Quaternion.identity);
            Player player = playergo.GetComponent<Player>();
            player.playerName = createPlayerMessage.name;
            player.playerColor = createPlayerMessage.color;
            player.playerTexture = createPlayerMessage.texture;

            // set it as the player
            NetworkServer.AddPlayerForConnection(connection, playergo);
        }
    }
}
