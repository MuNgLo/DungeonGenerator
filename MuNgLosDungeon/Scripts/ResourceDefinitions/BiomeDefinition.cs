using Godot;
using System;
using System.Linq;
using System.Resources;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class BiomeDefinition : DungeonAddonResource
    {
        [Export] public Vector3I size = Vector3I.One * 6;
        [ExportCategory("Debug")]
        [Export] public BiomeEntry[] debug;
        [ExportCategory("Walls")]
        [Export] public BiomeEntry[] walls;
        [ExportCategory("Floors")]
        [Export] public BiomeEntry[] floors;
        [ExportCategory("Ceilings")]
        [Export] public BiomeEntry[] ceilings;
        [ExportCategory("Props")]
        [Export] public BiomeEntry[] props;

        internal bool GetResource(PIECEKEYS key, int variantID, out Resource result)
        {
            result = null;
            BiomeEntry entry = null;
            if (debug.Where(p => p.key == key).Count() > 0) { entry = debug.Where(p => p.key == key).First(); }
            if (entry is null) { if (walls.Where(p => p.key == key).Count() > 0) { entry = walls.Where(p => p.key == key).First(); } }
            if (entry is null) { if (floors.Where(p => p.key == key).Count() > 0) { entry = floors.Where(p => p.key == key).First(); } }
            if (entry is null) { if (ceilings.Where(p => p.key == key).Count() > 0) { entry = ceilings.Where(p => p.key == key).First(); } }
            if (entry is null) { if (props.Where(p => p.key == key).Count() > 0) { entry = props.Where(p => p.key == key).First(); } }
            if (entry is null) { return false; }
            if (entry.resources.Length < 1)
            {
                GD.PrintErr($"BiomeDefinition::GetResouce({key}, {variantID}) No resources setup under that key!");
                return false;
            }

            if (variantID > 0 && variantID < entry.resources.Length)
            {
                result = entry.resources[variantID];
                return true;
            }
            if(variantID >= entry.resources.Length)
            {
                GD.PrintErr($"BiomeDefinition::GetResouce({key}, {variantID}) Variant dont exist!");
            }
            result = entry.resources[0];
            return true;
        }

        // Make sure you provide a parameterless constructor.
        // In C#, a parameterless constructor is different from a
        // constructor with all default values.
        // Without a parameterless constructor, Godot will have problems
        // creating and editing your resource via the inspector.
        //public BiomeDefinition() : this(0, null, null) { }
        public BiomeDefinition()
        {
            //debug defaults
            PIECEKEYS[] defDebugKeys = new PIECEKEYS[]
            {
                PIECEKEYS.DEBUG,
                PIECEKEYS.ERROR,
                PIECEKEYS.FAULTY,
                PIECEKEYS.WFGREEN,
                PIECEKEYS.WFRED,
                PIECEKEYS.DEBUGPATHEND,
                PIECEKEYS.DEBUGBRIDGE,
                PIECEKEYS.DEBUGSTAIR
            };
            string[] defDebugEntries = new string[]
            {
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_DebugMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_ErrorMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_FaultyMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_WallFlagGreenMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_WallFlagRedMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_PathEndMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_BridgeMESH.res",
                "res://addons/MuNgLosDungeon/Meshes/DefaultMeshes_StairMESH.res"
            };
            debug = new BiomeEntry[defDebugKeys.Length];
            SetupArray(ref debug, defDebugKeys, defDebugEntries);
            //walls
            PIECEKEYS[] defWallsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.W,
                    PIECEKEYS.WD,
                    PIECEKEYS.WDW,
                    PIECEKEYS.WCI
              };
            string[][] defWallsEntries = new string[][]
            {
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_wall01.tscn", "res://addons/MuNgLosDungeon/Scenes/def_wall02.tscn", "res://addons/MuNgLosDungeon/Scenes/def_wall03.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_walldooropening.tscn", "res://addons/MuNgLosDungeon/Scenes/def_walldooropening02.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_walldoorwide.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_wallcornerinner.tscn" }
            };
            walls = new BiomeEntry[defWallsKeys.Length];
            SetupArray(ref walls, defWallsKeys, defWallsEntries);
            //floors
            PIECEKEYS[] defFloorsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.F
              };
            string[][] defFloorsEntries = new string[][]
            {
                new string[] { "res://addons/MuNgLosDungeon/Scenes/def_floor.tscn", "res://addons/MuNgLosDungeon/Scenes/def_floor02.tscn", "res://addons/MuNgLosDungeon/Scenes/def_floor03.tscn" }
            };
            floors = new BiomeEntry[defFloorsKeys.Length];
            SetupArray(ref floors, defFloorsKeys, defFloorsEntries);
            //ceilings
            PIECEKEYS[] defCeilingsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.C
              };
            string[] defCeilingsEntries = new string[]
            {
                "res://addons/MuNgLosDungeon/Scenes/def_ceiling.tscn"
            };
            ceilings = new BiomeEntry[defCeilingsKeys.Length];
            SetupArray(ref ceilings, defCeilingsKeys, defCeilingsEntries);
            //prop
            PIECEKEYS[] defPropsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.AS,
                    PIECEKEYS.ASIC,
                    PIECEKEYS.BRIDGE,
                    PIECEKEYS.STAIRSPIRAL,
                    PIECEKEYS.STAIRPLATFORM,
                    PIECEKEYS.STAIR
              };
            string[][] defPropsEntries = new string[][]
            {
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_archsmall.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_archsmallcorner.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_bridgesection.tscn", "res://addons/MuNgLosDungeon/Scenes/def_bridgesection02.tscn",
                    "res://addons/MuNgLosDungeon/Scenes/def_bridgefoundation.tscn", 
                    "res://addons/MuNgLosDungeon/Scenes/def_bridgerail.tscn", "res://addons/MuNgLosDungeon/Scenes/def_bridgerail02.tscn", 
                    "res://addons/MuNgLosDungeon/Scenes/def_bridgepost.tscn", "res://addons/MuNgLosDungeon/Scenes/def_balcony.tscn"},
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_stairspiral01.tscn", "res://addons/MuNgLosDungeon/Scenes/def_stairspiral02.tscn", "res://addons/MuNgLosDungeon/Scenes/def_stairspiral03.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_stairsplatform.tscn" },
                new string[]{ "res://addons/MuNgLosDungeon/Scenes/def_stairs01.tscn", "res://addons/MuNgLosDungeon/Scenes/def_stairs02.tscn" }
            };
            props = new BiomeEntry[defPropsKeys.Length];
            SetupArray(ref props, defPropsKeys, defPropsEntries);
        }

       

        private void SetupArray(ref BiomeEntry[] arr, PIECEKEYS[] keys, string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                arr[i] = new BiomeEntry()
                {
                    key = keys[i],
                    resources = new Resource[1]
                    {
                        ResourceLoader.Load(paths[i])
                    }
                };

            }
        }

        private void SetupArray(ref BiomeEntry[] arr, PIECEKEYS[] keys, string[][] variants)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                Resource[] resources = new Resource[variants[i].Length];
                for (int b = 0; b < variants[i].Length; b++)
                {
                    resources[b] = ResourceLoader.Load(variants[i][b]);
                }
                arr[i] = new BiomeEntry()
                {
                    key = keys[i],
                    resources = resources
                };

            }
        }




    }// EOF CLASS
}