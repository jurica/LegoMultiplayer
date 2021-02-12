using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace Com.Majogames.MyLegoPUN
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// Eingabefeld für den Spielernamen. Lässt den Benutzer seinen Namen eingeben, der im Spiel über dem Spieler erscheint.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private Constants

        // Store the PlayerPref Key to avoid typos
        // Speichert den PlayerPref-Key, um Tippfehler zu vermeiden
        const string playerNamePrefKey = "PlayerName";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// 
        /// MonoBehaviour-Methode, die von Unity während der Initialisierungsphase auf GameObject aufgerufen wird.
        /// </summary>
        void Start()
        {
            string defaultName = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the name of player, and save it in the PlayerPrefs for futer sessions.
        /// Legt den Namen des Spielers fest und speichert ihn in den PlayerPrefs für zukünftige Sitzungen.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value)
        {
            Debug.Log(value);
            // #Important
            if(string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }

            PhotonNetwork.NickName = value;

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }

        #endregion

    }
}
