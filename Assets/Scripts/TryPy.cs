using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IronPython;
using IronPython.Modules;
using System.IO;

public class TryPy : MonoBehaviour {

	private string pythonPath = "/scripts/python/";

	// Use this for initialization
	void Start () {
		// create the engine like last time  
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		var ScriptScope = ScriptEngine.CreateScope();  

		// Add the python library path to the engine. Note that this will
		// not work for builds; you will need to manually place the python
		// library files in a place that your code can find it at runtime.
		var paths = ScriptEngine.GetSearchPaths();
		paths.Add(Application.dataPath + "../Lib");
		paths.Add(Application.dataPath + pythonPath);
		ScriptEngine.SetSearchPaths (paths);

		GameObject car = GameObject.Find ("Car");
		Debug.Log ("C# cube : " + car.name);

		// load the assemblies for unity, using the types of GameObject  
		// and Editor so we don't have to hardcoded paths  
		ScriptEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly); 
		string example = File.ReadAllText (Application.dataPath + pythonPath + "test.py");
		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example);  
		ScriptSource.Execute(ScriptScope);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
