using System.Collections;
using UnityEngine;
using Unity.LEGO.Game;
using Unity.LEGO.Minifig;

namespace Com.Majogames.MyLegoPUN
{
    public class MinifigInputManagerPUN : MonoBehaviour
    {
        MinifigControllerPUN m_MinifigController;
        GameFlowManager m_GameFlowManager;

        void Awake()
        {
            m_MinifigController = GetComponent<MinifigControllerPUN>();
            m_GameFlowManager = FindObjectOfType<GameFlowManager>();

            EventManager.AddListener<GameOverEvent>(OnGameOver);
            EventManager.AddListener<OptionsMenuEvent>(OnOptionsMenu);
        }

        void OnGameOver(GameOverEvent evt)
        {
            // Disable input when the game is over.
            m_MinifigController.SetInputEnabled(false);

            // If we have won, turn to the camera and do a little celebration!
            if (evt.Win)
            {
                m_MinifigController.TurnTo(Camera.main.transform.position);

                var randomCelebration = Random.Range(0, 3);
                switch (randomCelebration)
                {
                    case 0:
                        {
                            m_MinifigController.PlaySpecialAnimation(MinifigControllerPUN.SpecialAnimation.AirGuitar);
                            break;
                        }
                    case 1:
                        {
                            m_MinifigController.PlaySpecialAnimation(MinifigControllerPUN.SpecialAnimation.Flexing);
                            break;
                        }
                    case 2:
                        {
                            m_MinifigController.PlaySpecialAnimation(MinifigControllerPUN.SpecialAnimation.Dance);
                            break;
                        }
                }
            }
        }

        void OnOptionsMenu(OptionsMenuEvent evt)
        {
            // Only enable input if options menu is not active.
            // Delay update by one frame to prevent input the frame the options menu is closed.
            StartCoroutine(DoUpdateInput(!evt.Active));
        }

        IEnumerator DoUpdateInput(bool enabled)
        {
            yield return new WaitForEndOfFrame();

            m_MinifigController.SetInputEnabled(enabled && !m_GameFlowManager.GameIsEnding);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<GameOverEvent>(OnGameOver);
            EventManager.RemoveListener<OptionsMenuEvent>(OnOptionsMenu);
        }
    }
}
