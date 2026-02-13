using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemCoreSystem;

namespace SynapseSystem
{
    public interface IEvent { }
    
    public class Synapse : AKeySystem
    {
        private Dictionary<Type, object> m_callbackHolders = new();

        public override Task OnDeinitialize()
        {
            m_callbackHolders.Clear();
            return Task.CompletedTask;
        }
        
        public void BroadcastEvent<T>(in T message) where T : struct, IEvent
        {
            CallbackHolder<T> callbackHolder = GetCallbackHolder<T>();
            callbackHolder.TriggerCallbacks(message);
        }

        public void SubscribeToEvent<T>(Action<T> callback, bool callOnlyOnce = false) where T : struct, IEvent
        {
            CallbackHolder<T> callbackHolder = GetCallbackHolder<T>();
            callbackHolder.SubscribeToEvent(callback, callOnlyOnce);
        }

        public void UnsubscribeFromEvent<T>(Action<T> callback) where T : struct, IEvent
        {
            CallbackHolder<T> callbackHolder = GetCallbackHolder<T>();
            callbackHolder.UnsubscribeFromEvent(callback);
        }

        private CallbackHolder<T> GetCallbackHolder<T>() where T : struct, IEvent
        {
            if (!m_callbackHolders.ContainsKey(typeof(T)))
            {
                m_callbackHolders.Add(typeof(T), new CallbackHolder<T>());
            }

            return m_callbackHolders[typeof(T)] as CallbackHolder<T>;
        }

        private class CallbackHolder<T> where T : struct, IEvent
        {
            private List<Action<T>> m_callbacks = new();
            private List<Action<T>> m_singleUseCallbacks = new();

            public void SubscribeToEvent(Action<T> callback, bool callOnlyOnce)
            {
                if (callOnlyOnce)
                {
                    m_singleUseCallbacks.Add(callback);
                }
                else
                {
                    m_callbacks.Add(callback);   
                }
            }

            public void UnsubscribeFromEvent(Action<T> callback)
            {
                m_callbacks.Remove(callback);
            }

            public void TriggerCallbacks(T message)
            {
                //Debug.Log($"[Synapse] Triggered event {typeof(T).Name}");
                
                foreach (var callback in m_callbacks)
                {
                    callback?.Invoke(message);
                }

                foreach (var callback in m_singleUseCallbacks)
                {
                    callback?.Invoke(message);
                }
                m_singleUseCallbacks.Clear();
            }
        }
    }
}