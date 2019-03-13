using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    Button btn;
	// Use this for initialization
	void Start () {
        btn = transform.GetComponent<Button>();        
        btn.onClick.AddListener(delegate { LuaMVC.Program.SendNotification(LuaMVC.NotificationType.ChangeText, -Random.Range(1, 6)); });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
