using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	public int numberOfPlayers;
	[SerializeField]
	private int _actualPlayer;
	//[SerializeField]
	//private Board _board;
	[SerializeField]
	private List<Player> _players = new List<Player>();
	[SerializeField]
	private List<Card> _cards = new List<Card>();
	[SerializeField]
	private List<Sound> _sounds = new List<Sound>();
	[SerializeField]
	private bool gameStarted = false;
	private bool starting = true;
	private const int MAX_NUMBER_PLAYERS = 6;

	void Start() {
		for (int i = 0; i < 6; i++) {
			Player p = new Player();
			_players.Add(p);
		}
	}

	void Update() {

	}

	void StartGame() {
		starting = false;
		_players.Clear();
		Player.InstanceCounter = 0;
		_cards = ResourcesLoad.Cards();
		_sounds = ResourcesLoad.Sounds();
		if (numberOfPlayers > MAX_NUMBER_PLAYERS) {
			numberOfPlayers = MAX_NUMBER_PLAYERS;
		}
		for (int i = 0; i < numberOfPlayers; i++) {
			Player p = new Player();
			p.Points = 0;
			int index = Random.Range(0, _cards.Count);
			p.Card = _cards[index];
			_cards.RemoveAt(index);
			_players.Add(p);
		}
		_actualPlayer = _players[0].Number;
		gameStarted = true;
	}

	void NextPlayer() {
		_actualPlayer++;
		if (_actualPlayer > _players.Count) {
			_actualPlayer = 1;
		}
	}

	public List<Card> Cards {
		get { return _cards; }
	}

	IEnumerator ReadState(Player actualPlayer) {
		audio.PlayOneShot(actualPlayer.Audio);
		yield return new WaitForSeconds(actualPlayer.Audio.length);
		if (actualPlayer.Number != _actualPlayer) {
			audio.PlayOneShot(actualPlayer.Card.audio);
			yield return new WaitForSeconds(actualPlayer.Card.audio.length);
		}
		Sound[] points = Sound.ReadNumbers(actualPlayer.Points, _sounds);
		for (int i = 0; i < points.Length; i++) {
			audio.PlayOneShot(points[i].audio);
			yield return new WaitForSeconds(points[i].audio.length);
		}
		audio.PlayOneShot(_sounds[(int)SoundName.Pontos].audio);
		yield return new WaitForSeconds(_sounds[(int)SoundName.Pontos].audio.length);
	}

	IEnumerator Read(AudioClip[] args) {
		Debug.Log(args.Length);
		foreach (AudioClip ac in args) {
			audio.PlayOneShot(ac);
			yield return new WaitForSeconds(ac.length);
		}
	}

	void OnGUI() {
		Color defaultColor = GUI.color;
		Color defaultBackgroundColor = GUI.backgroundColor;
		// Top
		GUI.BeginGroup(new Rect(Screen.width / 2 - (Screen.width * 0.9f / 2), 0, Screen.width * 0.9f, Screen.height * 0.8f));
		{
			float topPos = 0f;
			GUI.Box(new Rect(0, topPos, Screen.width * 0.9f, Screen.height * 0.8f), "");
			{
				GUI.Label(new Rect(5, topPos, Screen.width * 0.9f, Screen.height * 0.06f), "αlphα");
				topPos += Screen.height * 0.05f;
				if (!gameStarted) {
					GUI.Label(new Rect(5, topPos, Screen.width * 0.9f, Screen.height * 0.06f), "Escolha o número de jogadores");
				} else {
					GUI.Label(new Rect(5, topPos, Screen.width * 0.9f, Screen.height * 0.06f), "Jogadores: " + _players.Count);
					topPos += Screen.height * 0.05f;
					Player actual = _players[_actualPlayer - 1];
					GUI.color = actual.Color;
					GUI.Label(new Rect(5, topPos, Screen.width * 0.9f, Screen.height * 0.06f), "Jogador atual: " + actual.Number);
					topPos += Screen.height * 0.05f;
					GUI.Label(new Rect(5, topPos, Screen.width * 0.9f, Screen.height * 0.06f), "Número de acertos: " + actual.Points);
					topPos += Screen.height * 0.05f;
				}

			}
		}
		GUI.EndGroup();
		GUI.color = defaultColor;
		// Bottom
		float currentLeftBox = 5;
		GUI.BeginGroup(new Rect(Screen.width / 2 - (Screen.width * 0.9f / 2), Screen.height - Screen.height * 0.1f, Screen.width * 0.9f, Screen.height * 0.5f));
		{
			GUI.Box(new Rect(0, 0, Screen.width * 0.9f, Screen.height * 0.15f), "");
			if (!gameStarted && starting) {
				for (int i = 0; i < MAX_NUMBER_PLAYERS; i++) {
					Player actualPlayer = _players[Mathf.Min(i, _players.Count)];
					GUI.backgroundColor = actualPlayer.Color;
					if (GUI.Button(new Rect(currentLeftBox, 5, Screen.height * 0.1f - 7, Screen.height * 0.1f - 7), actualPlayer.Number.ToString())) {
						numberOfPlayers = actualPlayer.Number;
						StartGame();
					}
					currentLeftBox += Screen.height * 0.1f;
				}
			} else {
				for (int i = 0; i < _players.Count; i++) {
					Player actualPlayer = _players[i];
					GUI.backgroundColor = actualPlayer.Color;
					if (GUI.Button(new Rect(currentLeftBox, 5, Screen.height * 0.1f - 7, Screen.height * 0.1f - 7), actualPlayer.Number.ToString())) {
						StartCoroutine(ReadState(actualPlayer));
					}
					currentLeftBox += Screen.height * 0.1f;
				}
			}
			if (gameStarted) {
				GUI.backgroundColor = defaultBackgroundColor;
				if (GUI.Button(new Rect(Screen.width * 0.9f - (Screen.height * 0.1f - 7) * 3, 5, Screen.height * 0.1f - 7, Screen.height * 0.1f - 7), "+1")) {
					_players[_actualPlayer - 1].Points++;
					StartCoroutine(Read(new AudioClip[] {
						_sounds[(int)SoundName.Mais_um_ponto_para].audio,
						_players[_actualPlayer - 1].Audio
					}));
				}
				if (GUI.Button(new Rect(Screen.width * 0.9f - Screen.height * 0.1f - 7, 5, Screen.height * 0.1f - 7, Screen.height * 0.1f - 7), ">>")) {
					NextPlayer();
					StartCoroutine(Read(new AudioClip[] { 
						_sounds[(int)SoundName.Rodada_do].audio, 
						_players[_actualPlayer - 1].Audio 
					}));
				}
			}
		}
		GUI.EndGroup();
	}
}
