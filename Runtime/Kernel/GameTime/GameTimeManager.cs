﻿using UnityEngine;

namespace Morpheus.GameTime
{
    public sealed class GameTimeManager
    {
        #region Singleton
        public static GameTimeManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new GameTimeManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
        #endregion

        private static readonly GameTimeInfo TimeInfo = new GameTimeInfo();

        public static float RealTimeSinceStartup => TimeInfo.RealTimeSinceStartup;
        public static float FixedDeltaTime => TimeInfo.FixedDeltaTime;
        public static float UnscaledDeltaTime => TimeInfo.UnscaledDeltaTime;
        public static float DeltaTime => TimeInfo.DeltaTime;

        // NOTE:
        // Stack pool makes memory fragmentation(I guess),
        // and it'll take 5 times longer than use new objects in 1 million quantity.
        // private Dictionary<Type, Stack<TimerBase>> timerPools = new Dictionary<Type, Stack<TimerBase>>(4);

        private GameTimerBase headNode;
        private GameTimerBase tailNode;

        private GameTimeManager() { }

        public void Clear()
        {
            GameTimerBase currNode = headNode;
            while (currNode != null)
            {
                GameTimerBase nextNode = currNode.NextNode;
                currNode.Reset();
                currNode.NextNode = null;
                currNode = nextNode;
            }
            headNode = null;
            tailNode = null;
        }

        public void FixedUpdate()
        {
            TimeInfo.FixedDeltaTime = Time.fixedDeltaTime;
        }

        // NOTE: This update MUST BE before other scripts.
        public void Update()
        {
            // NOTE: Access Time.deltaTime in loop is slower.
            TimeInfo.RealTimeSinceStartup = Time.realtimeSinceStartup;
            TimeInfo.UnscaledDeltaTime = Time.unscaledDeltaTime;
            TimeInfo.DeltaTime = Time.deltaTime;

            GameTimerBase prevNode = null;
            GameTimerBase currNode = headNode;
            while (currNode != null)
            {
                if (currNode.IsStop)
                {
                    // Remove node.
                    if (prevNode == null)
                    {
                        headNode = currNode.NextNode; // NOTE: NextNode can't be self.
                    }
                    else
                    {
                        prevNode.NextNode = currNode.NextNode;
                    }

                    // Set last node.
                    if (currNode == tailNode)
                    {
                        tailNode = prevNode;
                    }

                    // Move to next node.
                    GameTimerBase nextNode = currNode.NextNode;
                    currNode.NextNode = null;
                    currNode = nextNode;
                    continue;
                }

                currNode.Tick(TimeInfo);
                prevNode = currNode;
                currNode = currNode.NextNode;
            }
        }

        internal void AddLast(GameTimerBase timer)
        {
            if (timer.NextNode != null || timer == tailNode)
            {
                return;
            }

            if (headNode == null)
            {
                headNode = timer;
                tailNode = timer;
            }
            else
            {
                tailNode.NextNode = timer;
                tailNode = timer;
            }
        }
    }
}