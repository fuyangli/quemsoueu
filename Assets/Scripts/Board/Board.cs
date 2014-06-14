using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
	private List<Cell> _cells = new List<Cell>();

	public Cell[] Cells {
		get { return _cells.ToArray(); }
		set {
			_cells = new List<Cell>(value);
		}
	}
}
