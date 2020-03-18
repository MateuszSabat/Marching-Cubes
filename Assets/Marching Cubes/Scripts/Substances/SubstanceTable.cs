using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MarchingCubes
{
    public class SubstanceTable
    {
        public static int max = 64;
        public static string dataPath = Application.dataPath + "/Marching Cubes/Data/substances.dat";

        public static List<Substance> substances;
        private static List<Substance> previousSubstances;

        public static void Add(Substance s)
        {
            if(substances.Count >= max)
            {
                Debug.LogWarning("There cannot be more than " + max.ToString() + " substances in the list");
                return;
            }
            else
            {
                foreach (Substance substance in substances)
                {
                    if (substance.name == s.name)
                    {
                        Debug.LogWarning("Substance of name " + s.name + " already exists in substance list");
                        return;
                    }
                    if(substance.color == s.color)
                    {
                        Debug.LogWarning("Substance of that color already exist in substance list");
                        return;
                    }
                }
                substances.Add(s);
            }
        }
        public static void Remove(Substance s)
        {
            substances.Remove(s);
        }

        public static void RemoveDuplicate()
        {
            substances = substances.Distinct().ToList();
        }

        public static void Save()
        {
            FileStream fs = new FileStream(dataPath, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, substances);
            fs.Close();
        }

        public static void Load()
        {
            using (Stream stream = File.Open(dataPath, FileMode.Open))
            {
                if (stream.Length > 0)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    substances = (List<Substance>)bf.Deserialize(stream);
                }
                else
                {
                    Debug.Log("Substance list is empty");
                    Debug.Log("Creating new list");
                    substances = new List<Substance>
                    {
                        Substance.air
                    };
                }
            }
        }

        public static void Init()
        {
            Load();
            previousSubstances = new List<Substance>(substances);
        }

        public static void Reset()
        {
            substances = previousSubstances;
        }
    }
    [System.Serializable]
    public struct Substance
    {
        public string name;
        public float r, g, b;

        public Color color
        {
            get
            {
                return new Color(r, g, b);
            }
        }

        public static Substance air
        {
            get
            {
                return new Substance("air", new Color(0, 1, 0));
            }
        }

        public Substance(string _name = "new substance")
        {
            name = _name;
            r = 0.8f;
            g = 0.8f;
            b = 0.8f;
        }
        public Substance(string _name, Color _color)
        {
            name = _name;
            r = _color.r;
            g = _color.g;
            b = _color.b;
        }

        public Substance(Substance s)
        {
            name = s.name;
            r = s.r;
            g = s.g;
            b = s.b;
        }
    }
}