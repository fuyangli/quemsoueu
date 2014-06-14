using System.Globalization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public TextMesh TextMesh;

    // Int to receive number of players from static
    public static int NumberOfPlayers = -1;

    [SerializeField] private int _numberOfPlayers = 1;

    // Int to keep track of current player
    [SerializeField] private static int _actualPlayer;

    // Array of players in game
    [SerializeField] private Player[] _players;

    // List of cards in game
    [SerializeField] private List<Card> _deck = new List<Card>();

    // List of sounds used in game
    [SerializeField] private List<Sound> _sounds = new List<Sound>();

    // Constants to min and max players 
    private const int MAX_NUMBER_PLAYERS = 6;
    private const int MIN_NUMBER_PLAYERS = 2;

    // Use this for initialization
    private void Start() {
        
        // Populate List of Cards
        // Popule a lista de cartas
        _deck = ResourcesLoad.Cards();

        // Populate List of Sounds
        // Popule a lista de sons
        _sounds = ResourcesLoad.Sounds();

        // Instantiate array of players
        // Instacie uma array de jogadores
        _players = new Player[_numberOfPlayers];
        for(var i = 0; i < _players.Length; i++) {
            // Get a Index of a card to give to a player
            // Pegue um index de carta randomico para dar ao player
            int indexInDeck = Random.Range(0, _deck.Count);
            // Instantiate a player
            // Instancie um player
            _players[i] = new Player{
                Points = 0,
                Card = _deck[indexInDeck]
            };
            // After give to a player, remove that card from Deck
            // Após dar uma carta para o jogador, remova ela do deck
            _deck.RemoveAt(indexInDeck);
        }
        // Set actual player to first player created
        // Seta o jogador atual para o primeiro jogador criado
        _actualPlayer = _players[0].Number;

    }

    // Update is called once per frame
    private void Update() {
        // Set aspect ratio of Camera always the same
        // Seta o aspecto da camera sempre para o mesmo
        Camera.main.aspect = 4f / 3f;
    }

    // Static function to Call from Menu Manager and set the number of players
    // Função estática para ser chamada pelo Menu Manager e setar o número de jogadores
    public static void StartGame(int numberOfPlayers) {
        NumberOfPlayers = numberOfPlayers;
        Application.LoadLevel("Game");
    }
}