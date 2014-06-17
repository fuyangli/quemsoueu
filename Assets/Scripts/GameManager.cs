using System;
using System.Globalization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public Button PrefabPlayerButton;
    // Int to receive number of players from static
    public static int NumberOfPlayers = -1;

    [SerializeField] private int _numberOfPlayers = 1;

    // Int to keep track of current player
    [SerializeField] private static int _actualPlayer;

    // Array of players in game
    [SerializeField] private Player[] _players;
    // Array of players in game
    [SerializeField]private Button[] _playersButtons;

    // List of cards in game
    [SerializeField] private List<Card> _deck = new List<Card>();

    // List of sounds used in game
    [SerializeField] private List<Sound> _sounds = new List<Sound>();

    // Constants to min and max players 
    private const int MAX_NUMBER_PLAYERS = 6;
    private const int MIN_NUMBER_PLAYERS = 2;

    // Use this for initialization
    private void Start() {
        if(NumberOfPlayers >= 0) {
            _numberOfPlayers = NumberOfPlayers;
        }
        // Populate List of Cards
        // Popule a lista de cartas
        _deck = ResourcesLoad.Cards();

        // Populate List of Sounds
        // Popule a lista de sons
        _sounds = ResourcesLoad.Sounds();

        // Instantiate array of players
        // Instacie uma array de jogadores
        _players = new Player[_numberOfPlayers];
        _playersButtons = new Button[_numberOfPlayers];
        float iniPos = -1f, finPos = 1f;
        for(var i = 0; i < _players.Length; i++) {
            // Get a Index of a card to give to a player
            // Pegue um index de carta randomico para dar ao player
            var indexInDeck = Random.Range(0, _deck.Count);
            // Instantiate a player
            // Instancie um player
            _players[i] = new Player{
                Points = 0,
                Card = _deck[indexInDeck]
            };
            // After give to a player, remove that card from Deck
            // Após dar uma carta para o jogador, remova ela do deck
            _deck.RemoveAt(indexInDeck);
            float xPos = Mathf.Lerp(iniPos, finPos, Mathf.InverseLerp(0, _players.Length - 1, i));
            Button playerButton = Instantiate(PrefabPlayerButton, new Vector3(xPos, 0), Quaternion.identity) as Button;
            playerButton.name = String.Format("Button (Player {0})", _players[i].Number);
            playerButton.InsideText.text = _players[i].Number.ToString(CultureInfo.InvariantCulture);
            playerButton.Background.transform.localScale = new Vector3(Mathf.Lerp(6, 3, Mathf.InverseLerp(0, _players.Length - 1, _players.Length)), 2.5f);
            playerButton.Background.color = _players[i].Color;
            _playersButtons[i] = playerButton;
        }
        // Set actual player to first player created
        // Seta o jogador atual para o primeiro jogador criado
        _actualPlayer = _players[0].Number;

    }

    // Update is called once per frame
    private void Update() {
        
        if(Input.GetMouseButtonDown(0)) {
            var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null) {
                for(int index = 0; index < _playersButtons.Length; index++) {
                    if (hit.collider == _playersButtons[index].ButtonArea) {
                        StartCoroutine(ReadState(_players[index]));
                    }
                }
            }
        }
    }

    IEnumerator ReadState(Player actualPlayer)
    {
        audio.PlayOneShot(actualPlayer.Audio);
        yield return new WaitForSeconds(actualPlayer.Audio.length);
        if (actualPlayer.Number != _actualPlayer) {
            audio.PlayOneShot(actualPlayer.Card.audio);
            yield return new WaitForSeconds(actualPlayer.Card.audio.length);
        }
        var points = Sound.ReadNumbers(actualPlayer.Points, _sounds);
        for (int i = 0; i < points.Length; i++) {
            audio.PlayOneShot(points[i].audio);
            yield return new WaitForSeconds(points[i].audio.length);
        }
        audio.PlayOneShot(_sounds[(int)SoundName.Pontos].audio);
        yield return new WaitForSeconds(_sounds[(int)SoundName.Pontos].audio.length);
    }

    // Static function to Call from Menu Manager and set the number of players
    // Função estática para ser chamada pelo Menu Manager e setar o número de jogadores
    public static void StartGame(int numberOfPlayers) {
        NumberOfPlayers = numberOfPlayers;
        Application.LoadLevel("Game");
    }
}