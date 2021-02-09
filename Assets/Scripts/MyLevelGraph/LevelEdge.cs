﻿using System;
using UnityEngine;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class LevelEdge : MonoBehaviour
    {
        public Material attainableMaterial, defaultMaterial;

        [NonSerialized] public Field startField, connectedField;
        [NonSerialized] public int edgeWeight;
        private bool attainable;

        public bool Attainable 
        {
            get => attainable;
            set
            {
                attainable = value;
                GetComponentInChildren<MeshRenderer>().material = value ? attainableMaterial : defaultMaterial;
            }
        }
 
        public void Initialize(LevelGraph level, Field startField, Field connectedField, int weight)
        {
            this.startField = startField;
            this.connectedField = connectedField;
            edgeWeight = weight;

            Vector3 scale = transform.localScale;
            scale.z *= (connectedField.transform.position - startField.transform.position).magnitude;
            transform.localScale = scale;

            transform.rotation = Quaternion.LookRotation(connectedField.transform.position - startField.transform.position);
            var radians = transform.rotation.eulerAngles.y * Mathf.PI / 180;

            var offsetX = new Vector3(scale.z / 2 * Mathf.Sin(radians), 0, 0);
            var offsetZ = new Vector3(0, 0, scale.z / 2 * Mathf.Cos(radians));
            transform.position = startField.transform.position + offsetX + offsetZ;

            transform.parent = level.transform;
        }

        public override string ToString()
        {
            return $"Edge from {startField} to {connectedField}";
        }
    }
 
}
