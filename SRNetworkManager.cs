using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SRNetworkManager: MonoBehaviour, System.IDisposable {
	public const string API_URL = "http://example.com/api.php";
	
	public delegate void NetworkDelegate(WWW www);
	public delegate void NetworkErrorDelegate(string error);
	public delegate void NetworkFinishDelegate();
	
	public const float TIME_OUT = 20f;
	
	public event NetworkErrorDelegate OnError;
	public event NetworkFinishDelegate OnFinish;
	
	Queue<Request> requests;
	WWW _www;
	bool isNetworking = false;
	TimeOut timeout;
	
	private static SRNetworkManager _instance;

	public class Request {
		public string url;
		public NetworkDelegate del;
		public WWWForm form;
		public byte[] bytes;
		public Dictionary<string, string> header;
		
		// Constructors
		public Request(string url, NetworkDelegate del) {
			this.url = url;
			this.del = del;
		}
		
		public Request(string url, NetworkDelegate del, WWWForm form) : this(url, del) {
			this.form = form;
		}
		
		public Request(string url, NetworkDelegate del, byte[] bytes) : this(url, del) {
			this.bytes = bytes;
		}
		
		public Request(string url, NetworkDelegate del, byte[] bytes, Dictionary<string, string> header) : this(url, del, bytes) {
			this.header = header;
		}
		
		public WWW makeWWW() {
			if(header != null) {
				return new WWW(url, bytes, header); // problematic line
			}
			if(bytes != null) {
				return new WWW(url, bytes);
			}
			if(form != null) {
				return new WWW(url, form);
			}
			return new WWW(url);
		}
	}
	
	class TimeOut {
		float beforeProgress;
		float beforeTime;
		
		public bool CheckTimeout(float progress) {
			float now = Time.time;
			if((now - beforeTime) >  TIME_OUT) {
				return true; // time out
			}
			if(beforeProgress != progress) {
				beforeProgress = progress;
				beforeTime = now;
			}
			return false;
		}
	}
	
	public static SRNetworkManager instance {
		get {
			if(_instance == null) {
				GameObject go = new GameObject();
				go.name = "SRNetworkManager";
				_instance = go.AddComponent<SRNetworkManager>();
				DontDestroyOnLoad(go);
			}
			return _instance;
		}
	}
	
	// Life Cycle
	void Awake() {
		requests = new Queue<Request>();
		timeout = new TimeOut();
	}
	void Destroy() {
		Debug.Log("SRNetworkManager: Destroy");
		this.Dispose();
		_instance = null;
	}
	public void Dispose() {
		Debug.Log("SRNetworkManager: Dispose");
		isNetworking = false;
		StopAllCoroutines();
		if(_www != null) {
			_www.Dispose();
		}
		requests.Clear();
		OnError = null;
		OnFinish = null;
	}
	void FixedUpdate() {
		if(!isNetworking) {
			this.enabled = false;
		}
		if(timeout.CheckTimeout(CurrentProgress)) {
			if(OnError != null) {
				OnError("timeout");
			}
			this.Dispose();
		}
	}
	
	// Public
	public void Push(Request req) {
		requests.Enqueue(req);
	}
	
	public void Execute() {
		if(isNetworking) {
			Debug.Log("Already downloading...");
			return;
		}
		StartCoroutine("Download");
	}
	
	public float CurrentProgress {
		get {
			if(_www == null) {
				return 0f;
			}
			return _www.progress;
		}
	}
	
	// Private
	IEnumerator Download() {
		if(requests.Count == 0) {
			Debug.LogWarning("no requests");
			yield return true;
		}
		this.isNetworking = true;
		this.enabled = true;
		while(requests.Count > 0) {
			Request req = requests.Dequeue();
			_www = req.makeWWW();
			yield return _www;
			if(_www.error != null && _www.error.Length > 0) {
				if(OnError != null) {
					OnError(_www.error);
				}
			}
			req.del(_www);
		}
		if(OnFinish != null) {
			OnFinish();
		}
		this.isNetworking = false;
		this.enabled = false;
	}
}