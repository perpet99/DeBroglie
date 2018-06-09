﻿using System.Collections.Generic;
using System.Linq;

namespace DeBroglie.Console
{
    /**
     * Used with voxel generation, it matches the behaviour of 
     * https://github.com/sylefeb/VoxModSynth
     */
    public class BorderConstraint : IWaveConstraint
    {
        HashSet<int> groundPatterns;
        HashSet<int> nonGroundPatterns;
        HashSet<int> airPatterns;
        HashSet<int> nonAirPatterns;

        public BorderConstraint(TileModel<byte> model)
        {
            var groundPatterns = new HashSet<int>(model.TilesToPatterns[255]);
            var nonGroundPatterns = new HashSet<int>(Enumerable.Range(0, model.PatternCount).Except(groundPatterns));
            var airPatterns = new HashSet<int>(model.TilesToPatterns[0]);
            var nonAirPatterns = new HashSet<int>(Enumerable.Range(0, model.PatternCount).Except(airPatterns));
        }

        public CellStatus Check(WavePropagator wavePropagator)
        {
            return CellStatus.Undecided;
        }

        public CellStatus Init(WavePropagator wavePropagator)
        {
            var width = wavePropagator.Width;
            var height = wavePropagator.Height;
            var depth = wavePropagator.Depth;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var z = 0; z < depth; z++)
                    {
                        var isBoundary = x == 0 || x == width - 1 ||
                            y == 0 || y == height - 1 ||
                            z == 0 || z == depth - 1;
                        var patternsToBan = z == 0 ? nonGroundPatterns : isBoundary ? nonAirPatterns : groundPatterns;
                        foreach (var pattern in patternsToBan)
                        {
                            wavePropagator.Ban(x, y, z, pattern);
                        }
                    }
                }
            }
            return CellStatus.Undecided;
        }
    }
}
