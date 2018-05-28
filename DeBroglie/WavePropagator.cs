﻿using System;
using System.Collections.Generic;

namespace DeBroglie
{

    public enum CellStatus
    {
        Decided = 0,
        Undecided = -1,
        Contradiction = -2,
    }

    /**
     * WavePropagator holds a wave, and supports updating it's possibilities
     * according to the model constraints.
     */
    public class WavePropagator
    {
        private Wave wave;

        // From model
        private int[][][] propagator;
        private int patternCount;
        private double[] frequencies;

        private int width;
        private int height;
        private int indices;
        private bool periodic;
        private readonly IWaveConstraint[] constraints;
        private Random random = new Random();

        private Stack<PropagateItem> toPropagate;


        Directions directions;

        /**
          * compatible[index, pattern, direction] contains the number of patterns present in the wave
          * that can be placed in the cell next to index in the opposite direction of direction without being
          * in contradiction with pattern placed in index.
          * If possibilites[index][pattern] is set to false, then compatible[index, pattern, direction] has every direction negative or null
          */
        private int[,,] compatible;

        public WavePropagator(Model model, int width, int height, bool periodic, IWaveConstraint[] constraints = null)
        {
            this.propagator = model.Propagator;
            this.patternCount = model.PatternCount;
            this.frequencies = model.Frequencies;

            this.width = width;
            this.height = height;
            this.indices = width * height;
            this.periodic = periodic;
            this.constraints = constraints ?? new IWaveConstraint[0];
            this.directions = Directions.Cartesian2dDirections;

            this.toPropagate = new Stack<PropagateItem>();

            Clear();
        }

        // This is only exposed publically
        // in case users write their own constraints, it's not 
        // otherwise useful.
        #region Internal API

        public Wave Wave => wave;
        public int Width => width;
        public int Height => height;
        public int Indices => indices;
        public bool Periodic => periodic;
        public Directions Directions => directions;

        public int[][][] Propagator => propagator;
        public int PatternCount => patternCount;
        public double[] Frequencies => frequencies;


        public int GetIndex(int x, int y)
        {
            return x + y * width;
        }

        public void GetCoord(int index, out int x, out int y)
        {
            x = index % width;
            y = index / width;
        }

        public bool TryMove(int index, int direction, out int dest)
        {
            int x, y;
            GetCoord(index, out x, out y);
            return TryMove(x, y, direction, out dest);
        }

        public bool TryMove(int x, int y, int direction, out int dest)
        {
            x += directions.DX[direction];
            y += directions.DY[direction];
            if(periodic)
            {
                if (x < 0) x += width;
                if (x >= width) x -= width;
                if (y < 0) y += height;
                if (y >= height) y -= height;
            }
            else
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    dest = -1;
                    return false;
                }
            }
            dest = GetIndex(x, y);
            return true;
        }

        /**
         * Requires that index, pattern is possible
         */
        public bool InternalBan(int index, int pattern)
        {
            // Update compatible (so that we never ban twice)
            for (var d = 0; d < directions.Count; d++)
            {
                compatible[index, pattern, d] = 0;
            }
            // Queue any possible consequences of this changing.
            toPropagate.Push(new PropagateItem
            {
                Index = index,
                Pattern = pattern,
            });
            // Update the wave
            return wave.RemovePossibility(index, pattern);
        }

        public bool InternalSelect(int index, int chosenPattern)
        {
            for (var pattern = 0; pattern < patternCount; pattern++)
            {
                if (pattern == chosenPattern)
                {
                    continue;
                }
                if (wave.Get(index, pattern))
                {
                    if (InternalBan(index, pattern))
                        return true;
                }
            }
            return false;
        }
        #endregion

        private CellStatus Propagate()
        {
            PropagateItem item;
            while (toPropagate.TryPop(out item))
            {
                int x, y;
                GetCoord(item.Index, out x, out y);
                for (var d = 0; d < directions.Count; d++)
                {
                    int i2;
                    if (!TryMove(x, y, d, out i2))
                    {
                        continue;
                    }
                    var patterns = propagator[item.Pattern][d];
                    foreach (var p in patterns)
                    {
                        var c = --compatible[i2, p, d];
                        // We've just now ruled out this possible pattern
                        if (c == 0)
                        {
                            if (InternalBan(i2, p))
                            {
                                return CellStatus.Contradiction;
                            }
                        }
                    }
                }
            }
            return CellStatus.Undecided;
        }

        private int GetRandomPossiblePatternAt(int index)
        {
            var s = 0.0;
            for (var pattern = 0; pattern < patternCount; pattern++)
            {
                if (wave.Get(index, pattern))
                {
                    s += frequencies[pattern];
                }
            }
            var r = random.NextDouble() * s;
            for (var pattern = 0; pattern < patternCount; pattern++)
            {
                if (wave.Get(index, pattern))
                {
                    r -= frequencies[pattern];
                }
                if (r <= 0)
                {
                    return pattern;
                }
            }
            return patternCount - 1;
        }

        private CellStatus Observe()
        {
            // Choose a random cell
            var index = wave.GetRandomMinEntropyIndex(random);
            if (index == -1)
                return CellStatus.Decided;
            // Choose a random pattern
            var chosenPattern = GetRandomPossiblePatternAt(index);
            // Decide on the given cell
            if (InternalSelect(index, chosenPattern))
                return CellStatus.Contradiction;
            return CellStatus.Undecided;
        }

        // Returns the only possible value of a cell if there is only one,
        // otherwise returns -1 (multiple possible) or -2 (none possible)
        private int GetDecidedCell(int index)
        {
            int decidedPattern = (int)CellStatus.Contradiction;
            for (var pattern = 0; pattern < patternCount; pattern++)
            {
                if (wave.Get(index, pattern))
                {
                    if (decidedPattern == (int)CellStatus.Contradiction)
                    {
                        decidedPattern = pattern;
                    }
                    else
                    {
                        return (int)CellStatus.Undecided;
                    }
                }
            }
            return decidedPattern;
        }

        private CellStatus StepConstraints()
        {
            foreach (var constraint in constraints)
            {
                var status = constraint.Check(this);
                if (status != CellStatus.Undecided) return status;
                status = Propagate();
                if (status != CellStatus.Undecided) return status;
            }
            return CellStatus.Undecided;
        }

        /**
         * Resets the wave to it's original state
         */
        public void Clear()
        {
            wave = new Wave(frequencies, width * height);

            compatible = new int[indices, patternCount, directions.Count];
            for (int index = 0; index < indices; index++)
            {
                for (int pattern = 0; pattern < patternCount; pattern++)
                {
                    for (int d = 0; d < directions.Count; d++)
                    {
                        compatible[index, pattern, d] = propagator[pattern][directions.Inverse(d)].Length;
                    }
                }
            }

            StepConstraints();
        }

        /**
         * Removes pattern as a possibility from index
         */
        public CellStatus Ban(int x, int y, int pattern)
        {
            var index = GetIndex(x, y);
            if (wave.Get(index, pattern))
            {
                if (InternalBan(index, pattern))
                {
                    return CellStatus.Contradiction;
                }
            }
            return Propagate();
        }

        /**
         * Removes all other patterns as possibilities for index.
         */
        public CellStatus Select(int x, int y, int pattern)
        {
            var index = GetIndex(x, y);
            if (InternalSelect(index, pattern))
            {
                return CellStatus.Contradiction;
            }
            return Propagate();
        }

        /**
         * Make some progress in the WaveFunctionCollapseAlgorithm
         */
        public CellStatus Step()
        {
            CellStatus status = Observe();
            if (status != CellStatus.Undecided) return status;
            status = Propagate();
            if (status != CellStatus.Undecided) return status;
            return StepConstraints();
        }

        /**
         * Rpeatedly step until the status is Decided or Contradiction
         */
        public CellStatus Run()
        {
            CellStatus status;
            while (true)
            {
                status = Step();
                if (status != CellStatus.Undecided) return status;
            }
        }

        /**
         * Returns the array of decided patterns, writing
         * -1 or -2 to indicate cells that are undecided or in contradiction.
         */
        public int[,] ToArray()
        {
            var result = new int[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var index = GetIndex(x, y);
                    result[x, y] = GetDecidedCell(index);
                }
            }
            return result;
        }

        /**
         * Returns an array where each cell is a list of remaining possible patterns.
         */
        public List<int>[,] ToArraySets()
        {
            var result = new List<int>[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var index = GetIndex(x, y);
                    List<int> hs = result[x, y] = new List<int>();

                    for (var p = 0; p < patternCount; p++)
                    {
                        if (wave.Get(index, p))
                        {
                            hs.Add(p);
                        }
                    }
                }
            }
            return result;
        }

        private struct PropagateItem
        {
            public int Index { get; set; }
            public int Pattern { get; set; }
        }
    }
}