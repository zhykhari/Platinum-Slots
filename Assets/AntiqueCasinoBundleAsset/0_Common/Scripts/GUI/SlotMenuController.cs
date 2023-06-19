using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class SlotMenuController : MonoBehaviour
    {
        [Space(16, order = 0)]
        [SerializeField]
        private SlotController slot;

        public bool ControlActivity { get; private set; }

        #region temp vars
        private Button[] buttons;
        private SlotPlayer MPlayer { get { return SlotPlayer.Instance; } }
        private GuiController MGui { get { return GuiController.Instance; } }
        #endregion temp vars

        #region regular
        void Start()
        {
            buttons = GetComponentsInChildren<Button>();
        }
        #endregion regular

        /// <summary>
        /// Set all buttons interactble = activity
        /// </summary>
        /// <param name="activity"></param>
        public void SetControlActivity(bool activity)
        {
            ControlActivity = activity;
            if (buttons == null) return;
            foreach (Button b in buttons)
            {
              if(b)  b.interactable = activity;
            }
        }

        #region header menu
        public void Lobby_Click()
        {
            SceneLoader.Instance.LoadScene(0);
        }
        #endregion header menu

        public void Quit()
        {
#if !UNITY_IOS && !UNITY_WEBGL
            Application.Quit();
#endif
        }

        private string GetMoneyName(int count)
        {
            if (count > 1) return "coins";
            else return "coin";
        }
    }
}