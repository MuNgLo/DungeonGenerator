using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// Keeps track of props per map coordinate for internal props on the 1m³ placement resolution
    /// </summary>
    internal class RoomProps
    {
        private SectionBase room;

        internal Dictionary<MapCoordinate, Dictionary<Vector3I, RoomProp>> grids;

        internal List<RoomProp> All => GetAllProps();

        private List<RoomProp> GetAllProps()
        {
            throw new NotImplementedException();
        }

        internal RoomProps(SectionBase section)
        {
            grids = new Dictionary<MapCoordinate, Dictionary<Vector3I, RoomProp>>();
            this.room = section;
        }

        //private List<RoomProp> AllProps()
        //{
        //    List<RoomProp> allProps = props;
        //    foreach (StairsDrop stair in stairs)
        //    {
        //        allProps.AddRange(stair.Props);
        //    }
        //    return allProps;
        //}
        /// <summary>
        /// Removes all mathching keys from props. Then add one
        /// </summary>
        /// <param name="kData"></param>
        internal void Add(MapCoordinate coord, RoomProp pData)
        {
            //RemoveProp(pData);
            if(!grids.Keys.Contains(coord)) 
            {
                grids[coord] = new Dictionary<Vector3I, RoomProp>();
            }
            grids[coord][pData.Offset] = pData;
        }
        /// <summary>
        /// Removes all mathching keys from props.
        /// </summary>
        /// <param name="kData"></param>
        internal void RemoveProp(RoomProp pData)
        {
            //int count = props.RemoveAll(p => p.key == pData.key && p.Offset == pData.Offset && p.dir == pData.dir);
            //if (count > 0) { GD.Print($"Removed {count} props on {pData.Offset}"); }
        }
    }
}
