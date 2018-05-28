﻿using System;

namespace DeBroglie
{

    class Program
    {
        private static void Write(OverlappingModel model, WavePropagator propagator)
        {
            var results = model.ToArray(propagator);

            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var r = results[x, y];
                    string c;
                    switch (r)
                    {
                        case (int)CellStatus.Undecided: c = "?"; break;
                        case (int)CellStatus.Contradiction: c = "*"; break;
                        case 0: c = " "; break;
                        default: c = r.ToString(); break;
                    }
                    Console.Write(c);
                }
                Console.WriteLine();
            }
        }

        private static void WriteSteps(OverlappingModel model, WavePropagator propagator)
        {
            Write(model, propagator);
            Console.WriteLine();

            while (true)
            {
                var status = propagator.Step();
                Write(model, propagator);
                Console.WriteLine();
                if (status != CellStatus.Undecided)
                {
                    Console.WriteLine(status);
                    break;
                }
            }
        }

        private static CellStatus Run(WavePropagator propagator, int retries)
        {
            CellStatus status = CellStatus.Undecided;
            for (var retry = 0; retry < retries; retry++)
            {
                status = propagator.Run();
                if (status == CellStatus.Decided)
                {
                    break;
                }
            }
            return status;
        }

        static void Main(string[] args)
        {
            int[,] sample =
            {
                { 0, 0, 1, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0 },
                { 0, 0, 1, 1, 1, 1 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
            };

            var model = new OverlappingModel(sample, 3, false, 8);

            var pathConstraint = PathConstraint.Create(model, new[] { 1 }, new[]{
                new Point(0, 0),
                new Point(9, 9),
            });

            var propagator = new WavePropagator(model, 10, 10, false, new[] { pathConstraint });

            var status = Run(propagator, 5);
            Write(model, propagator);
        }
    }
}