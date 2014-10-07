using UnityEngine;
using System.Collections;

public class Monster : Tile {


	public override void init (Grid grid, int type, int x, int y, Vector2 pos) {
		base.init(grid, type, x, y, pos);

		baseType = "Monster";
		gameObject.name = "Monster_" + x + "_" + y;
	}


	public override void setType (int type) {
		this.type = type;
		image.sprite = (Sprite)grid.monsterTypes[type];
	}
}
