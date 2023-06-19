using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class WinCountAchievement : Achievement
	{
        #region regular
        public override void Load()
		{
            LoadRewardReceived();
            LoadCurrentCount();

            if (!TargetAchieved)
            {
                GameEvents.WinEvent += WinCountEventHandler;
            }

            RewardReceivedEvent +=(r)=> 
            {
                MPlayer.AddCoins(r);
            };
          
            ChangeCurrentCountEvent += (cc, tc)=>{};
        }

        private void OnDestroy()
        {
            GameEvents.WinEvent -= WinCountEventHandler;
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "wincount";
        }

        private void WinCountEventHandler(string id)
        {
            if (machineIDs == null || machineIDs.Count == 0 || machineIDs.Contains(id))
            {
                IncCurrentCount();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WinCountAchievement))]
    public class WinCountAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            WinCountAchievement t = (WinCountAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
