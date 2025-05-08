using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Godot.TextServer;
using static System.Formats.Asn1.AsnWriter;
using Munglo.DungeonGenerator.Sections;

namespace Munglo.DungeonGenerator
{
    public partial class DungeonGenerator : Node3D
    {
        #region TODO move this to settings
        [Export] private bool autoBuildNavMesh = false;
        #endregion

        private MapData map;
        private GenerationSettingsResource genSettings;
        public GenerationSettingsResource Config => genSettings;
        private Dictionary<PIECEKEYS, Dictionary<int, Resource>> cacheKeyedPieces;
        internal Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> Pieces => map.Pieces;
        internal List<MapPiece> PendingPieces => map.GetPieces(MAPPIECESTATE.PENDING);
        internal List<MapPiece> LockedPieces => map.GetPieces(MAPPIECESTATE.LOCKED);
        private Node3D LevelDebug => GetNode<Node3D>("GeneratedDebug");
        internal MapData Map => map;

        private Node3D mapContainer;
        private Node3D propContainer;
        private Node3D tileContainer;
        private Node3D unsortedContainer;
        private BiomeResource biome;

        public void BuildDungeon(GenerationSettingsResource settings,
         BiomeResource biome, SectionResource startRoom, SectionResource standardRoom, 
         bool floors, bool walls, bool ceilings, bool doPathing)
        {
            genSettings = settings;
            BuildData(biome, () => { ShowMap(floors, walls, ceilings); }, startRoom, standardRoom, doPathing);
        }
        /// <summary>
        /// Generates a new set of MapData. Calls the CallBack when done.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="callback"></param>
        /// <param name="startRoom"></param>
        /// <param name="standardRoom"></param>
        public void BuildSeededDungeon(int[] seed, GenerationSettingsResource settings, BiomeResource biome, Action callback, SectionResource startRoom, SectionResource standardRoom, bool doPathing)
        {
            genSettings = settings;
            Config.seed1 = seed[0];
            Config.seed2 = seed[1];
            Config.seed3 = seed[2];
            Config.seed4 = seed[3];
            genSettings = settings;
            BuildData(biome, () => { callback.Invoke(); }, startRoom, standardRoom, doPathing);
        }

        private async void BuildData(BiomeResource biome, Action callback, SectionResource startRoom, SectionResource standardRoom, bool doPathing)
        {
            cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
            this.biome = biome;
            if (genSettings is null)
            {
                GD.PrintErr($"DungeonGenerator::BuildDungeon() BuildDungeonFailed! settings is NULL[{genSettings is null}] biome is NUll[{biome is null}]");
                return;
            }
            map = new MapData(Config, startRoom, standardRoom);
            await map.GenerateMap(callback, doPathing);
        }
        public async void ShowMap(bool floors, bool walls, bool ceilings)
        {
            cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>();
            mapContainer = FindChild("Generated") as Node3D;
            unsortedContainer = new Node3D();
            unsortedContainer.Name = "UnSorted";
            mapContainer.AddChild(unsortedContainer, true);
            if (Engine.IsEditorHint())
            {
                MoveToEditedScene(unsortedContainer);
            }
            await VisualizeRooms(floors, walls, ceilings);
            await VisualizePaths(floors, walls, ceilings);
            if(autoBuildNavMesh) { BuildNavMesh(); }
        }
   
        private async Task VisualizePaths(bool floors, bool walls, bool ceilings)
        {
            foreach (ISection path in map.Sections)
            {
                await VisualizePath(path as PathSection, floors, walls, ceilings);
            }
        }

        private async Task VisualizePath(PathSection path, bool floors, bool walls, bool ceilings)
        {
            if (path == null) { return; }
            int count = 0;

            Node3D pathNode = new Node3D();
            propContainer = new Node3D();
            tileContainer = new Node3D();
            pathNode.Name = path.SectionStyle == string.Empty ? "Connector" : path.SectionStyle + "-" + string.Format("000", path.SectionIndex);
            propContainer.Name = $"Props[{path.PropCount}]";
            tileContainer.Name = $"Tiles[{path.TileCount}]";
            mapContainer.AddChild(pathNode, true);
            pathNode.AddChild(propContainer, true);
            pathNode.AddChild(tileContainer, true);
            // If in editor we add to active scene
            if (Engine.IsEditorHint())
            {
                MoveToEditedScene(pathNode);
                MoveToEditedScene(propContainer);
                MoveToEditedScene(tileContainer);
            }

            // Tiles
            foreach (MapPiece rp in path.Pieces)
            {
                MapPiece piece = map.GetPiece(rp.Coord);
                if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, floors, walls, ceilings, true))
                {
                    tileContainer.AddChild(visualNode, true);
                    visualNode.Position = Dungeon.GlobalPosition(piece);
                    visualNode.Show();
                    //AddDebugVisuals(piece);
                    if (Engine.IsEditorHint())
                    {
                        MoveToEditedScene(visualNode);
                    }
                    count++;
                    if (count % 250 == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                    }
                }
            }
        }



        private async Task VisualizeRooms(bool floors, bool walls, bool ceilings)
        {
            foreach (RoomSection room in map.Sections)
            {
                await VisualizeRoom(room, floors, walls, ceilings);
            }
        }
        private async Task VisualizeRoom(RoomSection room, bool floors, bool walls, bool ceilings)
        {
            if (room == null) { return; }
            int count = 0;

            Node3D roomNode = new Node3D();
            propContainer = new Node3D();
            tileContainer = new Node3D();
            roomNode.Name = room.SectionStyle + "-" + string.Format("000", room.SectionIndex);
            propContainer.Name = $"Props[{room.PropCount}]";
            tileContainer.Name = $"Tiles[{room.Pieces.Count}]";
            mapContainer.AddChild(roomNode, true);
            roomNode.AddChild(propContainer, true);
            roomNode.AddChild(tileContainer, true);
            // If in editor we add to active scene
            if (Engine.IsEditorHint())
            {
                MoveToEditedScene(roomNode);
                MoveToEditedScene(propContainer);
                MoveToEditedScene(tileContainer);
            }

            // Room Tiles
            foreach (MapPiece rp in room.Pieces)
            {
                MapPiece piece = map.GetPiece(rp.Coord);
                if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, floors, walls, ceilings, true))
                {
                    tileContainer.AddChild(visualNode, true);
                    visualNode.Position = Dungeon.GlobalPosition(piece);
                    visualNode.Show();
                    //AddDebugVisuals(piece);
                    if (Engine.IsEditorHint())
                    {
                        MoveToEditedScene(visualNode);
                    }
                    count++;
                    if (count % 250 == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                    }
                }
            }
        }
        internal void ResetVisuals()
        {
            foreach (Node item in GetNode<NavigationRegion3D>("Generated").GetChildren())
            {
                item.QueueFree();
            }
            foreach (Node item in LevelDebug.GetChildren())
            {
                item.QueueFree();
            }
        }


        /// <summary>
        /// Decodes and instantiates the nodes needed for the map piece data
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="piece"></param>
        /// <param name="makeCollider"></param>
        internal bool BuildVisualNode(BiomeResource biome, MapPiece piece, out Node3D visualNode, Node3D propParent, bool floors, bool walls, bool ceilings, bool makeCollider = true)
        {
            visualNode = new Node3D();
            visualNode.Name = piece.CoordString;

            // generate floors
            if (floors)
            {
                if (piece.keyFloor.key != PIECEKEYS.NONE && piece.keyFloor.key != PIECEKEYS.OCCUPIED &&
                    GetByKey(piece.keyFloor, biome, out Node3D floor, makeCollider)) { visualNode.AddChild(floor); };
            }
            // generate walls
            if (walls)
            {
                if (piece.Walls.HasFlag(WALLS.N))
                {
                    if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider)) 
                    {
                        visualNode.AddChild(wall); 
                    };
                }
                if (piece.Walls.HasFlag(WALLS.E))
                {
                    if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider)) 
                    { 
                        visualNode.AddChild(wall);
                    };
                }
                if (piece.Walls.HasFlag(WALLS.S))
                {
                    if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider)) 
                    { 
                        visualNode.AddChild(wall);
                    };
                }
                if (piece.Walls.HasFlag(WALLS.W))
                {
                    if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider)) 
                    { 
                        visualNode.AddChild(wall);
                    };
                }



                // Not flagged as wall but check for rounded corner keys
                if (piece.WallKeyNorth.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyNorth, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall); };
                }
                if (piece.WallKeyEast.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyEast, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall); };
                }
                if (piece.WallKeySouth.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeySouth, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall); };
                }
                if (piece.WallKeyWest.key == PIECEKEYS.WCI)
                {
                    if (GetByKey(piece.WallKeyWest, biome, out Node3D wall, makeCollider)) { visualNode.AddChild(wall); };
                }




            }
            // generate ceiling
            if (ceilings)
            {
                if (piece.keyCeiling.key != PIECEKEYS.NONE && GetByKey(piece.keyCeiling, biome, out Node3D ceiling, makeCollider)) { visualNode.AddChild(ceiling); };
            }
            return true;
        }
        private void MoveToEditedScene(Node node)
        {
            if (Engine.IsEditorHint())
            {
                node.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
                for (int i = 0; i < node.GetChildCount(); i++)
                {
                    MoveToEditedScene(node.GetChild(i));
                }
            }
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
                if(makeCollider) { (obj as MeshInstance3D).CreateConvexCollision(); }
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
            if(cacheKeyedPieces == null) { cacheKeyedPieces = new Dictionary<PIECEKEYS, Dictionary<int, Resource>>(); }

            if (!cacheKeyedPieces.ContainsKey(data.key)) { cacheKeyedPieces[data.key] = new Dictionary<int, Resource>(); }

            if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
            {
                if(biome.GetResource(data.key, data.variantID, out Resource result))
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
                else if (biome.extras.Where(p => p.key == data.key).Count() > 0)
                {
                    cacheKeyedPieces[data.key][data.variantID] = biome.extras.Where(p => p.key == data.key).First().GetResource(data.variantID);
                }
         
            }
            if (!cacheKeyedPieces.ContainsKey(data.key))
            {
                GD.PrintErr($"ResolveAndCache Key [{data.key}] was not found!");
                return null;
            }
            if (!cacheKeyedPieces[data.key].ContainsKey(data.variantID))
            {
                if (!cacheKeyedPieces[data.key].ContainsKey(0))
                {
                    GD.PrintErr($"ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! And Default fallback failed!");
                    return null;
                }
                GD.PrintErr($"ResolveAndCache", $"Key [{data.key}] Variant [{data.variantID}] was not found! Default used as fallback.");
                return cacheKeyedPieces[data.key][0];
            }
            return cacheKeyedPieces[data.key][data.variantID];
        }

        internal MapPiece GetRandomPiece()
        {
            return map.GetRandomPiece();
        }

        internal RoomSection GetRandomRoom()
        {
            return map.GetRandomRoom() as RoomSection;
        }

        public void BuildNavMesh()
        {
            GetNode<NavigationRegion3D>("Generated").BakeNavigationMesh();
        }
        /// <summary>
        /// Note! This does not actually clear navmesh. It just rebuilds it so call it when there is nothing to build from
        /// </summary>
        public async void ClearNavMesh()
        {
            await Task.Delay(30);
            GetNode<NavigationRegion3D>("Generated").BakeNavigationMesh();
        }
    }// EOF CLASS
}
