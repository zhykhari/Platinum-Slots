using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class AchievementsController : MonoBehaviour
	{
        public List<Achievement> achievements;

        public bool HaveTargetAchieved { get; private set; }

        public Action<bool> HaveTargetAchievedEvent;
        #region temp vars

        #endregion temp vars

        public static AchievementsController Instance;
		
		#region regular
		private void Start()
		{
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (var item in achievements)
            {
                item.Load();
                item.ChangeCurrentCountEvent += (c, t) => { CheckState(); };
                item.RewardReceivedEvent += (r) => { CheckState(); };
            }
            CheckState();
        }
		#endregion regular

        private void CheckState()
        {
            bool temp = HaveTargetAchieved;
            HaveTargetAchieved = false;
            foreach (var item in achievements)
            {
                if (item.TargetAchieved && !item.RewardReceived) 
                {
                    HaveTargetAchieved = true;
                    break;
                }
            }

           // if (temp != HaveTargetAchieved)
                HaveTargetAchievedEvent?.Invoke(HaveTargetAchieved);
        }
	}
}
