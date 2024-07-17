using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator.PropGrid
{
    /// <summary>
    /// Instantiated under a Section to store what props the section has.
    /// See PropHelper static class for useful things
    /// 1m³ placement resolution.
    /// </summary>
    internal class SectionProps
    {
        // The section this is part of
        private SectionBase section;
        // Data table of the prop stored
        internal List<SectionProp> props;

        internal List<SectionProp> Props => props;

        internal SectionProps(SectionBase section)
        {
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

    }// EOF CLASS
}
