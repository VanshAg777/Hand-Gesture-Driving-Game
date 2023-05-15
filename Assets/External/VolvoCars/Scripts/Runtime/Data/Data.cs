using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using VolvoCars.Utility;

namespace VolvoCars.Data
{

    public abstract class GenericData : ScriptableObject
    {

        [NonSerialized] private List<DataOperator> operators = new List<DataOperator>();

        public abstract void SubscribeImmediate(Action<object> action, bool invokeActionOnSubscribe = true);
        public abstract void Subscribe(Action<object> action, bool invokeActionOnSubscribe = true);
        public abstract void Unsubscribe(Action<object> action);

        public abstract object GetValue();
        public abstract void SetValue(object obj);
        public abstract void SetValueSilent(object obj, Action<object> silentSubscriber);
        /// <summary>
        /// The data item will be reset to the current value after exiting play mode.
        /// </summary>
        public abstract void SetCurrentValueAsDefault();
        public abstract void SetDefaultValueAsValue();
        public abstract Type GetValueType();

        public abstract void TriggerUpdate();

        public void AddOperator(DataOperator dataOperator)
        {
            var type = GetValueType();
            SubscribeImmediate((v) => dataOperator.OnValueUpdate(v, type));
            dataOperator.valueOperation = SetValue;
            dataOperator.Init(type);
            operators.Add(dataOperator);
        }

    }

    public abstract class DataOperator
    {

        public abstract void Init(Type type);
        public abstract void OnValueUpdate(object value, Type type);

        public delegate void ValueOperation(object value);
        public ValueOperation valueOperation;

    }

    [Serializable]
    public class Data<T> : GenericData
    {
        #region properties

        /// <summary>
        /// Value used at start and remembered between sessions.
        /// </summary>
        [SerializeField] protected T defaultValue;

        /// <summary>
        /// Value used at runtime
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetValue(value);
            }
        }
        [SerializeField] protected T _value;


        [NonSerialized] private bool initialized = false;
        [NonSerialized] private List<Action<T>> subscribersImmediate = new List<Action<T>>();
        [NonSerialized] private List<Action<T>> subscribers = new List<Action<T>>();
        [NonSerialized] private Dictionary<object, Action<T>> callbacks = new Dictionary<object, Action<T>>();
        [NonSerialized] private Action[] actions = new Action[10]; // Action pool for main thread execution

        #endregion


        public void OnEnable() { 
            if (!initialized) {
                Application.quitting += () =>
                {
                    _value = defaultValue;
                };
                initialized = true;
            }
        }


        /// <summary>
        /// Subscribe to changes of the data item's value. The action will be executed from the same thread as the caller, which might not be the main thread.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeActionOnSubscribe">Whether the action should be invoked when subscribe is called, for init purposes. Set to false if you only need updates when the value of the data is changed.</param>
        public void SubscribeImmediate(Action<T> action, bool invokeActionOnSubscribe = true)
        {
            subscribersImmediate.Add(action);
            if (invokeActionOnSubscribe) {
                action(_value);
            }
        }

        /// <summary>
        /// Subscribe to changes of the data item's value (on the main thread).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeActionOnSubscribe">Whether the action should be invoked when subscribe is called, for init purposes. Set to false if you only need updates when the value of the data is changed.</param>
        public void Subscribe(Action<T> action, bool invokeActionOnSubscribe = true)
        {
            subscribers.Add(action);
            if (invokeActionOnSubscribe) {
                action(_value);
            }
        }

        /// <summary>
        /// Unsubscirbe both the immediate action created by SubscribeImmediate() and the normal action by Subscribe().
        /// </summary>
        /// <param name="action"></param>
        public void Unsubscribe(Action<T> action)
        {
            if (subscribersImmediate.Contains(action))
            {
                subscribersImmediate.Remove(action);
            }
            if (subscribers.Contains(action))
            {
                subscribers.Remove(action);
            }
        }

        /// <summary>
        /// Subscribe to changes of the data item's value. The action will be executed from the same thread as the caller, which might not be the main thread.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeActionOnSubscribe">Whether the action should be invoked when subscribe is called, for init purposes. Set to false if you only need updates when the value of the data is changed.</param>
        public override void SubscribeImmediate(Action<object> action, bool invokeActionOnSubscribe = true)
        {
            Action<T> a = (obj) =>
            {
                action(obj);
            };
            callbacks.Add(action, a);
            SubscribeImmediate(a, invokeActionOnSubscribe);
        }

        /// <summary>
        /// Subscribe to changes of the data item's value (on the main thread).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeActionOnSubscribe">Whether the action should be invoked when subscribe is called, for init purposes. Set to false if you only need updates when the value of the data is changed.</param>
        public override void Subscribe(Action<object> action, bool invokeActionOnSubscribe = true)
        {
            Action<T> a = (obj) =>
            {
                action(obj);
            };
            callbacks.Add(action, a);
            Subscribe(a, invokeActionOnSubscribe);
        }

        /// <summary>
        /// Unsubscirbe both the immediate action created by SubscribeImmediate() and the normal action by Subscribe().
        /// </summary>
        /// <param name="action"></param>
        public override void Unsubscribe(Action<object> action)
        {
            if (callbacks.ContainsKey(action))
            {
                Unsubscribe(callbacks[action]);
                callbacks.Remove(action);
            }
        }

        /// <summary>
        /// Get the runtime value.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return Value;
        }

        /// <summary>
        /// Update the runtime value if the parameter's type matches with the value's type.
        /// </summary>
        /// <param name="obj"></param>
        public override void SetValue(object obj)
        {
            if (obj.GetType() == typeof(T))
            {
                SetValue((T)obj);
            }
        }

        /// <summary>
        /// Silently update the runtime value if the parameter's type matches with the value's type.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="silentSubscriber">The subcriber to be skipped.</param>
        public override void SetValueSilent(object obj, Action<object> silentSubscriber)
        {
            if (obj.GetType() == typeof(T))
            {
                SetValue((T)obj, silentSubscriber);
            }
        }

        /// <summary>
        /// Get the type of the value.
        /// </summary>
        /// <returns></returns>
        public override Type GetValueType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Assign the runtime value to the default value.
        /// </summary>
        public override void SetCurrentValueAsDefault()
        {
            defaultValue = _value;
        }

        /// <summary>
        /// Assign the default value to the runtime value.
        /// </summary>
        public override void SetDefaultValueAsValue()
        {
            _value = defaultValue;
        }

        /// <summary>
        /// Update the runtime value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="silentSubscriber"></param>
        protected virtual void SetValue(T value, Action<object> silentSubscriber = null)
        {
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
            if ((IsNullable() && _value == null && value != null) || (_value != null && !_value.Equals(value)))
#pragma warning restore RECS0017 // Possible compare of value type with 'null'
            {
                _value = value;
                Trigger(value, silentSubscriber);
            }
            else
            {
                _value = value;
            }
        }

        /// <summary>
        /// Broadcast value update to all subscribers except for the skipped one.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="silentSubscriber">The subscriber to be skipped.</param>
        protected virtual void Trigger(T value, Action<object> silentSubscriber = null)
        {
            List<Action<T>> subscribersImmediateList;
            if (silentSubscriber == null || !callbacks.ContainsKey(silentSubscriber))
            {
                subscribersImmediateList = subscribersImmediate;
            }
            else
            {
                subscribersImmediateList = subscribersImmediate.Where(s => s != callbacks[silentSubscriber]).ToList();
            }
            foreach (var subscriber in subscribersImmediateList)
            {
                subscriber(value);
            }

            // Send actions to main thread
            List<Action<T>> subscribersList;
            if (silentSubscriber == null || !callbacks.ContainsKey(silentSubscriber))
            {
                subscribersList = subscribers;
            }
            else
            {
                subscribersList = subscribers.Where(s => s != callbacks[silentSubscriber]).ToList();
            }
            int index = NotUsedActionPlaceholder();
            actions[index] = () =>
            {
                foreach (var subscriber in subscribersList)
                {
                    subscriber(value);
                }
                actions[index] = null;
            };
            MainThreadUtility.Execute(actions[index]);
            
        }

        /// <summary>
        /// Get an available index from the action pool for main thread execution.
        /// </summary>
        /// <returns></returns>
        private int NotUsedActionPlaceholder()
        {
            for (int i=0; i<actions.Length; ++i)
            {
                if (actions[i] == null)
                {
                    return i;
                }
            }
            // Extend array to register the latest data manipulation
            Array.Resize(ref actions, actions.Length + 1);
            return actions.Length - 1;
        }

        /// <summary>
        /// Broadcast value update to all subscribers.
        /// </summary>
        public override void TriggerUpdate()
        {
            Trigger(_value, null);
        }

        /// <summary>
        /// Check if the value type is nullable.
        /// </summary>
        /// <returns></returns>
        protected bool IsNullable()
        {
            Type type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }
    }

}