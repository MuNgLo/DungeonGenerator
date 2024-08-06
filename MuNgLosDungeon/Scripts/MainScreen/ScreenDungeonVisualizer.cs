using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Munglo.DungeonGenerator.UI
{
    /// <summary>
    /// The class that builds and updates the visual representation of the mapdata in the Dungeon Viewer
    /// </summary>
    [Tool]
    internal partial class ScreenDungeonVisualizer : Node3D
    {
        private MainScreen screen;
        private MapData map;
        private GenerationSettingsResource settings;
        private AddonSettings MasterConfig;
        private System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>> cacheKeyedPieces;
        private Node3D mapContainer;
        private Node3D propContainer;
        private Node3D tileContainer;
        private Node3D debugContainer;
        private BiomeResource biome;
        public MapData Map { get => map; }

        public override void _EnterTree()
        {
            screen = GetParent().GetParent().GetParent() as MainScreen;
            MasterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettings;
        }
        public async void BuildDungeon(GenerationSettingsResource settings, BiomeResource biome)
        {
            this.settings = settings;
            screen.ScreenNotify($"Generating:" + string.Format("{0:0}", 0) + "%");
            await ToSignal(GetTree(), "process_frame");
            //BuildData(biome, () => { ShowMap(null, null); }, settings.roomStart, settings.roomDefault);
            BuildData(biome, () => { ReDrawMap(); }, settings.roomStart, settings.roomDefault);
        }
        public async void BuildSection(string sectionTypeName, SectionResource sectionDef, Array<PlacerEntryResource> placers, GenerationSettingsResource settings, BiomeResource biome, Action callback)
        {
            this.settings = settings;
            screen.ScreenNotify($"Generating:" + string.Format("{0:0}", 0) + "%");
            await ToSignal(GetTree(), "process_frame");

            cacheKeyedPieces = new System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>>();
            this.biome = biome;

            map = new MapData(settings, settings.roomStart, settings.roomDefault);
            await map.GenerateSection(sectionTypeName, sectionDef, placers, callback);
        }


        private async void BuildData(BiomeResource biome, Action callback, SectionResource startRoom, SectionResource standardRoom)
        {
            cacheKeyedPieces = new System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>>();
            this.biome = biome;
            if (settings is null)
            {
                GD.PrintErr($"DungeonGenerator::BuildDungeon() BuildDungeonFailed! settings is NULL[{settings is null}] biome is NUll[{biome is null}]");
                return;
            }
            map = new MapData(settings, startRoom, standardRoom);
            await map.GenerateMap(callback);
        }
   
        /// <summary>
        /// Updates the visuals
        /// Obeying the floor start and floor end
        /// </summary>
        public async void ReDrawMap()
        {
            GD.Print($"ScreenDungeonVisulaizer::ReDrawMap()");
            cacheKeyedPieces = new System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>>();
            mapContainer = FindChild("Generated") as Node3D;
            debugContainer = FindChild("GeneratedDebug") as Node3D;
            for (int i = 0; i < settings.nbOfFloors; i++)
            {
                GetFloorContainer(i);
                GetFloorDebugContainer(i);
                if (i < MasterConfig.visibleFloorStart || i > MasterConfig.visibleFloorEnd) { ClearFloor(i); ClearDebugFloor(i); continue; }
                VisualizeFloor(i);
                await ToSignal(GetTree(), "process_frame");
            }
        }

        public void ReDrawSection()
        {
            GD.Print($"ScreenDungeonVisulaizer::ReDrawSection()");
            cacheKeyedPieces = new System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>>();
            mapContainer = FindChild("Generated") as Node3D;
            debugContainer = FindChild("GeneratedDebug") as Node3D;
            GetFloorContainer(0);
            GetFloorDebugContainer(0);
            VisualizeFloor(0);
        }
        /// <summary>
        /// Removes the visuals for a specific Floor
        /// </summary>
        /// <param name="floorIndex"></param>
        internal void ClearFloor(int floorIndex)
        {
            Node3D floorContainer = GetFloorContainer(floorIndex);

            if (floorContainer == null)
            {
                GD.Print($"ScreenDungeonVisulaizer::ClearFloor()  floor container node missing!");
                return;
            }
            foreach (Node child in floorContainer.GetChildren())
            {
                if (child is Node3D)
                {
                    child.QueueFree();
                }
            }
        }
        /// <summary>
        /// Removes the visuals for a specific Floor
        /// </summary>
        /// <param name="floorIndex"></param>
        internal void ClearDebugFloor(int floorIndex)
        {
            Node3D floorContainer = GetFloorDebugContainer(floorIndex);

            if (floorContainer == null)
            {
                GD.Print($"ScreenDungeonVisulaizer::ClearDebugFloor()  floor container node missing!");
                return;
            }
            foreach (Node child in floorContainer.GetChildren())
            {
                if (child is Node3D)
                {
                    child.QueueFree();
                }
            }
        }
        /// <summary>
        /// Clears All the visuals
        /// </summary>
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
        /// <summary>
        /// Clears All the debug visuals 
        /// </summary>
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
        /// <summary>
        /// Gets the floor parent. Creates if needed
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node3D GetFloorContainer(int y)
        {
            if (mapContainer.GetChildCount() < y + 1)
            {
                Node3D node = new Node3D();
                node.Name = string.Format("{0:000}" + "Floor", y);
                mapContainer.AddChild(node, true);
            }
            return mapContainer.GetChild(y) as Node3D;
        }
        /// <summary>
        /// Gets the floor parent. Creates if needed
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node3D GetFloorDebugContainer(int y)
        {
            if (debugContainer.GetChildCount() < y + 1)
            {
                Node3D node = new Node3D();
                node.Name = string.Format("{0:000}" + "Floor", y);
                debugContainer.AddChild(node, true);
            }
            return debugContainer.GetChild(y) as Node3D;
        }



      

        private void AddDebugVisuals(MapPiece piece)
        {
            // generate debug
            if (settings.debugPass)
            {
                Node3D floorParent = GetFloorDebugContainer(piece.Coord.y);

                foreach (KeyData keyData in piece.Debug)
                {
                    if (GetByKey(keyData, biome, out Node3D debugProp, false)) { floorParent.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
                if (piece.isBridge)
                {
                    if (GetByKey(
                        new KeyData() { key = PIECEKEYS.DEBUGBRIDGE, dir = piece.Orientation }
                        , biome, out Node3D debugProp, false)) { floorParent.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
                if (piece.hasStairs)
                {
                    if (GetByKey(
                    new KeyData() { key = PIECEKEYS.DEBUGSTAIR, dir = piece.Orientation }
                    , biome, out Node3D debugProp, false)) { floorParent.AddChild(debugProp); debugProp.Position = Dungeon.GlobalPosition(piece); };
                }
            }
        }


        private void VisualizeFloor(int floor)
        {
            ClearFloor(floor);

            foreach (ISection section in map.Sections)
            {
                if (section.Coord.y == floor || section.SectionIndex == 0)
                {
                        VisualizeSection(section);
                }
            }
        }
        private void VisualizeSection(ISection section)
        {
            if (section == null) { return; }
            section.SectionContainer = new Node3D();
            propContainer = new Node3D();
            tileContainer = new Node3D();
            section.SectionContainer.Name = "S" + string.Format("{0:000}", section.SectionIndex);
            propContainer.Name = $"Props[{section.PropCount}]";
            tileContainer.Name = $"Tiles[{section.Pieces.Count}]";
            GetFloorContainer(section.Coord.y).AddChild(section.SectionContainer, true);
            section.SectionContainer.AddChild(propContainer, true);
            section.SectionContainer.AddChild(tileContainer, true);
            // Section Tiles
            int index = 0;
            foreach (MapPiece rp in section.Pieces)
            {
                MapPiece piece = map.GetPiece(rp.Coord);
                if (BuildVisualNode(biome, piece, out Node3D visualNode, propContainer, true))
                {
                    visualNode.Name = $"S{string.Format("0:000", section.SectionIndex)}-T{index}";
                    tileContainer.AddChild(visualNode, true);
                    visualNode.Position = Dungeon.GlobalPosition(piece);
                    visualNode.Show();
                    AddDebugVisuals(piece);
                    index++;
                }
            }

            // Run the section prop placers
            if (section.Placers is not null)
            {
                foreach (PlacerEntryResource entry in section.Placers)
                {
                    if(entry is null || !entry.active  || entry.placer is null) { continue; }
                    IPlacer placer = entry.placer;

                    if (placer is not null)
                    {
                        if (screen.addon.Mode == VIEWERMODE.SECTION)
                        {
                            GD.Print($"ScreenDungeonVisualizer::VisualizeSection() Place on [{placer.ResourceName}]");
                        }
                        placer.Place(section);
                    }
                }
            }


            // Section Props
            foreach (SectionProp c in section.Props)
            {
                SpawnExtra(biome, c, true);
            }
        }
        internal void SpawnExtra(BiomeResource biome, SectionProp propData, bool makeCollider = true)
        {
            //GD.Print($"DungeonGenerator::SpawnRoomProp()");
            if (settings.showProps)
            {
                // skip arches 
                if(!settings.showArches)
                {
                    if (propData.key == PIECEKEYS.AS || propData.key == PIECEKEYS.ASIC) { return; }
                }


                if (GetByKey(new KeyData() { key = propData.key, dir = propData.dir, variantID = propData.variantID }, biome, out Node3D prop, makeCollider))
                {
                    propContainer.AddChild(prop, true);
                    //prop.GlobalPosition = Dungeon.GlobalRoomPropPosition(coord, propData.Offset);
                    prop.GlobalPosition = propData.position;
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
            // generate ceiling
            if (settings.showCeilings)
            {
                if (piece.keyCeiling.key != PIECEKEYS.NONE && GetByKey(piece.keyCeiling, biome, out Node3D ceiling, makeCollider)) { visualNode.AddChild(ceiling, true); };
            }
            // generate walls
            if (settings.showWalls)
            {
                for (int i = 1; i < 9; i *= 2)
                {
                    if (piece.Walls.HasFlag((WALLS)i))
                    {
                        if (GetByKey(piece.WallKey((MAPDIRECTION)Math.Log2(i) + 1), biome, out Node3D wall, makeCollider))
                        {
                            visualNode.AddChild(wall, true);
                        };
                    }
                }
                SpecialCaseRoundedCorners(piece, visualNode, biome, makeCollider);
            }
            return true;
        }
    
        /// <summary>
        /// Not flagged as wall but check for rounded corner keys
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="visualNode">parent</param>
        /// <param name="biome"></param>
        /// <param name="makeCollider"></param>
        private void SpecialCaseRoundedCorners(MapPiece piece, Node3D visualNode, BiomeResource biome, bool makeCollider)
        {
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
        /// <summary>
        /// returns a Node with the correct rotation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="biome"></param>
        /// <param name="obj"></param>
        /// <param name="makeCollider"></param>
        /// <returns></returns>
        private bool GetByKey(KeyData data, BiomeResource biome, out Node3D obj, bool makeCollider)
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
            if (cacheKeyedPieces == null) { cacheKeyedPieces = new System.Collections.Generic.Dictionary<PIECEKEYS, System.Collections.Generic.Dictionary<int, Resource>>(); }

            if (!cacheKeyedPieces.ContainsKey(data.key)) { cacheKeyedPieces[data.key] = new System.Collections.Generic.Dictionary<int, Resource>(); }

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

        /// <summary>
        /// Allow lookup of current MapData
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public MapPiece GetMapPiece(MapCoordinate coord)
        {
            if(map is null || map.Pieces.Count == 0)
            {
                GD.PushError("Mapdata needs to be rebuilt");
                return null;
            }
            return map.GetExistingPiece(coord);
        }
    }// eof class
}
