using Godot;
using System;
using System.Linq;
using System.Resources;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class BiomeResource : DungeonAddonResource
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
        [ExportCategory("Extras")]
        [Export] public BiomeEntry[] extras;

        private readonly string standardMeshPath = "res://addons/MDunGen/Meshes/Standard/Standard_";
        private readonly string standardScenePath = "res://addons/MDunGen/Scenes/Standard/";
        internal bool GetResource(PIECEKEYS key, int variantID, out Resource result)
        {
            result = null;
            BiomeEntry entry = null;
            if (debug.Where(p => p.key == key).Count() > 0) { entry = debug.Where(p => p.key == key).First(); }
            if (entry is null) { if (walls.Where(p => p.key == key).Count() > 0) { entry = walls.Where(p => p.key == key).First(); } }
            if (entry is null) { if (floors.Where(p => p.key == key).Count() > 0) { entry = floors.Where(p => p.key == key).First(); } }
            if (entry is null) { if (ceilings.Where(p => p.key == key).Count() > 0) { entry = ceilings.Where(p => p.key == key).First(); } }
            if (entry is null) { if (extras.Where(p => p.key == key).Count() > 0) { entry = extras.Where(p => p.key == key).First(); } }
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
        public BiomeResource()
        {
            //debug defaults
            PIECEKEYS[] defDebugKeys = new PIECEKEYS[]
            {
                PIECEKEYS.DEBUG,
                PIECEKEYS.ERROR,
                PIECEKEYS.FAULTY,
                PIECEKEYS.WFGREEN,
                PIECEKEYS.WFRED
            };
            string[] defDebugEntries = new string[]
            {
                standardMeshPath + "dbArrow.res",
                standardMeshPath + "dbError.res",
                standardMeshPath + "dbFaulty.res",
                standardMeshPath + "dbWallFlagGreen.res",
                standardMeshPath + "dbWallFlagRed.res"
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
                new string[]{ standardScenePath + "def_wall01.tscn", standardScenePath + "def_wall02.tscn", standardScenePath + "def_wall03.tscn" },
                new string[]{ standardScenePath + "def_wallopening01.tscn", standardScenePath + "def_wallopening02.tscn" },
                new string[]{ standardScenePath + "def_wallopeningwide.tscn" },
                new string[]{ standardScenePath + "def_wallcorner.tscn" }
            };
            walls = new BiomeEntry[defWallsKeys.Length];
            SetupArray(ref walls, defWallsKeys, defWallsEntries);
            
            // Floors
            PIECEKEYS[] defFloorsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.F
              };
            string[][] defFloorsEntries = new string[][]
            {
                new string[]{ 
                standardScenePath + "def_floor01.tscn",
                 standardScenePath + "def_floor02.tscn",
                  standardScenePath + "def_floor03.tscn", 
                   standardScenePath + "def_hallwayfloor01.tscn", 
                   standardScenePath + "def_hallwayfloor02.tscn"
                }
            };
            floors = new BiomeEntry[defFloorsKeys.Length];
            SetupArray(ref floors, defFloorsKeys, defFloorsEntries);
            
            // Ceilings
            PIECEKEYS[] defCeilingsKeys = new PIECEKEYS[]
              {
                    PIECEKEYS.C
              };
            string[][] defCeilingsEntries = new string[][]
            {
                new string[]{ standardScenePath + "def_ceiling.tscn" }
            };
            ceilings = new BiomeEntry[defCeilingsKeys.Length];
            SetupArray(ref ceilings, defCeilingsKeys, defCeilingsEntries);
            
            // Extras
            PIECEKEYS[] defExtrakeys = new PIECEKEYS[] 
            { 
                PIECEKEYS.ARCH,
            };
            string[][] defExtrasEntries = new string[][]
            {
                new string[]{ 
                    standardScenePath + "def_arch.tscn", 
                    standardScenePath + "def_archcorner.tscn", 
                    standardScenePath + "def_hallwayarch01.tscn", 
                    }
            };
            extras = new BiomeEntry[defExtrakeys.Length];
            SetupArray(ref extras, defExtrakeys, defExtrasEntries);
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