using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Munglo.DungeonGenerator.PropGrid
{
    /// <summary>
    /// Instantiated under a Section to store what props the section has.
    /// See PropHelper static class for useful things
    /// 1m³ placement resolution.
    /// </summary>
    internal class SectionProps
    {
        /// <summary>
        /// The determenistic random number generator for this
        /// </summary>
        private protected readonly PRNGMarsenneTwister rng;
        // The section this is part of
        private SectionBase section;
        // Data table of the prop stored
        internal List<SectionProp> props;

        internal List<SectionProp> Props => props;

        internal SectionProps(SectionBase section, ulong[] seed)
        {
            rng = new PRNGMarsenneTwister(seed);
            props = new List<SectionProp>();
            this.section = section;
        }
        /// <summary>
        /// Verify that the worldposition is inside the section
        /// Return False if adding failed
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="pData"></param>
        internal bool Add(SectionProp pData)
        {
            if (!section.IsInside(pData.position))
            {
                return false;
            }
            props.Add(pData);
            return true;
        }
        internal Vector3 GetRandomPropPosition()
        {
            Vector3 offset = new Vector3(-3.5f, -1.0f, -3.5f);
            Vector3I pos = Vector3I.Zero;
            int breaker = 10;
            while(breaker > 0)
            {
                breaker--;
                pos.X = rng.Next(section.MinCoord.x * 6, section.MaxCoord.x * 6 + 6);
                pos.Y = rng.Next(section.MinCoord.y * 6, section.MaxCoord.y * 6 + 6);
                pos.Z = rng.Next(section.MinCoord.z * 6, section.MaxCoord.z * 6 + 6);
                if(section.IsInside((Vector3I)(pos + offset))){ break; }
            }
            return pos + offset;
        }

        internal List<Vector3> GetFloorPositions(int sectionLevel)
        {
            List<Vector3> result = new List<Vector3>();
            int minX = section.MinCoord.x * 6 + 1;
            int maxX = (section.MaxCoord.x + 1) * 6 -1;
            int minZ = section.MinCoord.z * 6 + 1;
            int maxZ = (section.MaxCoord.z + 1) * 6 -1;
            for (int x = minX; x < maxX; x++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    Vector3 pos = new Vector3(x - 2.5f, 0, z - 2.5f);
                    if (section.IsInside(pos))
                    { 
                        result.Add(pos);
                    }
                }
            }
            return result;
        }
    }// EOF CLASS
}
