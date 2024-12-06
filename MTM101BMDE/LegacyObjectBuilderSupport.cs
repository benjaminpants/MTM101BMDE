using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI
{
    /// <summary>
    /// Add this component to your old ObjectBuilders to hopefully keep them functional for the moment.
    /// I suggest switching away from this class as soon as possible.
    /// (Untested)
    /// </summary>
    [Obsolete("Switch to StructureBuilder ASAP.")]
    public class LegacyObjectBuilderSupport : StructureBuilder
    {
        public ObjectBuilder builderToBuild;

        public override void Generate(LevelGenerator lg, Random rng)
        {
            base.Generate(lg, rng);
            builderToBuild.Build(ec, lg, lg.halls[0], rng);
        }

        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            builderToBuild.Load(ec, data.Select(x => x.position).ToList(), data.Select(x => x.direction).ToList());
        }
    }
}
