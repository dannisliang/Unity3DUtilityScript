/*
// Messenger.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
//
// Inspired by and based on Rod Hyde's Messenger:
// http://www.unifycommunity.com/wiki/index.php?title=CSharpMessenger
//
// This is a C# messenger (notification center). It uses delegates
// and generics to provide type-checked messaging between event producers and
// event consumers, without the need for producers or consumers to be aware of
// each other. The major improvement from Hyde's implementation is that
// there is more extensive error detection, preventing silent bugs.
//
// Usage example:
// Messenger<float>.AddListener("myEvent", MyEventHandler);
// ...
// Messenger<float>.Broadcast("myEvent", 1.0f);
/*
	Writing an event listener
    void OnSpeedChanged(float speed)
    {
        this.speed = speed;
    }
    Registering an event listener
    void OnEnable()
    {
        Messenger<float>.AddListener("speed changed", OnSpeedChanged);
    }
    Unregistering an event listener
    void OnDisable()
    {
        Messenger<float>.RemoveListener("speed changed", OnSpeedChanged);
    }
	Broadcasting an event
    if (speed != lastSpeed)
    {
        Messenger<float>.Broadcast("speed changed", speed);
    }
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_REMOVE_LISTENER
//#define LOG_BROADCAST_MESSAGE
using Debug = MyDebug.Debug;
using System;
using UnityEngine;
using System.Collections.Generic;

public enum MessengerMode
{
	DONT_REQUIRE_LISTENER,
	REQUIRE_LISTENER
}

static internal class MessengerInternal
{
	public static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate> ();
	public static readonly MessengerMode DEFAULT_MODE = MessengerMode.DONT_REQUIRE_LISTENER;

	public static void OnListenerAdding (string eventType, Delegate listenerBeingAdded)
	{
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
        Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif
		if (!eventTable.ContainsKey (eventType)) {
			eventTable.Add (eventType, null);
		}
		Delegate d = eventTable [eventType];
		if (d != null && d.GetType () != listenerBeingAdded.GetType ()) {
			throw new ListenerException (string.Format ("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType ().Name, listenerBeingAdded.GetType ().Name));
		}
	}

	public static void OnListenerRemoving (string eventType, Delegate listenerBeingRemoved)
	{
#if LOG_ALL_MESSAGES || LOG_REMOVE_LISTENER
        Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif
		if (eventTable.ContainsKey (eventType)) {
			Delegate d = eventTable [eventType];
			
			
			
			
			if (d == null) {
				throw new ListenerException (string.Format ("Attempting to remove listener with for event type {0} but current listener is null.", eventType));
			} else if (d.GetType () != listenerBeingRemoved.GetType ()) {
				throw new ListenerException (string.Format ("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType ().Name, listenerBeingRemoved.GetType ().Name));
			}
		} else {
			throw new ListenerException (string.Format ("Attempting to remove listener for type {0} but Messenger doesn't know about this event type.", eventType));
		}
	}

	public static void OnListenerRemoved (string eventType)
	{
		if (eventTable [eventType] == null) {
			eventTable.Remove (eventType);
		}
		
//		Debug.Log(eventTable.Count);
//		foreach(string str in eventTable.Keys){
//			Debug.Log(str);
//		}
	}

	public static void OnBroadcasting (string eventType, MessengerMode mode)
	{
		if (mode == MessengerMode.REQUIRE_LISTENER && !eventTable.ContainsKey (eventType)) {
			throw new MessengerInternal.BroadcastException (string.Format ("Broadcasting message {0} but no listener found.", eventType));
		}
	}

	public static BroadcastException CreateBroadcastSignatureException (string eventType)
	{
		return new BroadcastException (string.Format ("Broadcasting message {0} but listeners have a different signature than the broadcaster.", eventType));
	}

	public class BroadcastException : Exception
	{
		public BroadcastException (string msg) : base(msg)
		{
		}
	}

	public class ListenerException : Exception
	{
		public ListenerException (string msg) : base(msg)
		{
		}
	}
}


// No parameters
public static class Messenger
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback callback = d as Callback;
			if (callback != null) {
				callback ();
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}

// One parameter
public static class Messenger<T>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T> handler)
	{
		
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T> callback = d as Callback<T>;
			if (callback != null) {
				callback (arg1);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}


// Two parameters
public static class Messenger<T, U>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T, U> handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T, U>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T, U> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T, U>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1, U arg2)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, arg2, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T, U> callback = d as Callback<T, U>;
			if (callback != null) {
				callback (arg1, arg2);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}


// Three parameters
public static class Messenger<T, U, V>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T, U, V> handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T, U, V> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, arg2, arg3, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T, U, V> callback = d as Callback<T, U, V>;
			if (callback != null) {
				callback (arg1, arg2, arg3);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}

public static class Messenger<T, U, V, K>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T, U, V, K> handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T, U, V, K> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, arg2, arg3, arg4, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T, U, V, K> callback = d as Callback<T, U, V, K>;
			if (callback != null) {
				callback (arg1, arg2, arg3, arg4);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}

public static class Messenger<T, U, V, K,Z>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T, U, V, K,Z> handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K,Z>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T, U, V, K,Z> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K,Z>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4, Z arg5)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, arg2, arg3, arg4, arg5, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4, Z arg5, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T, U, V, K,Z> callback = d as Callback<T, U, V, K,Z>;
			if (callback != null) {
				callback (arg1, arg2, arg3, arg4, arg5);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}

public static class Messenger<T, U, V, K,Z,X,Y>
{
	private static Dictionary<string, Delegate> eventTable = MessengerInternal.eventTable;

	public static void AddListener (string eventType, Callback<T, U, V, K,Z,X,Y> handler)
	{
		MessengerInternal.OnListenerAdding (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K,Z,X,Y>)eventTable [eventType] + handler;
	}

	public static void RemoveListener (string eventType, Callback<T, U, V, K,Z,X,Y> handler)
	{
		MessengerInternal.OnListenerRemoving (eventType, handler);
		eventTable [eventType] = (Callback<T, U, V, K,Z,X,Y>)eventTable [eventType] - handler;
		MessengerInternal.OnListenerRemoved (eventType);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4, Z arg5, X arg6, Y arg7)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		Broadcast (eventType, arg1, arg2, arg3, arg4, arg5, arg6, arg7, MessengerInternal.DEFAULT_MODE);
	}

	public static void Broadcast (string eventType, T arg1, U arg2, V arg3, K arg4, Z arg5, X arg6, Y arg7, MessengerMode mode)
	{
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
		MessengerInternal.OnBroadcasting (eventType, mode);
		Delegate d;
		if (eventTable.TryGetValue (eventType, out d)) {
			Callback<T, U, V, K,Z,X,Y> callback = d as Callback<T, U, V, K,Z,X,Y>;
			if (callback != null) {
				callback (arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			} else {
				throw MessengerInternal.CreateBroadcastSignatureException (eventType);
			}
		}
	}
}
