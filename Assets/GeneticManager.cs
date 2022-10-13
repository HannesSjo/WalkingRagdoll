using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;
public class GeneticManager : MonoBehaviour {
    [Header("Refrences")]
    public StickmanController Controller;

    [Header("Controls")]
    public int InitialPopulation = 50;
    [Range(0.0f, 1.0f)]
    public float MutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int BestAgentSelection = 8;
    public int WorstAgentSelection = 3;
    public int NumberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;

    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start() {
        CreatePopulation();
    }

    private void CreatePopulation() {
        population = new NNet[InitialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome() {
        Controller.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationWithRandomValues(NNet[] newPopulation, int startingIndex) {
        while (startingIndex < InitialPopulation) {
            newPopulation[startingIndex] = new NNet();
            newPopulation[startingIndex].Initialise(Controller.Layers, Controller.Nodes);
            startingIndex++;
        }
    }

    public void Death (float fitness, NNet network) {
        if (currentGenome < population.Length - 1) {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }else {
            Repopulate();
        }
    }

    private void Repopulate() {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        NNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);
        population = newPopulation;

        currentGenome = 0;

        ResetToCurrentGenome();
    }

    private void Mutate(NNet[] newPopulation) {
        for (int i = 0; i < naturallySelected; i++) {
            for(int y = 0; y < newPopulation[i].weights.Count; y++) {
                if (Random.Range(0.0f, 1.0f) < MutationRate) {
                    newPopulation[i].weights[y] = MutateMatrix(newPopulation[i].weights[y]);
                }
            }
        }
    }

    Matrix<float> MutateMatrix (Matrix<float> m) {
        int random = Random.Range(1, (m.RowCount * m.ColumnCount) / 7);

        Matrix<float> R = m;

        for(int i = 0; i <random; i++) {
            int randomColumn = Random.Range(0, R.ColumnCount);
            int randomRow = Random.Range(0, R.RowCount);

            R[randomRow, randomColumn] = Mathf.Clamp(R[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return R;
    }

    private void Crossover(NNet[] newPopulation) {
        for (int i = 0; i < NumberToCrossover; i += 2) {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count <= 1) {
                for (int l = 0; l < 100; l++) {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            NNet Child1 = new NNet();
            NNet Child2 = new NNet();

            Child1.Initialise(Controller.Layers, Controller.Nodes);
            Child2.Initialise(Controller.Layers, Controller.Nodes);

            Child1.fitness = 0;
            Child2.fitness = 0;

            for (int x = 0; x < Child1.weights.Count; x++) {
                if (Random.Range(0.0f, 1.0f) < 0.5f) {
                    Child1.weights[x] = population[AIndex].weights[x];
                    Child2.weights[x] = population[BIndex].weights[x];
                } else {
                    Child2.weights[x] = population[AIndex].weights[x];
                    Child1.weights[x] = population[BIndex].weights[x];
                }
            }

            for (int x = 0; x < Child1.biases.Count; x++) {
                if(Random.Range(0.0f, 1.0f) < 0.5f) {
                    Child1.biases[x] = population[AIndex].biases[x];
                    Child2.biases[x] = population[BIndex].biases[x];
                } else {
                    Child2.biases[x] = population[AIndex].biases[x];
                    Child1.biases[x] = population[BIndex].biases[x];
                }
            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;
        }
    }

    private NNet[] PickBestPopulation() {
        NNet[] newPopulation = new NNet[InitialPopulation];
        for (int i = 0; i < BestAgentSelection; i++) {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(Controller.Layers, Controller.Nodes);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++) {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < WorstAgentSelection; i++) {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++) {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }

    private void SortPopulation() {
        for(int i = 0; i < population.Length; i++) {
            for(int j = i; j < population.Length; j++) {
                if (population[i].fitness < population[j].fitness) {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }
}
