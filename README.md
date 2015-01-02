SRNetworkManager
================

Network Manager for Unity 3D in C#

Usage
-----

The request can be made like this:

	WWWForm form = new WWWForm();
    form.AddField("param1", "value1");
    form.AddField("param2", "value2");
    // add or remove parameters here
		
    SRNetworkManager nm = SRNetworkManager.instance;
    nm.OnFinish += HandleOnFinish;
    nm.OnError += HandleOnError;
    nm.Push (new SRNetworkManager.Request(SRNetworkManager.API_URL, HandleFinishNetwork, form));
    nm.Execute();

And its requeset handler is:

    private void HandleFinishNetwork(WWW www) {
      // where www.text is the response text
    }
    
    private void HandleOnError (string error) {
	  Debug.LogError("Network Error occured: " + error);
	}
	  
	private void HandleOnFinish() {
	  Debug.Log("All requests executed");
	}
	  

Note: Remember to change `API_URL` at line 6 to URL of your API.
