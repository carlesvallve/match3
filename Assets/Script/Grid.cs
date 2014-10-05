using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour {

	// controls
	private TouchControls touchControls;
	private TouchLayer touchLayer;
	private Vector2 swipeDirection;

	// editable from the unity editor
	private int rows = 9;
	private int cols = 7;
	private int tileSize = 42;

	// tile types
	public Object[] tileTypes;
	public Object[] monsterTypes;

	// tile data
	private Tile[,] tiles; // tiles.GetLength(0), tiles.GetLength(1)
	private List<Tile> matches;

	Player player;


	// *****************************************************
	// Grid
	// *****************************************************

	void Start () {
		// initialize touch controls
		initTouchControls();

		// initialize camera
		initCamera();

		// load sprites from the given resources folder to this object array
		tileTypes = Resources.LoadAll("Tiles/Textures/Random", typeof(Sprite));
		monsterTypes = Resources.LoadAll("Tiles/Textures/Monsters", typeof(Sprite));

		// initialize tile array
		tiles = new Tile[cols, rows];

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				tiles[x, y] = createTile(Random.Range(0, tileTypes.Length), x, y);
			}
		}

		// insert player
		player = createPlayer(Random.Range(0, monsterTypes.Length), 0, 0);
	}


	private Tile createTile (int type, int x, int y) {
		GameObject obj = (GameObject)Instantiate(Resources.Load("Tiles/Prefabs/Tile"), Vector3.zero, Quaternion.identity);
		Tile tile = obj.GetComponent<Tile>();
		tile.init(this, type, x, y);

		return tile;
	}


	private Player createPlayer (int type, int x, int y) {
		Destroy(tiles[x, y].gameObject);

		GameObject obj = (GameObject)Instantiate(Resources.Load("Tiles/Prefabs/Player"), Vector3.zero, Quaternion.identity);
		Player player = obj.GetComponent<Player>();
		player.init(this, type, x, y);



		setTileAtCoords(player, new Vector2(player.x, player.y));

		return player;
	}


	// *****************************************************
	// Swap Tiles
	// *****************************************************

	private IEnumerator swapTiles (Tile tile1, Tile tile2) {
		// escape if we dont have 2 tiles to swap
		if (!tile1 || !tile2) yield break;

		// play swapping sound
		Audio.play("audio/fx2/Swoosh3", 0.3f, Random.Range(2.0f, 2.5f), false);

		// swap tiles
		moveTile(tile1, tile2.transform.localPosition);
		moveTile(tile2, tile1.transform.localPosition);

		// wait for tiles to end moving
		yield return new WaitForSeconds(0.1f);

		// resolve tile matches
		StartCoroutine(resolveMatches());
	}


	private void moveTile(Tile tile, Vector2 pos) {
		setTileAtCoords(tile, pos);
		tile.finalPos = pos;
		tile.alive = true;
		tile.moving = true;
		tile.spaces = 0;
	}


	// *****************************************************
	// Tile Matches
	// *****************************************************

	private IEnumerator resolveMatches () {
		// disable user interaction
		touchControls.enabled = false;

		// get all matches in the board
		List<Tile> matches = getAllMatches();

		if (matches.Count > 0) {
			// if we found matches, resolve them
			destroyMatches(matches);

			yield return new WaitForSeconds(0.25f);

			spawnMatches(matches);
		} else {
			// if no more matches, re-enable user interaction
			touchControls.enabled = true;
		}
	}


	private List<Tile> getAllMatches () {
		matches = new List<Tile>();

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				Tile tile = getTileAtCoords(new Vector2(x, y));
				tile.image.color = Color.white;
			}
		}

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				Tile tile = getTileAtCoords(new Vector2(x, y));

				// resolve tile horizontal matches
				List<Tile> tileMatchesH = this.getTileHorizontalMatches(tile);
				if (tileMatchesH.Count >= 3) {
					for (int i = 0; i < tileMatchesH.Count; i++) {
						matches.Add(tileMatchesH[i]);
					}
				}

				// resolve tile vertical matches
				List<Tile> tileMatchesV = this.getTileVerticalMatches(tile);
				if (tileMatchesV.Count >= 3) {
					for (int i = 0; i < tileMatchesV.Count; i++) {
						matches.Add(tileMatchesV[i]);
					}
				}
			}
		}

		matches = matches.Distinct().ToList();

		return matches;
	}


	private List<Tile> getTileHorizontalMatches (Tile originTile) {
		// get tile horizontal matches
		List<Tile> tileMatches = new List<Tile>();

		for (int x = originTile.x; x < cols ; x++) {
			Tile tile = getTileAtCoords(new Vector2(x, originTile.y));

			if (tile.baseType == originTile.baseType && tile.type == originTile.type) {
				tileMatches.Add(tile);
			} else {
				break;
			}
		}

		// return tile matches
		return tileMatches;
	}


	private List<Tile> getTileVerticalMatches (Tile originTile) {
		// get tile vertical matches
		List<Tile> tileMatches = new List<Tile>();

		for (int y = originTile.y; y < rows; y++) {
			Tile tile = getTileAtCoords(new Vector2(originTile.x, y));

			if (tile.baseType == originTile.baseType && tile.type == originTile.type) {
				tileMatches.Add(tile);
			} else {
				break;
			}
		}

		// return tile matches
		return tileMatches;
	}


	// *****************************************************
	// Destroy Matches
	// *****************************************************

	private void destroyMatches(List<Tile> matches) {

		//Audio.play("audio/fx/hit-stub", 0.8f, Random.Range(0.5f, 1.5f), false);
		Audio.play("audio/fx2/Punch", 0.8f, Random.Range(0.5f, 1.5f), false);

		// destroy matches
		for (int i = 0; i < matches.Count; i++) {
			Tile tile = matches[i];
			tile.explode();
		}
	}


	/*private void paintTiles(List<Tile> tilesToPaint, Color color) {
		// destroy matches
		for (int i = 0; i < tilesToPaint.Count; i++) {
			Tile tile = tilesToPaint[i];
			tile.image.color = tile.alive ? Color.blue : Color.red; //color;
		}
	}*/


	// *****************************************************
	// Spawn Tiles
	// *****************************************************

	private void spawnMatches (List<Tile> matches) {
		// spawn matches in the given swipe direction
		List<Tile> spawns = new List<Tile>();

		if (swipeDirection.x == -1) spawns = spawnTilesRight();
		if (swipeDirection.x == 1) spawns = spawnTilesLeft();
		if (swipeDirection.y == -1) spawns = spawnTilesBottom();
		if (swipeDirection.y == 1) spawns = spawnTilesTop();

		// move spawned tiles to new positions in board
		StartCoroutine(moveSpawns(spawns));
	}


	private List<Tile> spawnTilesRight () {
		List<Tile> spawns = new List<Tile>();

		for (int y = 0; y < rows; y++) {
			int c = 0;
			for (int x = 0; x < cols; x++) {
				Tile tile = getTileAtCoords(new Vector2(x, y));

				if (!tile.alive) {
					int xx = cols + c;
					tile.spawn(new Vector2(xx, -tile.y));
					c++;
					spawns.Add(tile);
				} else {
					if (c > 0) spawns.Add(tile);
				}
			}
		}

		return spawns;
	}


	private List<Tile> spawnTilesLeft () {
		List<Tile> spawns = new List<Tile>();

		for (int y = 0; y < rows; y++) {
			int c = 0;
			for (int x = cols - 1; x >=0; x--) {
				Tile tile = getTileAtCoords(new Vector2(x, y));

				if (!tile.alive) {
					int xx = - c - 1;
					tile.spawn(new Vector2(xx, -tile.y));
					//tile.transform.localPosition = new Vector2(xx, -tile.y);
					//tile.setType(Random.Range(0, tileTypes.Length));
					c++;
					spawns.Add(tile);
				} else {
					if (c > 0) spawns.Add(tile);
				}
			}
		}

		return spawns;
	}


	private List<Tile> spawnTilesBottom () {
		List<Tile> spawns = new List<Tile>();


		for (int x = 0; x < cols; x++) {
			int c = 0;
			for (int y = 0; y < rows; y++) {
				Tile tile = getTileAtCoords(new Vector2(x, y));

				if (!tile.alive) {
					int yy = rows + c;
					tile.spawn(new Vector2(tile.x, -yy));
					//tile.transform.localPosition = new Vector2(tile.x, -yy);
					//tile.setType(Random.Range(0, tileTypes.Length));
					c++;
					spawns.Add(tile);
				} else {
					if (c > 0) spawns.Add(tile);
				}
			}
		}

		return spawns;
	}



	private List<Tile> spawnTilesTop () {
		List<Tile> spawns = new List<Tile>();

		for (int x = 0; x < cols; x++) {
			int c = 0;
			for (int y = rows - 1; y >=0; y--) {
				Tile tile = getTileAtCoords(new Vector2(x, y));

				if (!tile.alive) {
					int yy = - c - 1;
					tile.spawn(new Vector2(tile.x, -yy));
					//tile.transform.localPosition = new Vector2(tile.x, -yy);
					//tile.setType(Random.Range(0, tileTypes.Length));
					c++;
					spawns.Add(tile);
				} else {
					if (c > 0) spawns.Add(tile);
				}
			}
		}

		return spawns;
	}


	// *****************************************************
	// Move Spawned Tiles
	// *****************************************************

	private void getSpawnSpaces (List<Tile> spawns) {
		for (int i = 0; i < spawns.Count; i++) {
			Tile tile = spawns[i];
			tile.spaces = 0;

			if (swipeDirection.x == 1) {
				int startX = Mathf.Max((int)tile.transform.localPosition.x + 1, 0);
				for (var x = startX; x < cols; x++) {
					if (!tiles[x, tile.y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.x == -1) {
				int startX = Mathf.Min((int)tile.transform.localPosition.x, cols - 1);
				for (var x = startX; x >= 0; x--) {
					if (!tiles[x, tile.y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.y == 1) {
				int startY = Mathf.Max((int)-tile.transform.localPosition.y + 1, 0);
				for (var y = startY; y < rows; y++) {
					if (!tiles[tile.x, y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.y == -1) {
				int startY = Mathf.Min((int)-tile.transform.localPosition.y, rows - 1);
				for (var y = startY; y >= 0; y--) {
					if (!tiles[tile.x, y].alive) tile.spaces++;
				}
			}
		}
	}


	private IEnumerator moveSpawns (List<Tile> spawns) {
		// get empty spaces for each tile to spawn
		getSpawnSpaces(spawns);

		// play moving sound
		//Audio.play("audio/fx/magic-water", 0.2f, Random.Range(1.5f, 3.0f), false);

		// move tiles
		for (int i = 0; i < spawns.Count; i++) {
			Tile tile = spawns[i];

			if (swipeDirection.x == 1) {
				float xx = tile.transform.localPosition.x + tile.spaces;
				moveTile(tile, new Vector2(xx, -tile.y));
			}

			if (swipeDirection.x == -1) {
				float xx = tile.transform.localPosition.x - tile.spaces;
				moveTile(tile, new Vector2(xx, -tile.y));
			}

			if (swipeDirection.y == 1) {
				float yy = -tile.transform.localPosition.y + tile.spaces;
				moveTile(tile, new Vector2(tile.x, -yy));
			}

			if (swipeDirection.y == -1) {
				float yy = -tile.transform.localPosition.y - tile.spaces;
				moveTile(tile, new Vector2(tile.x, -yy));
			}
		}

		// TODO: figure out the exact moment when the last tile arrives to position
		yield return new WaitForSeconds(0.35f);

		// again, resolve tile matches
		StartCoroutine(resolveMatches());
	}


	// *****************************************************
	// Grid and Tile operations
	// *****************************************************

	private Vector2 getPixelPosInGrid (Vector2 pos) {
		// get position relative to upper left corner of the screen
		pos.y = Mathf.Abs(pos.y - Screen.height);

		// get position relative to grid
		pos = new Vector2(
			pos.x - Screen.width / 2 + cols / 2 * tileSize + tileSize / 2 + transform.localPosition.x * tileSize,
			pos.y - Screen.height / 2 + rows / 2 * tileSize + tileSize / 2 + transform.localPosition.y * tileSize
		);

		return pos;
	}


	private Vector2 getCoords(Vector2 pos) {
		// get given position in grid
		pos = getPixelPosInGrid(pos);

		// get tile coords on position
		int x = (int)(pos.x / tileSize);
		int y = (int)(pos.y / tileSize);

		return new Vector2(x, y);
	}


	private Tile getTileAtPos (Vector2 pos) {
		return getTileAtCoords(getCoords(pos));
	}


	private Tile getTileAtCoords(Vector2 coords) {
		if (coords.x < 0 || coords.y < 0 || coords.x > cols - 1 || coords.y > rows - 1) {
			print ("coords outside grid!");
			return null;
		}

		return tiles[(int)coords.x, (int)coords.y];
	}


	private void setTileAtCoords(Tile tile, Vector2 coords) {
		tile.x = (int)coords.x;
		tile.y = (int)-coords.y;
		tile.name = "Tile_" + tile.x + "_" + tile.y;
		tiles[tile.x, tile.y] = tile;
	}


	// *****************************************************
	// Camera
	// *****************************************************

	private void initCamera () {
		Vector2 d = new Vector2(cols / 2  == (int) cols / 2 ? 1 : 0.5f, rows / 2  == (int) rows / 2 ? 1 : 0.5f);
		Camera.main.transform.localPosition = new Vector3((cols - d.x) / 2, -(rows - d.y) / 2, -10);
	}


	// *****************************************************
	// Touch Controls
	// *****************************************************

	private void initTouchControls () {
		touchControls = GameObject.Find("TouchControls").GetComponent<TouchControls>();
		touchLayer = touchControls.getLayer("grid");

		touchLayer.onPress += onTouchPress;
		touchLayer.onRelease += onTouchRelease;
		touchLayer.onMove += onTouchMove;
		touchLayer.onSwipe += onTouchSwipe;
	}


	public void onTouchPress (TouchEvent e) {
		//print ("press " + e.activeTouch.endPos);
		//print("PosInGrid; " + getPixelPosInGrid(e.activeTouch.endPos));
	}


	public void onTouchRelease (TouchEvent e) {
		//print ("release " + e.activeTouch.endPos);
	}


	public void onTouchMove (TouchEvent e) {
		//print ("move " + e.activeTouch.endPos);
	}


	public void onTouchSwipe (TouchEvent e) {
		//print ("swipe "+ e.activeTouch.startPos + " " + e.activeTouch.deltaPos + " " + e.activeTouch.relativeDeltaPos);

		// get final delta
		Vector2 delta = e.activeTouch.deltaPos.normalized; //new Vector2(
		if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
			delta.y = 0;
		} else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x)) {
			delta.x = 0;
		}
		delta = new Vector2(Mathf.Round(delta.x), -Mathf.Round(delta.y));

		// get tile 1
		Vector2 coords1 = getCoords(e.activeTouch.startPos);
		Tile tile1 = getTileAtCoords(coords1);

		// get tile 2
		Vector2 coords2 = new Vector2(coords1.x + delta.x, coords1.y + delta.y);
		Tile tile2 = getTileAtCoords(coords2);

		// swipe tiles
		swipeDirection = delta;
		StartCoroutine(swapTiles(tile1, tile2));
	}
}
