using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class NNet : MonoBehaviour {
    //Create matrix with 1 layer and 6 empty nodes
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 15);

    //Create list with matrixes
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    //Create another matrix with 1 layer and 5 empty nodes for Output values
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 5);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();
    public float fitness;

    //Initialise HiddenLayers
    public void Initialise(int hLayerCount, int hNodeCount) {
        //Clear lists if they exist
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i <= hLayerCount; i++) {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hNodeCount);

            hiddenLayers.Add(f);
            biases.Add(Random.Range(-1f,1f));

            if (i == 0) {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(15, hNodeCount);
                weights.Add(inputToH1);
            }
            Matrix<float> HTH = Matrix<float>.Build.Dense(hNodeCount, hNodeCount);
            weights.Add(HTH);
        }
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hNodeCount, 5);
        weights.Add(OutputWeight);
        biases.Add(Random.Range(-1f, 1f));

        RandomWeights();
    }

    private void RandomWeights() {
        for (int i = 0; i < weights.Count; i++) {
            for (int j = 0; j < weights[i].RowCount; j++) {
                for (int x = 0; x < weights[i].ColumnCount; x++) {
                    weights[i][j, x] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float, float, float, float) RunNetwork(float a, float b, float c, float d, float e, float f, float g, float h, float i, float j, float k, float l, float m, float n, float p) {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = d;
        inputLayer[0, 4] = e;
        inputLayer[0, 5] = f;
        inputLayer[0, 6] = g;
        inputLayer[0, 7] = h;
        inputLayer[0, 8] = i;
        inputLayer[0, 9] = j;
        inputLayer[0, 10] = k;
        inputLayer[0, 11] = l;
        inputLayer[0, 12] = m;
        inputLayer[0, 13] = n;
        inputLayer[0, 14] = p;

        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int x = 1; x < hiddenLayers.Count; x++) {
            hiddenLayers[x] = ((hiddenLayers[x - 1] * weights[x]) + biases[x]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        //Output order =  Torso, LLeg, LFoot, RLeg, RFoot;
        return (Sigmoid(outputLayer[0,0]),
            (float)Math.Tanh(outputLayer[0, 1]),
            (float)Math.Tanh(outputLayer[0, 2]),
            (float)Math.Tanh(outputLayer[0, 3]),
            (float)Math.Tanh(outputLayer[0, 4]));
    }

    public NNet InitialiseCopy(int hiddenLayerCount, int hiddenNodeCount) {
        NNet n = new NNet();

        List<Matrix<float>> newWeights = new List<Matrix<float>>();
        
        for (int i = 0; i < weights.Count; i++) {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for(int x = 0; x < currentWeight.RowCount; x++) {
                for (int y = 0; y < currentWeight.ColumnCount; y++) {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }
            newWeights.Add(currentWeight);
        }

        List<float> newBiases = new List<float>();

        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitialiseHidden(hiddenLayerCount, hiddenNodeCount);

        return n;
    }

    private void InitialiseHidden(int hiddenLayerCount, int hiddenNodeCount) {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for(int i = 0; i < hiddenLayerCount + 1; i++) {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNodeCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }
    private float Sigmoid(float s) {
        return (1 / (1 + Mathf.Exp(-s)));
    }
}
