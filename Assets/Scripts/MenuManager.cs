using System.Globalization;
using UnityEngine;

namespace Assets.Scripts {
    public class MenuManager : MonoBehaviour {

        public Color DisabledColor;
        public Color EnabledColor;
        public BoxCollider2D AddPlayerButton;
        public BoxCollider2D RemPlayerButton;
        public BoxCollider2D CloseGameButton;
        public BoxCollider2D StartGameButton;
        public TextMesh NumberOfPlayersTextMesh;

        private int _numberOfPlayers;
        // Constants to min and max players 
        private const int MAX_NUMBER_PLAYERS = 6;
        private const int MIN_NUMBER_PLAYERS = 2;

        // Use this for initialization
        private void Start() {
            _numberOfPlayers = MIN_NUMBER_PLAYERS;
            NumberOfPlayersTextMesh.text = MIN_NUMBER_PLAYERS.ToString(CultureInfo.InvariantCulture);
        }

        // Update is called once per frame
        private void Update() {
            // Set aspect ratio of Camera always the same
            Camera.main.aspect = 4f / 3f;

            // Handle buttons color
            ((SpriteRenderer) AddPlayerButton.gameObject.renderer).color = _numberOfPlayers >= MAX_NUMBER_PLAYERS
                ? DisabledColor
                : EnabledColor;
            ((SpriteRenderer) RemPlayerButton.gameObject.renderer).color = _numberOfPlayers <= MIN_NUMBER_PLAYERS
                ? DisabledColor
                : EnabledColor;

            // Check if Right Mouse Button or Touch is happening
            if(Input.GetMouseButtonDown(0)) {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if(hit.collider) {
                    if(hit.collider == AddPlayerButton) {
                        _numberOfPlayers++;
                        _numberOfPlayers = Mathf.Clamp(_numberOfPlayers, MIN_NUMBER_PLAYERS, MAX_NUMBER_PLAYERS);
                    }
                    else if(hit.collider == RemPlayerButton) {
                        _numberOfPlayers--;
                        _numberOfPlayers = Mathf.Clamp(_numberOfPlayers, MIN_NUMBER_PLAYERS, MAX_NUMBER_PLAYERS);
                        }
                    else if(hit.collider == CloseGameButton) {
                        Application.Quit();
                    }
                    else if(hit.collider == StartGameButton) {
                        GameManager.StartGame(_numberOfPlayers);
                    }
                }
            }
            NumberOfPlayersTextMesh.text = _numberOfPlayers.ToString(CultureInfo.InvariantCulture);
        }
    }
}