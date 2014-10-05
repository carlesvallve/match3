using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour {

	// controls
	public TouchControls touchControls;
	public TouchLayer touchLayer;

	// editable from the unity editor
	public int rows = 9;
	public int cols = 7;
	public int tileSize = 42;

	// grid data
	public Object[] tileTypes;
	public Tile[,] tiles; // tiles.GetLength(0), tiles.GetLength(1)


	List<Tile> matches;


	private Vector2 swipeDirection;


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

		// initialize tile array
		tiles = new Tile[cols, rows];

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++){
				tiles[x, y] = createTile(Random.Range(0, tileTypes.Length), x, y);
			}
		}

		// resolve initial tile matches
		resolveInitialMatches();
	}


	private Tile createTile (int type, int x, int y) {
		GameObject obj = (GameObject)Instantiate(Resources.Load("Tiles/Tile"), Vector3.zero, Quaternion.identity);
		Tile tile = obj.GetComponent<Tile>();
		tile.init(this, type, x, y);

		return tile;
	}


	// *****************************************************
	// Swap Tiles
	// *****************************************************

	private IEnumerator swapTiles (Tile tile1, Tile tile2) {
		// swap tiles
		moveTile(tile1, tile2.transform.position);
		moveTile(tile2, tile1.transform.position);

		// wait for tiles to end moving
		yield return new WaitForSeconds(0.3f);

		// resolve tile matches
		resolveMatches();
	}


	private void moveTile(Tile tile, Vector2 pos) {
		setTileAtCoords(tile, pos);
		tile.finalPos = pos;
		tile.alive = true;
		tile.spaces = 0;
	}


	// *****************************************************
	// Tile Matches
	// *****************************************************

	private void resolveInitialMatches () {
		/*for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++){
				Tile tile = tiles[x, y];

				int c = 0;
				List<Tile> matches = getAllMatches();

				while(matches.Count > 0) {
					tile.setType(Random.Range(0, tileTypes.Length));
					matches = getAllMatches();
					c++;
					if (c == 5000) {
						print ("maximum iterations exceeded!");
						return;
					}
				}
			}
		}*/
	}


	private void resolveMatches () {
		List<Tile> matches = getAllMatches();
		print (matches.Count);

		if (matches.Count > 0) {
			destroyMatches(matches);
			spawnMatches(matches);
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

			if (tile.type == originTile.type) {
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

			if (tile.type == originTile.type) {
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
		// destroy matches
		for (int i = 0; i < matches.Count; i++) {
			Tile tile = matches[i];
			tile.alive = false;
			// set a new trandom type
			tile.setType(Random.Range(0, tileTypes.Length));
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
					tile.transform.position = new Vector2(xx, -tile.y);
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
					tile.transform.position = new Vector2(xx, -tile.y);
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
					tile.transform.position = new Vector2(tile.x, -yy);
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
					tile.transform.position = new Vector2(tile.x, -yy);
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
				int startX = Mathf.Max((int)tile.transform.position.x + 1, 0);
				for (var x = startX; x < cols; x++) {
					if (!tiles[x, tile.y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.x == -1) {
				int startX = Mathf.Min((int)tile.transform.position.x, cols - 1);
				for (var x = startX; x >= 0; x--) {
					if (!tiles[x, tile.y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.y == 1) {
				int startY = Mathf.Max((int)-tile.transform.position.y + 1, 0);
				for (var y = startY; y < rows; y++) {
					if (!tiles[tile.x, y].alive) tile.spaces++;
				}
			}

			if (swipeDirection.y == -1) {
				int startY = Mathf.Min((int)-tile.transform.position.y, rows - 1);
				for (var y = startY; y >= 0; y--) {
					if (!tiles[tile.x, y].alive) tile.spaces++;
				}
			}
		}
	}


	private IEnumerator moveSpawns (List<Tile> spawns) {
		// get empty spaces for each tile to spawn
		getSpawnSpaces(spawns);

		// move tiles
		for (int i = 0; i < spawns.Count; i++) {
			Tile tile = spawns[i];

			if (swipeDirection.x == 1) {
				float xx = tile.transform.position.x + tile.spaces;
				moveTile(tile, new Vector2(xx, -tile.y));
			}

			if (swipeDirection.x == -1) {
				float xx = tile.transform.position.x - tile.spaces;
				moveTile(tile, new Vector2(xx, -tile.y));
			}

			if (swipeDirection.y == 1) {
				float yy = -tile.transform.position.y + tile.spaces;
				moveTile(tile, new Vector2(tile.x, -yy));
			}

			if (swipeDirection.y == -1) {
				float yy = -tile.transform.position.y - tile.spaces;
				moveTile(tile, new Vector2(tile.x, -yy));
			}
		}

		yield return new WaitForSeconds(0.6f);

		// again, resolve tile matches
		resolveMatches();
	}


	// *****************************************************
	// Get Tiles
	// *****************************************************

	private Vector2 getCoords(Vector2 pos) {
		// get position relative to upper left corner of the screen
		pos.y = Mathf.Abs(pos.y - Screen.height);

		// get position relative to grid
		pos = new Vector2(
			pos.x - Screen.width / 2 + cols / 2 * tileSize + tileSize / 2,
			pos.y - Screen.height / 2 + rows / 2 * tileSize + tileSize / 2
		);

		//print ("Pos: " + pos);

		// get tile coords on position
		int x = (int)(pos.x / tileSize);
		int y = (int)(pos.y / tileSize);

		return new Vector2(x, y);
	}


	private Tile getTileAtPos (Vector2 pos) {
		return getTileAtCoords(getCoords(pos));
	}


	private Tile getTileAtCoords(Vector2 coords) {
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
		Camera.main.transform.position = new Vector3((cols - d.x) / 2, -(rows - d.y) / 2, -10);
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
		//getTileAtPos(e.activeTouch.endPos);
		//if (swipeDirection != Vector2.zero) resolveMatches();
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
