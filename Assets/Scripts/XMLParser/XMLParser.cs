using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class XMLParser {
	public static List<Card> ParseCards(string xml) {
		XmlReader reader = XmlReader.Create(new StringReader(xml));
		List<Card> cards = new List<Card>();
		Card current = null;

		while (reader.Read()) {
			if (reader.IsStartElement("card")) {
				if (current != null) {
					cards.Add(current);
				}
				current = new Card();
			}
			if (current != null) {
				if (reader.IsStartElement("id")) {
					current.ID = reader.ReadElementContentAsInt();
				}
				if (reader.IsStartElement("text")) {
					current.InsideText = reader.ReadElementContentAsString();
				}
				if (reader.IsStartElement("audio")) {
					string pathToAudio = reader.ReadElementContentAsString();
					current.audio = Resources.Load<AudioClip>(pathToAudio);
				}
			}
		}
		if (current != null) {
			cards.Add(current);
		}
		return cards;
	}

	public static List<Sound> ParseAudios(string xml) {
		XmlReader reader = XmlReader.Create(new StringReader(xml));
		List<Sound> sounds = new List<Sound>();
		Sound current = null;
		while (reader.Read()) {
			if (reader.IsStartElement("sound")) {
				if (current != null) {
					sounds.Add(current);
				}
				current = new Sound();
			}
			if (current != null) {
				if (reader.IsStartElement("id")) {
					current.ID = reader.ReadElementContentAsInt();
				}
				if (reader.IsStartElement("name")) {
					current.Name = reader.ReadElementContentAsString();
				}
				if (reader.IsStartElement("path")) {
					string pathToAudio = reader.ReadElementContentAsString();
					current.audio = Resources.Load<AudioClip>(pathToAudio);
				}
			}
		}
		if (current != null) {
			sounds.Add(current);
		}
		return sounds;
	}
}