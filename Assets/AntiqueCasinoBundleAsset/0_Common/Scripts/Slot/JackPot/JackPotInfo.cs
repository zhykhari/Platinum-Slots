using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class JackPotInfo : MonoBehaviour
    {
        [SerializeField]
        private string jpName;
        [SerializeField]
        private string machineID;
        [SerializeField]
        private long defaultValue;
        [SerializeField]
        public Text amountText;
        [SerializeField]
        public TextMesh amountTextMesh;
        [SerializeField]
        private string coinsFormat = "0,0";

        public long Amount { get; private set; }


        void Start()
        {
            Amount = GetAmount();
            GameEvents.ChangeJackpotEvent += ChangeAmountEventHandler;
            RefreshOutput(Amount);
        }

        private void OnDestroy()
        {
            GameEvents.ChangeJackpotEvent -= ChangeAmountEventHandler;
        }

        public long GetAmount()
        {
            return JackPot.GetJPAmount(machineID, jpName, defaultValue);
        }

        private void ChangeAmountEventHandler(string machine_id, string jp_name, long amount)
        {
            if(string.Equals(machine_id, machineID, System.StringComparison.Ordinal) && (string.Equals(jp_name, jpName, System.StringComparison.Ordinal)))
            {
                Amount = amount;
                RefreshOutput(Amount);
            }
        }

        private void RefreshOutput(long newAmount)
        {
            if (this && amountText) amountText.text = newAmount.ToString(coinsFormat);
            if (this && amountTextMesh) amountTextMesh.text = newAmount.ToString(coinsFormat);
        }
    }
}