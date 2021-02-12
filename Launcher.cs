using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Com.Majogames.MyLegoPUN
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// Die maximale Anzahl von Spielern pro Raum. Wenn ein Raum voll ist, können ihm keine neuen Spieler mehr beitreten, und es wird ein neuer Raum erstellt.
        /// </summary>
        [Tooltip("Die maximale Anzahl von Spielern pro Raum. Wenn ein Raum voll ist, können ihm keine neuen Spieler mehr beitreten, und es wird ein neuer Raum erstellt.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        /// <summary>
        /// The UI Panel to let the user enter name, connect and play.
        /// </summary>
        [Tooltip("Das UI-Panel, damit der Benutzer seinen Namen eingeben, sich verbinden und spielen kann.")]
        [SerializeField]
        private GameObject controlPanel;

        /// <summary>
        /// The UI Label to inform the User that the connction is in progress
        /// </summary>
        [Tooltip("Das UI-Label, um den Benutzer darüber zu informieren, dass die Verbindung hergestellt wird.")]
        [SerializeField]
        private GameObject progressLabel;

        #endregion

        #region Private Fields

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// 
        /// Versionsnummer
        /// </summary>
        private string gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        private bool isConnecting;

        private bool wasConnected;

        #endregion

        #region MonoBehavior CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// 
        /// MonoBehaviour-Methode, die von Unity während der frühen Initialisierungsphase auf GameObject aufgerufen wird.
        /// </summary>
        private void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            // #Kritisch
            // Dies stellt sicher, dass wir PhotonNetwork.LoadLevel() auf dem Master-Client verwenden können und alle Clients im selben Raum ihren Pegel automatisch synchronisieren
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// 
        /// MonoBehaviour-Methode, die von Unity während der Initialisierungsphase auf GameObject aufgerufen wird.
        /// </summary>
        void Start()
        {
            //Connect();
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// 
        /// Starten Sie den Verbindungsvorgang.
        /// - Wenn bereits verbunden, wird versucht, einem zufälligen Raum beizutreten.
        /// - wenn noch nicht verbunden, verbinden wir diese Anwendungsinstanz mit dem Photon Cloud Network.
        /// </summary>
        public void Connect()
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);            

            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                // #Kritisch müssen wir an dieser Stelle versuchen, einem Zufallsraum beizutreten. Wenn dies fehlschlägt, werden wir in OnJoinRandomFailed() benachrichtigt und wir erstellen einen.
                PhotonNetwork.JoinRandomRoom();
                wasConnected = true;
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                // #Kritisch, wir müssen zunächst eine Verbindung zum Photon Online Server herstellen.
                // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public void Disconnect()
        {
            PhotonNetwork.Disconnect();
            wasConnected = false;
        }

        public void Reconnect()
        {
            if (!PhotonNetwork.IsConnected && wasConnected)
            {
                PhotonNetwork.ReconnectAndRejoin();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        #endregion

        #region MonoBehaviourPunCallbacks CallBacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basic Tutorial/Launcher: OnConnectToMaster() was called by PUN");

            // we don't want to do anything if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                // #Kritisch: Als erstes versuchen wir, einem potentiell existierenden Raum beizutreten. Wenn es einen gibt, gut, sonst werden wir mit OnJoinRandomFailed() zurückgerufen.
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            isConnecting = false;

            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basic Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. No random room available, so we create one. \nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            // #Kritisch: Wir konnten einem zufälligen Raum nicht beitreten, vielleicht existiert keiner oder sie sind alle voll. Keine Sorge, wir erstellen einen neuen Raum.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            // #Kritisch: Wir laden nur, wenn wir der erste Spieler sind, ansonsten verlassen wir uns auf `PhotonNetwork.AutomaticallySyncScene`, um unsere Instanzszene zu synchronisieren.
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");

                // #Critical
                // Load the Room Level.
                //PhotonNetwork.LoadLevel("Room for 1");
                PhotonNetwork.LoadLevel("Camp Fire");
            }
        }

        #endregion

    }
}

