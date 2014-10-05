using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	protected Grid grid;
	public SpriteRenderer image;
	public TextMesh label;

	private float speed = 16.0f;

	public Vector2 finalPos;

	public bool moving = false;
	public bool exploding = false;
	public bool alive = true;

	public string baseType;
	public int type;
	public int x;
	public int y;
	public int spaces = 0;


	public virtual void init (Grid grid, int type, int x, int y) {
		// vars
		this.grid = grid;
		this.x = x;
		this.y = y;

		// gameObject
		this.baseType = "tile";
		this.name = "Tile_" + x + "_" + y;
		transform.parent = grid.transform;
		transform.localPosition = new Vector2(x, -y);

		// sprite
		image = GetComponent<SpriteRenderer>();
		image.sortingOrder = y + x;

		// label
		label = transform.Find("Label").GetComponent<TextMesh>();
		label.renderer.sortingLayerName = "Overlay";
		label.renderer.sortingOrder = -9;
		label.text = x + "," + y;
		label.gameObject.SetActive(false);

		// set type
		setType(type);

		// set props
		finalPos = transform.localPosition;
	}


	public virtual void setType (int type) {
		this.type = type;
		image.sprite = (Sprite)grid.tileTypes[type];
	}


	public void explode () {
		this.alive = false;
		this.exploding = true;
	}


	public void spawn(Vector2 pos) {
		transform.localPosition = pos;
		setType(Random.Range(0, grid.tileTypes.Length));
		transform.localScale = new Vector3(1, 1, 1);
	}


	void Update () {
		float step = speed * Time.deltaTime;

		// update position
		if (alive && moving) {
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, finalPos, step);
			//transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos, step * 2);

			if (transform.localPosition.x == finalPos.x && transform.localPosition.y == finalPos.y) {
				moving = false;
				Audio.play("audio/fx/bongo_acute6", 0.4f, Random.Range(2.0f, 3.0f), false);
			}
		}

		if (exploding) {
			transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(0, 0, 0), step);
			//transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), step * 2);

			if (transform.localScale == Vector3.zero) {
				exploding = false;
			}
		}

		// update label
		//label.text = "" + type;
		//label.text = "" + spaces;
		//label.text = "" + x + "," + y;
	}
}
