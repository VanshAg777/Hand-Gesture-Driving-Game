using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace VolvoCars.Utility
{

    public class MainThreadUtility : MonoBehaviour
    {

        private static MainThreadUtility instance = null;
        private static readonly Queue<Action> queue = new Queue<Action>();
        private static System.Threading.Thread mainThread;

        /// <summary>
        /// Adds action for execution on main thread.
        /// </summary>
        /// <param name="action">Action to be queued.</param>
        public static void Execute(Action action)
        {
            lock (queue)
            {
                if (IsMainThread())
                {
                    action.Invoke();
                }
                else
                {
                    queue.Enqueue(action);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static MainThreadUtility GetInstance()
        {
            if (instance == null)
            {
                CreateInstance();
            }
            return instance;
        }

        private static void CreateInstance()
        {
            var g = new GameObject("MainThreadUtility");
            instance = g.AddComponent<MainThreadUtility>();
            DontDestroyOnLoad(g);
        }

        private static bool IsMainThread()
        {
            if (mainThread == null) return false;

            return mainThread.Equals(System.Threading.Thread.CurrentThread);
        }

        private void Awake()
        {
            mainThread = System.Threading.Thread.CurrentThread;
        }

        private void Update()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    queue.Dequeue().Invoke();
                }
            }
        }

        private void FixedUpdate()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    queue.Dequeue().Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

    }

}