﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    09.07.2020 - first
    23.07.2020 - add DelayAction
    12.10.2020 - tween color
    12.09.2022 - fix tween value calculation error
 */
namespace Mkey
{
    public class TweenIntValue
    {
        private int tValue;
        private int tweenId;
        private float maxTweenTime;
        private float minTweenTime;
        private Action<int> onUpdate;
        private GameObject g;
        private bool onlyPositive;

        public TweenIntValue(GameObject g, int initValue, float minTweenTime, float maxTweenTime, bool onlyPositive, Action<int> onUpdate)
        {
            tValue = initValue;
            this.maxTweenTime = Mathf.Max(0, maxTweenTime);
            this.minTweenTime = Mathf.Clamp(minTweenTime, 0, maxTweenTime);
            this.g = g;
            this.onlyPositive = onlyPositive;
            this.onUpdate = onUpdate;
        }

        public void Tween(int newValue, int valuePerSecond)
        {
            SimpleTween.Cancel(tweenId, false);
            float add = newValue - tValue;

            if ((add > 0 && onlyPositive) || !onlyPositive)
            {
                valuePerSecond = Mathf.Max(1, valuePerSecond);
                float tT = Mathf.Abs((float)add / (float)valuePerSecond);
                tT = Mathf.Clamp(tT, minTweenTime, maxTweenTime);
                int oldValue = tValue;
                tweenId = SimpleTween.Value(g, 0, 1, tT).SetOnUpdate((float val) =>
                {
                    if (this != null)
                    {
                        tValue = MathMk.Lerp( oldValue, newValue, val);
                        onUpdate?.Invoke(tValue);
                    }
                }).AddCompleteCallBack(()=> { tValue = newValue; onUpdate?.Invoke(tValue); }).ID;
            }
            else if (onlyPositive)
            {
                if (this != null)
                {
                    tValue = newValue;
                    onUpdate?.Invoke(tValue);
                }
            }
        }
    }

    public class TweenLongValue
    {
        private long tValue;
        private int tweenId;
        private float maxTweenTime;
        private float minTweenTime;
        private Action<long> onUpdate;
        private GameObject g;
        private bool onlyPositive;

        public TweenLongValue(GameObject g, long initValue, float minTweenTime, float maxTweenTime, bool onlyPositive, Action<long> onUpdate)
        {
            tValue = initValue;
            this.maxTweenTime = Mathf.Max(0, maxTweenTime);
            this.minTweenTime = Mathf.Clamp(minTweenTime, 0, maxTweenTime);
            this.g = g;
            this.onlyPositive = onlyPositive;
            this.onUpdate = onUpdate;
        }

        public void Tween(long newValue, long valuePerSecond)
        {
            SimpleTween.Cancel(tweenId, false);
            long add = newValue - tValue;

            if ((add > 0 && onlyPositive) || !onlyPositive)
            {
                valuePerSecond = Math.Max(1, valuePerSecond);
                float tT = Mathf.Abs((float)add / (float)valuePerSecond);
                tT = Mathf.Clamp(tT, minTweenTime, maxTweenTime);
                long oldValue = tValue;
                tweenId = SimpleTween.Value(g, 0, 1, tT).SetOnUpdate((float val) =>
                {
                    if (this != null)
                    {
                        tValue = MathMk.Lerp(oldValue, newValue, val);
                        onUpdate?.Invoke(tValue);
                    }
                }).AddCompleteCallBack(() => { tValue = newValue; onUpdate?.Invoke(tValue); }).ID;
            }
            else if (onlyPositive)
            {
                if (this != null)
                {
                    tValue = newValue;
                    onUpdate?.Invoke(tValue);
                }
            }
        }
    }

    public class TweenExt
    {
        public static void DelayAction(GameObject g, float delay, Action completeCallBack)
        {
            SimpleTween.Value(g, 0, 1, delay).AddCompleteCallBack(completeCallBack);
        }

        public static void TweenColor(GameObject g,  Color start, Color end, float tweenTime, float delayTime, Action<Color> onUpdate, Action completeCallBack)
        {
            SimpleTween.Value(g, 0f, 1f, tweenTime).SetOnUpdate((float val)=> {onUpdate?.Invoke(Color.Lerp( start, end, val)); }).AddCompleteCallBack(completeCallBack).SetDelay(delayTime);
        }
    }
}
