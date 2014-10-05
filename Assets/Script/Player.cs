using UnityEngine;
using System.Collections;

public class Player : Tile {


	public override void init (Grid grid, int type, int x, int y) {
		base.init(grid, type, x, y);

		this.name = "Player_" + x + "_" + y;
		this.baseType = "player";

		print(name + " " + baseType);
	}


	public override void setType (int type) {
		this.type = type;
		image.sprite = (Sprite)grid.monsterTypes[type];
	}
}
