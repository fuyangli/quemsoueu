using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class Player {
	//// Statics
	// int to set number of the player
	public static int InstanceCounter = 0;
	private static readonly Color[] Colors = {
	    ResourcesLoad.ColorFrom255(191, 57, 43), //red
		ResourcesLoad.ColorFrom255(39, 174, 96), //green
		ResourcesLoad.ColorFrom255(41, 128, 185), //blue
		ResourcesLoad.ColorFrom255(241, 196, 15), //yellow
		ResourcesLoad.ColorFrom255(52, 73, 94), //dark blue
		ResourcesLoad.ColorFrom255(155, 89, 182) //magenta
	};
	//// Privates
	// Identification
	[SerializeField]
	private readonly int _number;
	[SerializeField]
	private readonly Color _color;
	// Others
	[SerializeField]
	private Cell _positionOnBoard;
	[SerializeField]
	private int _points;
	[SerializeField]
	private Card _card;
	[SerializeField]
	private readonly AudioClip _audio;

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

		_color = Colors[InstanceCounter];
		_number = ++InstanceCounter;
		_audio = Resources.Load<AudioClip>("TTS/Players/Jogador " + _number + "_pt-BR");
		_points = 0;

	}

	~Player() {
		InstanceCounter--;
	}
}
