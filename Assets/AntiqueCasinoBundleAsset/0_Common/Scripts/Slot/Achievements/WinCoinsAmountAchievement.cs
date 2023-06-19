using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class WinCoinsAmountAchievement : Achievement
	{
        #region regular
        public override void Load()
		{
            LoadRewardReceived();
            LoadCurrentCount();

            if (!TargetAchieved)
            {
                GameEvents.WinCoinsEvent += WinCoinsAmountEventHandler;
            }
            RewardReceivedEvent +=(r)=> 
            {
                MPlayer.AddCoins(r);
            };
          
            ChangeCurrentCountEvent += (cc, tc)=>{};
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "wincoinsamount";
        }

        private void WinCoinsAmountEventHandler(string id, int amount)
        {
            if (machineIDs == null || machineIDs.Count == 0 || machineIDs.Contains(id))
            {
                IncCurrentCount(amount);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WinCoinsAmountAchievement))]
    public class WinCoinsAmountAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            WinCoinsAmountAchievement t = (WinCoinsAmountAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
