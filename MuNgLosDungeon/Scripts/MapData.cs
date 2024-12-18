﻿using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// On instantiation this sets itself up to be used. Call GenerateMap() togenerate dungeon data. Use its callback to
    /// do what you want with it.
    /// </summary>
    public class MapData
    {
        private System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>> pieces;
        private GenerationSettingsResource mapArgs;
        internal SectionResource startRoom;
        internal SectionResource standardRoom;
        private List<ISection> sections;
        private System.Collections.Generic.Dictionary<MapCoordinate, SectionConnection> connections;
        private PRNGMarsenneTwister rng;

        public List<ISection> Sections => sections;
        public System.Collections.Generic.Dictionary<MapCoordinate, SectionConnection> Connections { get => connections; set => connections = value; }
        public GenerationSettingsResource MapArgs => mapArgs;

        internal System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>> Pieces => pieces;
        internal int nbOfPieces => pieces.Values.SelectMany(p => p.Values).Distinct().SelectMany(p => p.Values).Distinct().Count(); // Würkz
        internal MapData(GenerationSettingsResource args, SectionResource startRoom, SectionResource standardRoom)
        {
            sections = new List<ISection>();
            connections = new System.Collections.Generic.Dictionary<MapCoordinate, SectionConnection>();
            pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>();
            mapArgs = args;
            this.startRoom = startRoom;
            this.standardRoom = standardRoom;
        }
        /// <summary>
        /// This generates the data that describes the layout of the dungeon
        /// When done it calls back
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal async Task GenerateMap(Action callback)
        {
            GD.Print("MapData::GenerateMap() Generation started.....");
            rng = new PRNGMarsenneTwister(MapArgs.Seed);
            MapBuilder builder = new MapBuilder(this);
            await builder.BuildMapData();
            callback.Invoke();
        }

        internal async Task GenerateSection(string sectionTypeName, SectionResource sectionDef, Array<PlacerEntryResource> placers, Action callback)
        {
            GD.Print($"MapData::GenerateSection() Generation started..... defIsNull[{sectionDef is null}] placersisNull[{placers is null}]");
            MapBuilder builder = new MapBuilder(this);
            await builder.BuildSection(sectionTypeName, sectionDef, placers);
            callback.Invoke();
        }

        internal void SavePiece(MapPiece piece)
        {
            pieces[piece.Coord.x][piece.Coord.y][piece.Coord.z] = piece;
        }

        /// <summary>
        /// TODO rework this
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="piece"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool GetRandomRandomEdgePiece(int floor, out MapPiece piece, out MAPDIRECTION dir)
        {

            int y = floor;
            int x = -1;
            int z = -1;

            dir = MAPDIRECTION.NORTH;

            // N or S
            if (rng.Next(100) < 50)
            {
                x = rng.Next(5);
                if (rng.Next(100) < 50)
                {
                    z = 0;
                    dir = MAPDIRECTION.SOUTH;
                }
                else
                {
                    z = 5 - 1;
                    dir = MAPDIRECTION.NORTH;
                }
            }
            else // W or E
            {
                z = rng.Next(5);
                if (rng.Next(100) < 50)
                {
                    x = 0;
                    dir = MAPDIRECTION.EAST;
                }
                else
                {
                    x = 5 - 1;
                    dir = MAPDIRECTION.WEST;
                }
            }

            VerifyPiece(new MapCoordinate( x, y, z));
            piece = pieces[x][y][z];
            return true;
        }


        internal ISection GetRandomRoom()
        {
            return sections[rng.Next(0, sections.Count)];
        }



        internal MapPiece GetRandomPiece()
        {
            int x = pieces.ElementAt(rng.Next(pieces.Keys.Count)).Key;
            int y = pieces[x].ElementAt(rng.Next(pieces[x].Keys.Count)).Key;
            int z = pieces[x][y].ElementAt(rng.Next(pieces[x][y].Keys.Count)).Key;
            MapPiece pick = pieces[x][y][z];
            while (pick.keyFloor.key != PIECEKEYS.F)
            {
                x = pieces.ElementAt(rng.Next(pieces.Keys.Count)).Key;
                y = pieces[x].ElementAt(rng.Next(pieces[x].Keys.Count)).Key;
                z = pieces[x][y].ElementAt(rng.Next(pieces[x][y].Keys.Count)).Key;
                pick = pieces[x][y][z];
            }
            //ProcGenMKIII.Log("MapData", "GetRandomPiece", $"pieces.Keys.Count[{pieces.Keys.Count}] pieces.ElementAt(0)[{pieces.ElementAt(0)}]");
            //ProcGenMKIII.Log("MapData", "GetRandomPiece", $"pieces.Keys.Count[{pieces[x].Keys.Count}] pieces[x].ElementAt(0)[{(pieces[x].ElementAt(0))}]");
            return pick;
        }
        /// <summary>
        /// Uses piece verification and then return the piece.
        /// Will create piece if needed.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        internal MapPiece GetPiece(MapCoordinate coord)
        {
            VerifyPiece(coord);
            return pieces[coord.x][coord.y][coord.z];
        }
     

        /// <summary>
        /// Get piece if it exists or return null
        /// Use this when iterating across map to not change map
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        internal MapPiece GetExistingPiece(MapCoordinate coord)
        {
            if (pieces.ContainsKey(coord.x))
            {
                if (pieces[coord.x].ContainsKey(coord.y))
                {
                    if (pieces[coord.x][coord.y].ContainsKey(coord.z))
                    {
                        return pieces[coord.x][coord.y][coord.z];
                    }
                }
            }
            return null;
        }
   
        /// <summary>
        /// Returns a List of pieces with the state
        /// </summary>
        /// <param name="queriedState"></param>
        /// <returns></returns>
        internal List<MapPiece> GetPieces(MAPPIECESTATE queriedState)
        {
            List<MapPiece> picked = new List<MapPiece>();

            foreach (int keyX in pieces.Keys)
            {
                foreach (int keyY in pieces[keyX].Keys)
                {
                    foreach (int keyZ in pieces[keyX][keyY].Keys)
                    {
                        if (pieces[keyX][keyY][keyZ].State == queriedState)
                        {
                            picked.Add(pieces[keyX][keyY][keyZ]);
                        }
                    }
                }
            }
            return picked;
        }
  
        /// <summary>
        /// Verifies piece instance exists. Makes one if needed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="verbose"></param>
        private void VerifyPiece(MapCoordinate coord, bool verbose = false)
        {
            if (pieces == null) { pieces = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>>(); }

            if (!pieces.Keys.Contains(coord.x)) { pieces[coord.x] = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapPiece>>(); }
            if (!pieces[coord.x].Keys.Contains(coord.y)) { pieces[coord.x][coord.y] = new System.Collections.Generic.Dictionary<int, MapPiece>(); }
            if (!pieces[coord.x][coord.y].Keys.Contains(coord.z))
            {
                if (verbose) { GD.PrintErr($"VerifyPieceSpace", $"insert blank piece [{coord.x}.{coord.y}.{coord.z}]"); }

                pieces[coord.x][coord.y][coord.z] = new MapPiece(this, coord);
            }

        }

        internal void AddOpeningBetweenSections(SectionConnection connection, bool overrideLocked)
        {
            MapPiece p1 = GetExistingPiece(connection.Coord);
            MapPiece p2 = GetExistingPiece(connection.Coord + connection.Dir);
            AddOpeningBetweenSections(p1, p2, connection.Dir, overrideLocked);
        }

        internal void AddOpeningBetweenSections(MapPiece p1, MapPiece p2, MAPDIRECTION dir, bool overrideLocked)
        {
            if (p1.SectionIndex < 0 || p2.SectionIndex < 0) return;
            if (p1.SectionIndex == p2.SectionIndex) return;
            sections[p1.SectionIndex].AddOpening(p1.Coord, dir, false, overrideLocked);
            sections[p2.SectionIndex].AddOpening(p2.Coord, Dungeon.Flip(dir), false, overrideLocked);

            sections[p2.SectionIndex].Connections.Add(p1.Coord);
        }
        public MapPiece GetNextPieceOver(MapPiece startPiece, MAPDIRECTION orientation)
        {

            switch (orientation)
            {
                case MAPDIRECTION.NORTH:
                    startPiece = GetPiece(startPiece.Coord.StepNorth);
                    break;
                case MAPDIRECTION.EAST:
                    startPiece = GetPiece(startPiece.Coord.StepEast);
                    break;
                case MAPDIRECTION.SOUTH:
                    startPiece = GetPiece(startPiece.Coord.StepSouth);
                    break;
                case MAPDIRECTION.WEST:
                    startPiece = GetPiece(startPiece.Coord.StepWest);
                    break;
                case MAPDIRECTION.UP:
                    startPiece = GetPiece(startPiece.Coord.StepUp);
                    break;
                case MAPDIRECTION.DOWN:
                    startPiece = GetPiece(startPiece.Coord.StepDown);
                    break;
            }
            return startPiece;
        }
      
        #region Functional to manipulate mappieces
      

        internal void AddDoorWide(MapPiece piece1, bool overrideLocked)
        {
            MapPiece piece2 = piece1.Neighbour(Dungeon.TwistRight(piece1.Orientation), true);
            piece1.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = Dungeon.Flip(piece1.Orientation) }, overrideLocked); 
            piece2.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = Dungeon.Flip(piece1.Orientation) }, overrideLocked);
            piece1.Neighbour(Dungeon.Flip(piece1.Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = piece1.Orientation }, overrideLocked);
            piece2.Neighbour(Dungeon.Flip(piece1.Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = piece1.Orientation }, overrideLocked);
        }

     

        #endregion
    }// EOF CLASS
}
