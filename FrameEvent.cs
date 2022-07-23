using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class FrameEvent {
    private static FrameEvent.Manager instance;
    public struct DefaultType {}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitFrameEvent() {
        instance = new GameObject("Frame Event Manager").AddComponent<FrameEvent.Manager>();
        GameObject.DontDestroyOnLoad(instance.gameObject);
    }

    //------------------------------------------------------------------------------------------------------------------

    public static void Trigger<T>(string channel, UnityEngine.Object objectChannel, T e) {
        instance.GetContainer<T>().Add(new Item<T>(e, channel, objectChannel));
    }

    public static void Trigger<T>(T e) => Trigger<T>(null, null, e);
    public static void Trigger<T>(UnityEngine.Object objectChannel, T e) => Trigger<T>(null, objectChannel, e);
    public static void Trigger<T>(string channel, T e) => Trigger<T>(channel, null, e);
    
    public static void Trigger(string channel, UnityEngine.Object objectChannel = null) => Trigger<DefaultType>(channel, objectChannel, default);
    public static void Trigger(UnityEngine.Object objectChannel) => Trigger<DefaultType>(null, objectChannel, default);
    public static void Trigger(string channel) => Trigger<DefaultType>(channel, null, default);

    //------------------------------------------------------------------------------------------------------------------

    private static bool When<T>(out T e, string channel = null, UnityEngine.Object objectChannel = null) {
        var container = instance.GetContainer<T>();
        var count = container.ready.Count;

        for (int i = 0; i < count; i++) {
            var item = container.ready[i];

            if (item.channel == channel && item.objectChannel == objectChannel) {
                e = item.e;
                return true;
            }
        }

        e = default;
        return false;
    }

    public static bool When<T>(out T e) => When<T>(out e, null, null);
    public static bool When<T>(UnityEngine.Object objectChannel, out T e) => When<T>(out e, null, objectChannel);
    public static bool When<T>(string channel, UnityEngine.Object objectChannel, out T e) => When<T>(out e, channel, objectChannel);
    public static bool When<T>(string channel, out T e) => When<T>(out e, channel, null);

    // When without out argument
    public static bool When<T>(string channel) => When<T>(channel, null, out var _);
    public static bool When<T>(UnityEngine.Object objectChannel) => When<T>(null, objectChannel, out var _);
    public static bool When<T>(string channel, UnityEngine.Object objectChannel) => When<T>(channel, objectChannel, out var _);

    public static bool When(string channel, UnityEngine.Object objectChannel = null) => When<DefaultType>(channel, objectChannel, out var _);
    public static bool When(UnityEngine.Object objectChannel) => When<DefaultType>(null, objectChannel, out var _);

    //------------------------------------------------------------------------------------------------------------------


    public static IEnumerable<T> WhenMany<T>(string channel = null, UnityEngine.Object objectChannel = null) {
        var container = instance.GetContainer<T>();
        container.channel = channel;
        container.objectChannel = objectChannel;
        return container;
    }

    public static IEnumerable<T> WhenMany<T>(UnityEngine.Object objectChannel) => WhenMany<T>(null, objectChannel);

    //------------------------------------------------------------------------------------------------------------------

    public static void TriggerEvent<T>(this UnityEngine.Object objectChannel, string channel, T e) => Trigger<T>(channel, objectChannel, e);
    public static void TriggerEvent<T>(this UnityEngine.Object objectChannel, T e) => Trigger<T>(null, objectChannel, e);
    public static void TriggerEvent(this UnityEngine.Object objectChannel, string channel = null) => Trigger<DefaultType>(channel, objectChannel, default);

    public static bool OnEvent<T>(this UnityEngine.Object objectChannel, string channel, out T e) => When<T>(channel, objectChannel, out e);
    public static bool OnEvent<T>(this UnityEngine.Object objectChannel, out T e) => When<T>(null, objectChannel, out e);
    public static bool OnEvent<T>(this UnityEngine.Object objectChannel) => When<T>(null, objectChannel, out var _);
    public static bool OnEvent(this UnityEngine.Object objectChannel, string channel = null) => When<DefaultType>(channel, objectChannel, out var _);

    //------------------------------------------------------------------------------------------------------------------

    public static void AddListener<T>(string channel, UnityEngine.Object objectChannel, Action<T> listener) {
        instance.GetContainer<T>().AddListener(channel, objectChannel, listener);
    }

    public static void AddListener<T>(Action<T> listener) => AddListener<T>(null, null, listener);
    public static void AddListener<T>(UnityEngine.Object objectChannel, Action<T> listener) => AddListener<T>(null, objectChannel, listener);
    public static void AddListener<T>(string channel, Action<T> listener) => AddListener<T>(channel, null, listener);

    public static void AddListener(string channel, UnityEngine.Object objectChannel, Action<DefaultType> listener) => AddListener<DefaultType>(channel, objectChannel, listener);
    public static void AddListener(UnityEngine.Object objectChannel, Action<DefaultType> listener) => AddListener<DefaultType>(null, objectChannel, listener);
    public static void AddListener(Action<DefaultType> listener) => AddListener<DefaultType>(null, null, listener);

    //------------------------------------------------------------------------------------------------------------------

    public static void RemoveListener<T>(string channel, UnityEngine.Object objectChannel, Action<T> listener) {
        instance.GetContainer<T>().RemoveListener(channel, objectChannel, listener);
    }

    public static void RemoveListener<T>(Action<T> listener) => RemoveListener<T>(null, null, listener);
    public static void RemoveListener<T>(UnityEngine.Object objectChannel, Action<T> listener) => RemoveListener<T>(null, objectChannel, listener);
    public static void RemoveListener<T>(string channel, Action<T> listener) => RemoveListener<T>(channel, null, listener);
    
    public static void RemoveListener(string channel, UnityEngine.Object objectChannel, Action<DefaultType> listener) => RemoveListener<DefaultType>(channel, objectChannel, listener);
    public static void RemoveListener(UnityEngine.Object objectChannel, Action<DefaultType> listener) => RemoveListener<DefaultType>(null, objectChannel, listener);
    public static void RemoveListener(Action<DefaultType> listener) => RemoveListener<DefaultType>(null, null, listener);

    //------------------------------------------------------------------------------------------------------------------

    public static void RemoveAllListeners<T>(string channel, UnityEngine.Object objectChannel = null) {
        instance.GetContainer<T>().RemoveAllListeners(channel, objectChannel);
    }

    public static void RemoveAllListeners<T>(UnityEngine.Object objectChannel) => RemoveAllListeners<T>(null, objectChannel);

    //------------------------------------------------------------------------------------------------------------------

    public class Container<T> : IContainer, IEnumerable<T> {
        public List<Item<T>> ready = new List<Item<T>>();
        private List<Item<T>> queue = new List<Item<T>>();
        private Dictionary<int, Action<T>> listenersHashDict = new Dictionary<int, Action<T>>();

        public string channel;
        public UnityEngine.Object objectChannel;
        public event Action<T> listenersNoChannels = _ => { };

        private ContainerEnumerator<T> enumerator;

        public Container() {
            enumerator = new ContainerEnumerator<T>(ready);
        }

        public void Add(Item<T> item) {
            queue.Add(item);
            listenersNoChannels(item.e);
            
            if (listenersHashDict.TryGetValue(item.GetChannelsHash(), out var listeners)) 
                listeners(item.e);
        }

        public void PrepareForNextFrame() {
            objectChannel = null;
            channel = null;

            ready.Clear();
            ready.AddRange(queue);
            queue.Clear();
        }

        public void AddListener(string channel, UnityEngine.Object objectChannel, Action<T> listener) {
            if (channel == null && objectChannel == null) {
                listenersNoChannels += listener;
                return;
            }

            var hash = Item<T>.GetChannelsHash(channel, objectChannel);

            if (!listenersHashDict.ContainsKey(hash))
                listenersHashDict[hash] = _ => { };

            listenersHashDict[hash] += listener;
        }

        public void RemoveListener(string channel, UnityEngine.Object objectChannel, Action<T> listener) {
            if (channel == null && objectChannel == null) {
                listenersNoChannels -= listener;
                return;
            }

            var hash = Item<T>.GetChannelsHash(channel, objectChannel);

            if (listenersHashDict.TryGetValue(hash, out var listeners)) {
                listeners -= listener;

                // if (listeners.GetInvocationList().Length <= 1)
                //     listenersHashDict.Remove(hash);
            }
        }

        public void RemoveAllListeners(string channel, UnityEngine.Object objectChannel) {
            if (channel == null && objectChannel == null) {
                foreach (var del in listenersNoChannels.GetInvocationList())
                    listenersNoChannels -= del as Action<T>;
            }
            else
                listenersHashDict.Remove(Item<T>.GetChannelsHash(channel, objectChannel));
        }

        public IEnumerator<T> GetEnumerator() {
            enumerator.objectChannel = objectChannel;
            enumerator.channel = channel;

            enumerator.Reset();
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    public class ContainerEnumerator<T> : IEnumerator<T> {
        public string channel;
        public UnityEngine.Object objectChannel;

        private List<Item<T>> ready;
        private int index = -1;

        public ContainerEnumerator(List<Item<T>> ready) {
            this.ready = ready;
        }

        public T Current => ready[index].e;
        object IEnumerator.Current => Current;

        public bool MoveNext() {
            while (++index < ready.Count) {
                var container = ready[index];
                
                if (container.channel == channel && container.objectChannel == objectChannel)
                    return true;
            }

            return index < ready.Count;
        }

        public void Reset() {
            index = -1;
        }

        public void Dispose() { }
    }

    //------------------------------------------------------------------------------------------------------------------

    public struct Item<T> {
        public T e;
        public string channel;
        public UnityEngine.Object objectChannel;

        public Item(T e, string channel = null, UnityEngine.Object objectChannel = null) {
            this.e = e;
            this.channel = channel;
            this.objectChannel = objectChannel;
        }

        public int GetChannelsHash() {
            return (channel?.GetHashCode() ?? 0) ^ (objectChannel?.GetInstanceID() ?? 0);
        }

        public static int GetChannelsHash(string channel, UnityEngine.Object objectChannel) {
            return (channel?.GetHashCode() ?? 0) ^ (objectChannel?.GetInstanceID() ?? 0);
        }
    }

    public interface IContainer {
        public void PrepareForNextFrame();
    }

    //------------------------------------------------------------------------------------------------------------------

    public class Manager : MonoBehaviour {
        public Dictionary<Type, FrameEvent.IContainer> containers = new Dictionary<Type, FrameEvent.IContainer>();

        public FrameEvent.Container<T> GetContainer<T>() {
            if (!containers.TryGetValue(typeof(T), out var container)) 
                containers.Add(typeof(T), container = new FrameEvent.Container<T>());

            return (FrameEvent.Container<T>)container;
        }

        private void LateUpdate() {
            foreach (var type in containers.Keys)
                containers[type].PrepareForNextFrame();
        }
    }
}