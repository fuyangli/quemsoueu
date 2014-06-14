using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SoundName {
	Error1,
	Next1,
	Um,
	Dois,
	Três,
	Quatro,
	Cinco,
	Seis,
	Sete,
	Oito,
	Nove,
	Zero,
	Pontos,
	e,
	Rodada_do,
	Mais_um_ponto_para
}

public class ResourcesLoad {
	[SerializeField]
	private static List<AudioClip> _sounds;

	public static List<Card> Cards() {
		return XMLParser.ParseCards(Resources.Load<TextAsset>("XML/cards").text);
	}

	public static List<Sound> Sounds() {
		return XMLParser.ParseAudios(Resources.Load<TextAsset>("XML/sounds").text);
	}
}
