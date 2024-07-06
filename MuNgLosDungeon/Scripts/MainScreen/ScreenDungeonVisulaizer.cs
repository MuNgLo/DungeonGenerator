using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Munglo.DungeonGenerator
{
    [Tool]
    internal partial class ScreenDungeonVisulaizer : Node3D
    {
        private MainScreen screen;
        private MapData map;
        private GenerationSettingsResource settings;
        private AddonSettings MasterConfig;
        private Dictionary<PIECEKEYS, Dictionary<int, Resource>> cacheKeyedPieces;
        private Node3D mapContainer;
        private Node3D propContainer;
        private Node3D tileContainer;
        private BiomeResource biome;
        private Node3D LevelDebug => GetNode<Node3D>("GeneratedDebug");

        public override void _EnterTree()
        {
            screen = GetParent().GetParent().GetParent() as MainScreen;
            MasterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettings;
        }

        public void BuildDungeon(GenerationSettingsResource settings, BiomeResource biome)
        {
            screen.ScreenNotify($"Generating:" + string.Format("{0:0}", 0) + "%");
            this.settings = settings;
            BuildData(biome, () => { ShowMap(null, null); }, settings.roomStart, settings.roomDefault);
        }
        private async void BuildData(BiomeResource biome, Action callback, RoomResource startRoom, RoomResource standardRoom)
        {
            cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
            this.biome = biome;
            if (settings is null)
            {
                GD.PrintErr($"DungeonGenerator::BuildDungeon() BuildDungeonFailed! settings is NULL[{settings is null}] biome is NUll[{biome is null}]");
                return;
            }
            map = new MapData(settings, startRoom, standardRoom);
            await map.GenerateMap(callback);
        }
        public async void ShowMap(object sender, EventArgs e)
        {
            screen.ScreenNotify($"Generating:" + string.Format("{0:0}", 10) + "%");
            cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
            mapContainer = FindChild("Generated") as Node3D;
            await VisualizeSections();
            //if (autoBuildNavMesh) { BuildNavMesh(); }
        }

        internal void ClearLevel()
        {
            Node3D generated = GetNode<Node3D>("Generated");
            if (generated == null)
            {
                GD.Print($"ScreenDungeonVisulaizer::ClearLevel()  Generated node missing!");
                return;
            }
            foreach (Node child in generated.GetChildren())
            {
                if (child is Node3D)
                {
                    child.QueueFree();
                }
            }
            //mapGen.ClearNavMesh();
        }
        internal void ClearLevelDebug()
        {
            Node3D generated = GetNode<Node3D>("GeneratedDebug");
            if (generated == null)
            {
                GD.Print($"ScreenDungeonVisulaizer::ClearLevel()  GeneratedDebug node missing!");
                return;
            }
            foreach (Node child in generated.GetChildren())
            {
                if (child is Node3D)
                {
                    child.QueueFree();
                }
            }
        }

        private async Task VisualizePath(PathSection path)
        {
            if (path == null) { return; }
            Node3D pathNode = new Node3D();
            propContainer = new Node3D();
            tileContainer = new Node3D();
            pathNode.Name = "Path" + string.Format("{0:000}", path.SectionIndex) + "PID";
            propContainer.Name = $"Props[{path.PropCount}]";
            tileContainer.Name = $"Tiles[{path.TileCount}]";

            GetFloorContainer(path.Coord.y).AddChild(pathNode, true);

            pathNode.AddChild(propContainer, true);
            pathNode.AddChild(tileContainer, true);
            // Tiles
            int index = 0;
            foreach (MapPiece rp in path.Pieces)
            {
                MapPiece piece = map.GetPiece(rp.Coord);
                if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, true))
                {
                    visualNode.Name = $"Path{string.Format("0:000", path.SectionIndex)}-{index}";
                    tileContainer.AddChild(visualNode, true);
                    visualNode.Position = Dungeon.GlobalPosition(piece);
                    visualNode.Show();
                    AddDebugVisuals(piece);
                    index++;
                    if (index % 250 == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                    }
                }
            }
        }

        private Node3D GetFloorContainer(int y)
        {
            if(mapContainer.GetChildCount() < y + 1)
            {
                Node3D node = new Node3D();
                node.Name = string.Format("{0:000}" + "Floor", y);
                mapContainer.AddChild(node, true);
            }
            return mapContainer.GetChild(y) as Node3D;
        }

        private void AddDebugVisuals(MapPiece piece)
        {
            // generate debug
            if (settings.debugPass)
            {
                foreach (KeyData keyData in piece.Debug)
                {
                    if (GetByKey(keyData, biome, out Node3D debugProp, false)) { LevelDebug.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
                if (piece.isBridge)
                {
                    if (GetByKey(
                        new KeyData() { key = PIECEKEYS.DEBUGBRIDGE, dir = piece.Orientation }
                        , biome, out Node3D debugProp, false)) { LevelDebug.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
                if (piece.hasStairs)
                {
                    if (GetByKey(
                    new KeyData() { key = PIECEKEYS.DEBUGSTAIR, dir = piece.Orientation }
                    , biome, out Node3D debugProp, false)) { LevelDebug.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
            }
        }

        private async Task VisualizeSections()
        {
            foreach (ISection section in map.Sections)
            {
                if (section.Coord.y < MasterConfig.visibleFloorStart || section.Coord.y > MasterConfig.visibleFloorEnd - 1) { continue; }
                screen.ScreenNotify($"Generating:" + string.Format("{0:0}", 100) + "%");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");

                if (section is PathSection)
                {
                    await VisualizePath(section as PathSection);
                    continue;
                }
                if (section is RoomSection)
                {
                    await VisualizeRoom(section as RoomSection);
                    continue;
                }

                GD.Print($"Section is not recogniced!");
            }
        }
        
        private async Task VisualizeRoom(ISection room)
        {
            if (room == null) { return; }
            if (room is not RoomSection) { return; }
            int count = 0;
            Node3D roomNode = new Node3D();
            propContainer = new Node3D();
            tileContainer = new Node3D();
            roomNode.Name = "Room" + string.Format("{0:000}" + "SID", room.SectionIndex);
            propContainer.Name = $"Props[{room.PropCount}]";
            tileContainer.Name = $"Tiles[{room.Pieces.Count}]";
            GetFloorContainer(room.Coord.y).AddChild(roomNode, true);
            roomNode.AddChild(propContainer, true);
            roomNode.AddChild(tileContainer, true);
            // Room Tiles
            foreach (MapPiece rp in room.Pieces)
            {
                MapPiece piece = map.GetPiece(rp.Coord);
                if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, true))
                {
                    tileContainer.AddChild(visualNode, true);
                    visualNode.Position = Dungeon.GlobalPosition(piece);

                    visualNode.Show();
                    AddDebugVisuals(piece);
                    count++;
                    if (count % 250 == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                    }
                }
            }
            // Room Prop
            foreach (MapCoordinate c in room.PropGrids.Keys)
            {
                foreach (Vector3I prop in room.PropGrids[c].Keys)
                {
                    SpawnRoomProp(biome, c, room.PropGrids[c][prop], true);
                    count++;
                    if (count % 400 == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                    }
                }
            }
        }
        internal void SpawnRoomProp(BiomeResource biome, MapCoordinate coord, RoomProp propData, bool makeCollider = true)
        {
            //GD.Print($"DungeonGenerator::SpawnRoomProp()");
            if (settings.showProps)
            {
                if (GetByKey(new KeyData() { key = propData.key, dir = propData.dir, variantID = propData.variantID }, biome, out Node3D prop, makeCollider))
                {
                    propContainer.AddChild(prop, true);
                    prop.GlobalPosition = Dungeon.GlobalRoomPropPosition(coord, propData.Offset);


                };
            }
        }
        /// <summary>
        /// Decodes and instantiates the nodes needed for the map piece data
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="piece"></param>
        /// <param name="makeCollider"></param>
        internal bool BuildVisualNode(BiomeResource biome, MapPiece piece, out Node3D visualNode, Node3D propParent, bool makeCollider = true)
        {
            visualNode = new Node3D();
            visualNode.Name = piece.CoordString;

            // generate floors
            if (settings.showFloors)
            {
                if (piece.keyFloor.key != PIECEKEYS.NONE && piece.keyFloor.key != PIECEKEYS.OCCUPIED &&
                    GetByKey(piece.keyFloor, biome, out Node3D floor, makeCollider)) { visualNode.AddChild(floor, true); };
            }
            // generate walls
            if (settings.showWalls)
            {
                if (piece.Walls.HasFlag(WALLS.N))
                {
                    if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider))
                    {
                        visualNode.AddChild(wall, true);
                    };
                }
                if (piece.Walls.HasFlag(WALLS.E))
                {
                    if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider))
                    {
                        visualNode.AddChild(wall, true);
                    };
                }
                if (piece.Walls.HasFlag(WALLS.S))
                {
                    if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider))
                    {
                        visualNode.AddChild(wall, true);
                    };
                }
                if (piece.Walls.HasFlag(WALLS.W))
                {
                    if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider))
                    {
                        visualNode.AddChild(wall, true);
                    };
                }



                // Not flagged as wall but check for rounded corner keys
                if (piece.WallKeyNorth.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall, true); };
                }
                if (piece.WallKeyEast.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall, true); };
                }
                if (piece.WallKeySouth.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall, true); };
                }
                if (piece.WallKeyWest.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall, true); };
                }
            }
            // generate ceiling
            if (settings.showCeilings)
            {
                if (piece.keyCeiling.key != PIECEKEYS.NONE && GetByKey(piece.keyCeiling, biome, out Node3D ceiling, makeCollider)) { visualNode.AddChild(ceiling, true); };
            }

            // generate props
            if (settings.showProps)
            {
                foreach (KeyData keyData in piece.Props)
                {
                    // Skip arches if no show
                    if (!settings.showArches)
                    {
                        if (keyData.key == PIECEKEYS.AS || keyData.key == PIECEKEYS.ASIC) { continue; }
                    }

                    if (GetByKey(keyData, biome, out Node3D prop, makeCollider))
                    {
                        propParent.AddChild(prop, true);
                        prop.Position = Dungeon.GlobalPosition(piece);
                        prop.Show();
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// returns a Node with the correct rotation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="biome"></param>
        /// <param name="obj"></param>
        /// <param name="makeCollider"></param>
        /// <returns></returns>
        internal bool GetByKey(KeyData data, BiomeResource biome, out Node3D obj, bool makeCollider)
        {
            if (data.key == PIECEKEYS.NONE || data.key == PIECEKEYS.OCCUPIED) { obj = null; return false; }
            Resource res = ResolveAndCache(data, biome);
            if (res == null) { obj = null; return false; }


            // Split depending if Mesh or Prefab
            if (res is Mesh)
            {
                obj = new MeshInstance3D() { Mesh = res as Mesh };
                if (makeCollider) { (obj as MeshInstance3D).CreateConvexCollision(); }
            }
            else
            {
                obj = (res as PackedScene).Instantiate() as Node3D;
                if (obj == null)
                {
                    GD.Print($"DungeonGenerator::GetByKey() Key was {data.key} resolving packed scene resulted in NULL!");
                    return false;
                }
            }
            obj.Name = data.key.ToString() + "-" + data.dir.ToString();
            if (data.dir != MAPDIRECTION.ANY) { obj.RotationDegrees = Dungeon.ResolveRotation(data.dir); } else { obj.RotationDegrees = Vector3.Up * 45.0f; }
            return true;
        }
        private Resource ResolveAndCache(KeyData data, BiomeResource biome)
        {
            if (cacheKeyedPieces == null) { cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>(); }

            if (!cacheKeyedPieces.ContainsKey(data.key)) { cacheKeyedPieces[data.key] = new Dictionary<int, Resource>(); }

            if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
            {
                if (biome.GetResource(data.key, data.variantID, out Resource result))
                {
                    cacheKeyedPieces[data.key][data.variantID] = result;
                }

                if (biome.debug.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.debug.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }
                else if (biome.walls.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.walls.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }
                else if (biome.floors.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.floors.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }
                else if (biome.ceilings.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.ceilings.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }
                else if (biome.props.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.props.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }

            }
            if (!cacheKeyedPieces.ContainsKey(data.key))
            {
                //Log(this, "ResolveAndCache", $"Key [{data.key}] was not found!");
                return null;
            }
            if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
            {
                if (!cacheKeyedPieces[data.key].ContainsKey(0))
                {
                    //Log(this, "ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! And Default fallback failed!");
                    return null;
                }
                //Log(this, "ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! Default used as fallback.");
                return cacheKeyedPieces[data.key][0];
            }
            return cacheKeyedPieces[data.key][data.variantID];
        }

        internal void SetDebugLayer(bool state)
        {
            Node3D generated = GetNode<Node3D>("GeneratedDebug");
            if (generated == null)
            {
                GD.Print($"ScreenDungeonVisulaizer::ClearLevel()  GeneratedDebug node missing!");
                return;
            }
            if (!state)
            {
                generated.Hide();
                screen.ScreenNotify($"Debug OFF");
            }
            else
            {
                generated.Show();
                screen.ScreenNotify($"Debug ON");
            }
        }
    }// eof class
}
