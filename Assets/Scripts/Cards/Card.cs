using UnityEngine;
using System.Collections;

[System.Serializable]
public class Card {
	//// Privates
	// Identification
	[SerializeField]
	private int _id = 0;
	// Others
	[SerializeField]
	private string _insideText = string.Empty;
	//// Public

	public AudioClip audio;

	public int ID {
		get { return _id; }
		set { 
			if (value >= 0) {
				_id = value;
			}
		}
	}

	public string InsideText {
		get { return _insideText; }
		set {
			if (!string.IsNullOrEmpty(value)) {
				_insideText = value;
			}
		}
	}
}
