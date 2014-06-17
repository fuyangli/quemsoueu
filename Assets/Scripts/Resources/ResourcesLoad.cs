using System;
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

    public static Color ColorFrom255(int r, int g, int b) {
        float newR = Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 255, r));
        float newG = Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 255, g));
        float newB = Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 255, b));
        return new Color(newR, newG, newB);
    }
}
