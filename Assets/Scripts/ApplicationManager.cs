using UnityEngine;
using System.Collections;

public class ApplicationManager : MonoBehaviour {

    public BoxCollider2D CloseGameButton;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        // Set aspect ratio of Camera always the same
        // Seta o aspecto da camera sempre para o mesmo
        Camera.main.aspect = 4f / 3f;

        if (Input.GetMouseButtonDown(0)) {
            var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider) {
                if(hit.collider == CloseGameButton) {
                    Application.Quit();
                }
            }
        }
	}
}
