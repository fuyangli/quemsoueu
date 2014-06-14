using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;

public class TextToAudio : EditorWindow {

	public static string textToSpeech;
	public static string language;
	public static string subfolder;
	public static bool _isResource;

	private static string _subfolder;

	// Open the Window
	[MenuItem("TTS/Download Text")]
	static void Init() {
		TextToAudio window = GetWindow<TextToAudio>();
		window.Show();
	}

	//Download the mp3
	void Download() {

		// URL to Google TTS
		string url = "http://translate.google.com/translate_tts?tl="+ language +"&q=" + textToSpeech.Replace(" ", "+").Trim();
		string fileName = textToSpeech + "_" + language;
		_subfolder = string.IsNullOrEmpty(subfolder) ? string.Empty : "/" + subfolder;
		string path = Application.dataPath + (_isResource ? "/Resources/TTS" : "/TTS") + _subfolder;
		string totalAudioPath = path + "/" + fileName + ".mp3";

		if(!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}

		using (var client = new WebClient()) {
			client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:7.0.1) Gecko/20100101 Firefox/7.0.1";
			client.DownloadFile(url, totalAudioPath);
		}

		AssetDatabase.Refresh();
	}

	//Draw the Window
	void OnGUI() {
		EditorGUI.DropShadowLabel(new Rect(0,0,position.width,20), "TTS");
		_isResource = EditorGUI.Toggle(new Rect(0, 0, position.width - 20, 20),"Resource?" ,_isResource);
		textToSpeech = EditorGUI.TextField(new Rect(0, 25, position.width - 20, 20), "Text:", textToSpeech);
		language = EditorGUI.TextField(new Rect(0, 50, position.width - 20, 20), "Lang: (en-US, pt-BR, etc)", language);
		subfolder = EditorGUI.TextField(new Rect(0, 75, position.width - 20, 20), "Subfolder: ", subfolder);

		if(GUI.Button( new Rect(0,100,position.width, 30), "Download!")) {
			Download();
		}
	}
	
}
