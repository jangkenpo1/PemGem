using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrandChurchMapGenerator : EditorWindow
{
    private const int TileSize = 32;
    private const string OutputFolder = "Assets/Art/Tilesets/Church";

    [MenuItem("Isekai Programmer/Generate Map Gereja Megah (32x32)")]
    public static void GenerateGrandChurch()
    {
        EnsureFolderExists(OutputFolder);

        // 1. Generate Tekstur & Tile 32x32 Pixel Art
        Dictionary<char, Tile> tiles = new Dictionary<char, Tile>
        {
            ['.'] = CreatePixelTile("tile_marble", DrawMarbleFloor()),
            ['='] = CreatePixelTile("tile_carpet", DrawRedGoldCarpet()),
            ['+'] = CreatePixelTile("tile_summon_circle", DrawSummonCircle()),
            ['#'] = CreatePixelTile("tile_gothic_wall", DrawGothicWall()),
            ['W'] = CreatePixelTile("tile_stained_glass", DrawStainedGlass()),
            ['O'] = CreatePixelTile("tile_pillar", DrawMarblePillar()),
            ['A'] = CreatePixelTile("tile_altar", DrawGoldenAltar()),
            ['B'] = CreatePixelTile("tile_pew", DrawWoodenPew()),
            ['*'] = CreatePixelTile("tile_brazier", DrawSacredBrazier()),
            ['D'] = CreatePixelTile("tile_exit_door", DrawExitCarpetDoor())
        };

        // 2. Denah Interior Gereja Megah (20 x 20 Tile)
        // Baris atas = Utara (Altar), Baris bawah = Selatan (Pintu Keluar ke Jalan Desa)
        string[] layout = new string[]
        {
            "#####WW##WW##WW#####",
            "#*......AAAA......*#",
            "#.......AAAA.......#",
            "#..O.....V......O..#",
            "#........==........#",
            "#.....*.++++.*.....#",
            "#.......+R.+.......#",
            "#..O....+.E+....O..#",
            "#........==........#",
            "#..BBBB..==..BBBB..#",
            "#........==........#",
            "#..O.....==.....O..#",
            "#..BBBB..==..BBBB..#",
            "#........==........#",
            "#..BBBB..==..BBBB..#",
            "#..O.....==.....O..#",
            "#..BBBB..==..BBBB..#",
            "#........==........#",
            "#.*......==......*.#",
            "########DDDD########"
        };

        // 3. Buat Hierarki Grid & Tilemap di Scene
        GameObject oldMap = GameObject.Find("Map_Gereja_Megah");
        if (oldMap != null) DestroyImmediate(oldMap);

        GameObject root = new GameObject("Map_Gereja_Megah");
        Grid grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        Tilemap groundMap = CreateTilemapLayer(root.transform, "Layer_Ground_Walkable", 0, false);
        Tilemap wallColliderMap = CreateTilemapLayer(root.transform, "Layer_Walls_Colliders", 1, true);

        GameObject entitiesHolder = new GameObject("Entities_And_Triggers");
        entitiesHolder.transform.SetParent(root.transform);

        int height = layout.Length;
        int width = layout[0].Length;

        for (int row = 0; row < height; row++)
        {
            int y = (height - 1) - row; // Membalik sumbu Y agar baris 0 berada di paling atas
            for (int x = 0; x < width; x++)
            {
                char symbol = layout[row][x];
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 worldCenter = new Vector3(x + 0.5f, y + 0.5f, 0f);

                // Semua sel mendapat lantai dasar marmer atau karpet terlebih dahulu
                if (symbol == '=' || symbol == 'D' || symbol == 'V')
                    groundMap.SetTile(cellPos, tiles['=']);
                else if (symbol == '+' || symbol == 'R' || symbol == 'E')
                    groundMap.SetTile(cellPos, tiles['+']);
                else
                    groundMap.SetTile(cellPos, tiles['.']);

                // Penempatan objek penghalang (Collider)
                if (symbol == '#' || symbol == 'W' || symbol == 'O' || symbol == 'A' || symbol == 'B' || symbol == '*')
                {
                    wallColliderMap.SetTile(cellPos, tiles[symbol]);
                }

                // Penempatan Marker Entitas & Trigger Pintu
                if (symbol == 'R')
                    CreateMarker(entitiesHolder.transform, "Spawn_Raka_IntroState", worldCenter, false);
                else if (symbol == 'V')
                    CreateMarker(entitiesHolder.transform, "NPC_Uskup_Valerian", worldCenter, true);
                else if (symbol == 'E')
                    CreateMarker(entitiesHolder.transform, "NPC_Suster_Elara", worldCenter, true);
                else if (symbol == 'D')
                {
                    groundMap.SetTile(cellPos, tiles['D']);
                    CreateMarker(entitiesHolder.transform, "Trigger_Exit_To_JalanDesa", worldCenter, true);
                }
            }
        }

        Selection.activeGameObject = root;
        Debug.Log("Map Gereja Megah (20x20, 32x32 PPU) berhasil dibuat!");
    }

    private static Tilemap CreateTilemapLayer(Transform parent, string name, int sortingOrder, bool addCollider)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortingOrder;

        if (addCollider)
        {
            go.AddComponent<TilemapCollider2D>();
        }
        return tm;
    }

    private static void CreateMarker(Transform parent, string name, Vector3 position, bool addTrigger)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent);
        marker.transform.position = position;

        if (addTrigger)
        {
            BoxCollider2D col = marker.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);
        }
    }

    private static Tile CreatePixelTile(string fileName, Color32[] pixels)
    {
        Texture2D tex = new Texture2D(TileSize, TileSize, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        tex.Apply();

        string pngPath = $"{OutputFolder}/{fileName}.png";
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(pngPath);

        TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = TileSize;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = fileName;

        string tileAssetPath = $"{OutputFolder}/{fileName}.asset";
        AssetDatabase.CreateAsset(tile, tileAssetPath);
        return tile;
    }

    private static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    // --- GENERATOR TEKSTUR PIXEL ART 32x32 ---

    private static Color32[] DrawMarbleFloor()
    {
        Color32 baseCol = new Color32(232, 236, 242, 255);
        Color32 groutCol = new Color32(185, 194, 208, 255);
        Color32 veinCol = new Color32(212, 219, 230, 255);
        Color32[] px = new Color32[TileSize * TileSize];

        for (int y = 0; y < TileSize; y++)
            for (int x = 0; x < TileSize; x++)
            {
                if (x == 0 || y == 0 || x == 16 || y == 16) px[y * TileSize + x] = groutCol;
                else if ((x + y) % 11 == 0) px[y * TileSize + x] = veinCol;
                else px[y * TileSize + x] = baseCol;
            }
        return px;
    }

    private static Color32[] DrawRedGoldCarpet()
    {
        Color32 red = new Color32(148, 27, 36, 255);
        Color32 darkRed = new Color32(115, 18, 26, 255);
        Color32 gold = new Color32(235, 186, 68, 255);
        Color32[] px = new Color32[TileSize * TileSize];

        for (int y = 0; y < TileSize; y++)
            for (int x = 0; x < TileSize; x++)
            {
                if (x <= 1 || x >= 30) px[y * TileSize + x] = darkRed;
                else if (x == 3 || x == 4 || x == 27 || x == 28) px[y * TileSize + x] = gold;
                else px[y * TileSize + x] = (x + y) % 4 == 0 ? darkRed : red;
            }
        return px;
    }

    private static Color32[] DrawSummonCircle()
    {
        Color32[] px = DrawMarbleFloor();
        Color32 cyan = new Color32(64, 212, 255, 255);
        Color32 gold = new Color32(255, 215, 90, 255);

        for (int y = 0; y < TileSize; y++)
            for (int x = 0; x < TileSize; x++)
            {
                if (x == 2 || x == 29 || y == 2 || y == 29) px[y * TileSize + x] = cyan;
                else if ((x == y || x + y == 31) && x > 5 && x < 26) px[y * TileSize + x] = gold;
            }
        return px;
    }

    private static Color32[] DrawGothicWall()
    {
        Color32 stone = new Color32(62, 68, 82, 255);
        Color32 trim = new Color32(42, 46, 56, 255);
        Color32 highlight = new Color32(92, 100, 118, 255);
        Color32[] px = new Color32[TileSize * TileSize];

        for (int y = 0; y < TileSize; y++)
            for (int x = 0; x < TileSize; x++)
            {
                if (y < 4 || y > 28) px[y * TileSize + x] = trim;
                else if (x % 8 == 0 || y % 8 == 0) px[y * TileSize + x] = trim;
                else if (y % 8 == 7) px[y * TileSize + x] = highlight;
                else px[y * TileSize + x] = stone;
            }
        return px;
    }

    private static Color32[] DrawStainedGlass()
    {
        Color32[] px = DrawGothicWall();
        Color32 blue = new Color32(58, 168, 245, 255);
        Color32 gold = new Color32(250, 200, 75, 255);
        Color32 ruby = new Color32(225, 70, 95, 255);

        for (int y = 5; y < 27; y++)
            for (int x = 6; x < 26; x++)
            {
                if (x == 6 || x == 25 || y == 5 || y == 26) px[y * TileSize + x] = gold;
                else if ((x + y) % 6 < 2) px[y * TileSize + x] = blue;
                else if ((x + y) % 6 < 4) px[y * TileSize + x] = gold;
                else px[y * TileSize + x] = ruby;
            }
        return px;
    }

    private static Color32[] DrawMarblePillar()
    {
        Color32[] px = DrawMarbleFloor();
        Color32 col = new Color32(215, 222, 232, 255);
        Color32 shade = new Color32(145, 155, 172, 255);
        Color32 gold = new Color32(230, 185, 70, 255);

        for (int y = 2; y < 30; y++)
            for (int x = 6; x < 26; x++)
            {
                if (y <= 5 || y >= 26) px[y * TileSize + x] = gold;
                else if (x < 9 || x > 22) px[y * TileSize + x] = shade;
                else px[y * TileSize + x] = col;
            }
        return px;
    }

    private static Color32[] DrawGoldenAltar()
    {
        Color32[] px = DrawMarbleFloor();
        Color32 gold = new Color32(242, 192, 65, 255);
        Color32 darkGold = new Color32(175, 125, 30, 255);
        Color32 cloth = new Color32(165, 30, 42, 255);

        for (int y = 3; y < 29; y++)
            for (int x = 2; x < 30; x++)
            {
                if (x <= 4 || x >= 27 || y <= 5 || y >= 26) px[y * TileSize + x] = darkGold;
                else if (y >= 10 && y <= 21) px[y * TileSize + x] = cloth;
                else px[y * TileSize + x] = gold;
            }
        return px;
    }

    private static Color32[] DrawWoodenPew()
    {
        Color32[] px = DrawMarbleFloor();
        Color32 wood = new Color32(128, 78, 42, 255);
        Color32 darkWood = new Color32(82, 48, 24, 255);

        for (int y = 6; y < 26; y++)
            for (int x = 1; x < 31; x++)
            {
                if (y <= 8 || y >= 23 || x <= 2 || x >= 29) px[y * TileSize + x] = darkWood;
                else px[y * TileSize + x] = wood;
            }
        return px;
    }

    private static Color32[] DrawSacredBrazier()
    {
        Color32[] px = DrawMarbleFloor();
        Color32 iron = new Color32(55, 60, 70, 255);
        Color32 fire = new Color32(255, 145, 35, 255);
        Color32 core = new Color32(255, 235, 120, 255);

        for (int y = 6; y < 26; y++)
            for (int x = 8; x < 24; x++)
            {
                if (y < 15) px[y * TileSize + x] = iron;
                else if (x >= 12 && x <= 19 && y <= 21) px[y * TileSize + x] = core;
                else px[y * TileSize + x] = fire;
            }
        return px;
    }

    private static Color32[] DrawExitCarpetDoor()
    {
        Color32[] px = DrawRedGoldCarpet();
        Color32 glow = new Color32(255, 245, 180, 255);
        for (int y = 0; y < 4; y++)
            for (int x = 4; x < 28; x++)
                px[y * TileSize + x] = glow;
        return px;
    }
}