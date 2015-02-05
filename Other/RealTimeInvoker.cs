using Debug = MyDebug.Debug;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



//不依赖Time.timeScale 的延时调用器
public class RealTimeInvoker : MonoBehaviour {

	private static RealTimeInvoker mInst;
	private float mRealTime = 0f;
	private List<InvokerInfo> infoList = new List<InvokerInfo>();


	public class InvokerInfo{

		public System.Action action;
		public float invokeTime;

		public InvokerInfo (System.Action action, float invokeTime)
		{
			this.action = action;
			this.invokeTime = invokeTime;
		}
	}

	public static void InvokeDelay(System.Action action,float delay){
		if (mInst == null) {
			Spawn ();
		}
		if (action != null) {
			mInst.InternalInvokeDelay (action, delay);
		}
	}

	private static void Spawn ()
	{
		GameObject go = new GameObject ("_RealTimeInvoker");
		mInst = go.AddComponent<RealTimeInvoker>();
		mInst.mRealTime = Time.realtimeSinceStartup;
	}

	private void InternalInvokeDelay(System.Action action,float delay){
		if (delay < 0) {
			delay = 0;
		}
		InvokerInfo ii = new InvokerInfo (action, Time.realtimeSinceStartup + delay);
		infoList.Add (ii);
	}

	void Update ()
	{
		mRealTime = Time.realtimeSinceStartup;
		if (infoList.Count > 0) {
			int i = 0;
			while(i < infoList.Count){
				InvokerInfo info = infoList [i];
				if (mRealTime >= info.invokeTime) {
					info.action ();
					infoList.RemoveAt (i);
				} else {
					i++;
				}
			}
		}
	}

	void OnDestroy(){
		infoList.Clear ();
		mInst = null;
	}
}
