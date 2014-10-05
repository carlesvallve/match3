﻿using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	private Grid grid;
	public SpriteRenderer image;
	public TextMesh label;

	public Vector2 finalPos;

	private float speed = 16.0f;
	public bool moving = false;

	public int type;
	public int x;
	public int y;

	public bool alive = true;

	public int spaces = 0;


	public void init (Grid grid, int type, int x, int y) {
		// vars
		this.grid = grid;
		this.type = type;
		this.x = x;
		this.y = y;

		// gameObject
		name = "Tile_" + x + "_" + y;
		transform.parent = grid.transform;
		transform.localPosition = new Vector2(x, -y);

		// sprite
		image = GetComponent<SpriteRenderer>();
		image.sprite = (Sprite)grid.tileTypes[type]; // Resources.Load("Gems/0", typeof(Sprite)) as Sprite;
		image.sortingOrder = y + x;

		// label
		label = transform.Find("Label").GetComponent<TextMesh>();
		label.renderer.sortingLayerName = "Overlay";
		label.renderer.sortingOrder = -9;
		label.text = x + "," + y;

		// set props
		finalPos = transform.localPosition;
	}


	public void setType (int type) {
		this.type = type;
		image.sprite = (Sprite)grid.tileTypes[type];
	}


	void Update () {
		// update position
		if (alive && moving) {
			float step = speed * Time.deltaTime;
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, finalPos, step);
			//transform.position = Vector3.Lerp(transform.position, finalPos, step);

			if (transform.localPosition.x == finalPos.x && transform.localPosition.y == finalPos.y) {
				moving = false;
				Audio.play("audio/fx/bongo_acute6", 0.4f, Random.Range(2.0f, 3.0f), false);
			}
		}

		// update label
		label.text = "" + type;
		//label.text = "" + transform.position.x + "," + transform.position.y + "\n" + x + "," + y;
		//label.text = "" + spaces; //type + " (" + x + "," + y + ")";
	}
}
