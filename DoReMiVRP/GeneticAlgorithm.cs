﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Genetic;
using static Sys.Tool.GeneticAlgorithm;

namespace Sys.Tool
{
    //This class will accept one set of clustered data at a time and return the process results  
    public class GeneticAlgorithm
    {
        public event Action<EventArgsIterationLoop> OnIterationLoop;

        private int _selectionMethod = 0;
        private int _citiesCount = 0;

        public int citiesCount { get { return _citiesCount; } }
        public int populationSize = 100;
        public int iterations = 50;
        public EnumSelectionMethod selectionMethod = EnumSelectionMethod.Elite ;
        public bool greedyCrossover = true;

        public double[,] map = null;
        private System.Drawing.Color ClusterColor;

        public enum EnumSelectionMethod
        {
            Elite=0,
            Rank=1,
            Roulette=2
        }

       public void LoadMap(ArrayList Data)
        {
            map = new double[Data.Count, 2];
            _citiesCount = Data.Count;

            int i = 0;
            ClusterColor = ((ClusterData)Data[0]).Color;

            foreach (ClusterData item in Data)
            {
                map[i,0] = item.X;
                map[i,1] = item.Y;                
                i += 1;
            }
        } 


        public void Start(EnumSelectionMethod selectionmethod = EnumSelectionMethod.Elite,
                            int populationsize=100, int Iterations=50, bool IsgreedyCrossover = true)
        {
            // get population size
            this.populationSize = populationsize;
            this.selectionMethod = selectionmethod;
            this.iterations = Iterations;
            this.greedyCrossover = IsgreedyCrossover;

            _selectionMethod = (int) selectionmethod;

            if (populationSize > 100) populationSize = 100; // limit to Max 100
            if (populationSize < 50) populationSize = 50; // limit to Min 10
            populationSize = Math.Max(10, Math.Min(100, populationSize));
                       
            // iterations
            if (iterations > 100) iterations = 100; // limit to Max 200
            if (iterations < 50) iterations = 50; // limit to Min 10
            iterations = Math.Max(50, iterations);

            SearchSolution(ClusterColor);


        }


        private void SearchSolution(System.Drawing.Color clusterColor)
        {
            // create fitness function
            TSPFitnessFunction fitnessFunction = new TSPFitnessFunction(map);

            // create population
            Population population = new Population(populationSize,
                (greedyCrossover) ? new TSPChromosome(map) : new PermutationChromosome(citiesCount),
                fitnessFunction,
                (_selectionMethod == 0) ? (ISelectionMethod)new EliteSelection() :
                (_selectionMethod == 1) ? (ISelectionMethod)new RankSelection() :
                (ISelectionMethod)new RouletteWheelSelection()
                );

            // iterations
            int i = 1;

            // path
            double[,] path = new double[citiesCount + 1, 2];

            // loop
            while (true)
            {
                // run one epoch of genetic algorithm
                population.RunEpoch();

                // display current path
                ushort[] bestValue = ((PermutationChromosome)population.BestChromosome).Value;

                for (int j = 0; j < citiesCount; j++)
                {
                    path[j, 0] = map[bestValue[j], 0];
                    path[j, 1] = map[bestValue[j], 1];
                }
                path[citiesCount, 0] = map[bestValue[0], 0];
                path[citiesCount, 1] = map[bestValue[0], 1];
                
                // increase current iteration
                i++;
                
                if ((iterations != 0) && (i > iterations))
                    break;
            }
            //raiseevent
            EventArgsIterationLoop obj = new EventArgsIterationLoop();
            obj.citiesCount = citiesCount;
            obj.IterationCount = i;
            obj.populationSize = populationSize;
            obj.selectionMethod = selectionMethod;
            obj.greedyCrossover = greedyCrossover;
            obj.path = path;
            obj.color = clusterColor;
            IAsyncResult result = OnIterationLoop.BeginInvoke(obj, null, null);

        }


    }

    public struct EventArgsIterationLoop
    {
        public double[,] path;
        public int IterationCount;
        public int populationSize;
        public EnumSelectionMethod selectionMethod;
        public bool greedyCrossover;
        public int citiesCount;
        public System.Drawing.Color color;
    }

}
