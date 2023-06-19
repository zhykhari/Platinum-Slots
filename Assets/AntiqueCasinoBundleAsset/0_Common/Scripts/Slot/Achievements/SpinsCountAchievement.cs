using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mkey
{
	public class SpinsCountAchievement : Achievement
	{

        #region regular
        public override void Load()
		{
            LoadRewardReceived();
            LoadCurrentCount();

            if (!TargetAchieved)
            {
                GameEvents.SpinEvent += SpinCountEventHandler;
            }

            RewardReceivedEvent +=(r)=> 
            {
                MPlayer.AddCoins(r);
            };
          
            ChangeCurrentCountEvent += (cc, tc)=>{};
        }

        private void OnDestroy()
        {
            GameEvents.SpinEvent -= SpinCountEventHandler;
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "spinscount";
        }

        private void SpinCountEventHandler(string id)
        {
            if (machineIDs == null || machineIDs.Count == 0 || machineIDs.Contains(id))
            {
                IncCurrentCount();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpinsCountAchievement))]
    public class SpinsCountAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SpinsCountAchievement t = (SpinsCountAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
