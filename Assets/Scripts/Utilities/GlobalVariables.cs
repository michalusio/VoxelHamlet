using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public interface IVariable
    {
        string AsString();
        int AsInt();
    }

    [CreateAssetMenu(fileName = "GlobalVariablesDB", menuName = "GlobalVariables", order = 1)]
    public class GlobalVariables : ScriptableObject
    {
        [SerializeField] private Variable[] variables;

        private Dictionary<string, Variable> variableDictionary;

        public IVariable this[string key]
        {
            get {
                variableDictionary ??= variables.ToDictionary(v => v.name);
                return variableDictionary[key];
            }
        }

        [Serializable]
        private class Variable: IVariable
        {
            [SerializeField] public string name;
            [SerializeField] public string value;

            public string AsString()
            {
                return value;
            }

            public int AsInt()
            {
                return int.Parse(value);
            }
        }
    }
}
