using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Sound {
	[SerializeField]
	private int _id;
	[SerializeField]
	private string _name;
	public AudioClip audio;

	public int ID {
		get { return _id; }
		set { 
			if (value >= 0) {
				_id = value;
			}
		}
	}

	public string Name {
		get { return _name; }
		set { 
			if (!string.IsNullOrEmpty(value)) {
				_name = value;
			}
		}
	}

	public static Sound[] ReadNumbers(int number, List<Sound> sounds) {
		string n = number.ToString();
		List<Sound> result = new List<Sound>();
		foreach (char c in n) {
			switch (c) {
				case '1':
					result.Add(sounds[(int)SoundName.Um]);
					break;
				case '2':
					result.Add(sounds[(int)SoundName.Dois]);
					break;
				case '3':
					result.Add(sounds[(int)SoundName.Três]);
					break;
				case '4':
					result.Add(sounds[(int)SoundName.Quatro]);
					break;
				case '5':
					result.Add(sounds[(int)SoundName.Cinco]);
					break;
				case '6':
					result.Add(sounds[(int)SoundName.Seis]);
					break;
				case '7':
					result.Add(sounds[(int)SoundName.Sete]);
					break;
				case '8':
					result.Add(sounds[(int)SoundName.Oito]);
					break;
				case '9':
					result.Add(sounds[(int)SoundName.Nove]);
					break;
				case '0':
					result.Add(sounds[(int)SoundName.Zero]);
					break;
			}
		}
		return result.ToArray();
	}
}

