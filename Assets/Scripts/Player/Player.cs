using UnityEngine;
using System.Collections;

[System.Serializable]
public class Player {
	//// Statics
	// int to set number of the player
	public static int instanceCounter = 0;
	private static Color[] colors = {
		new Color(1, 0, 0),
		new Color(0, 1, 0),
		new Color(0, 0, 1),
		new Color(1, 1, 0),
		new Color(0, 1, 1),
		new Color(1, 0, 1)
	};
	//// Privates
	// Identification
	[SerializeField]
	private int _number;
	[SerializeField]
	private Color _color;
	// Others
	[SerializeField]
	private Cell _positionOnBoard;
	[SerializeField]
	private int _points;
	[SerializeField]
	private Card _card;
	[SerializeField]
	private AudioClip _audio;

	public int Number {
		get { return _number; }
	}

	public Color Color {
		get{ return _color; }
	}

	public int Points {
		get { return _points; }
		set {
			if (value > 0)
				_points = value;
		}
	}

	public Card Card {
		get { return _card; }
		set {
			_card = value;
		}
	}

	public Cell PositionOnBoard {
		get { return _positionOnBoard; }
	}

	public AudioClip Audio {
		get { return _audio; }
	}
	// Constructor
	public Player() {
		if (instanceCounter > 6) {
			return;
		}
		this._color = colors[instanceCounter];
		this._number = ++instanceCounter;
		this._audio = Resources.Load<AudioClip>("TTS/Players/Jogador " + _number + "_pt-BR");
		this._points = 0;

	}

	~Player() {
		instanceCounter--;
	}
}
