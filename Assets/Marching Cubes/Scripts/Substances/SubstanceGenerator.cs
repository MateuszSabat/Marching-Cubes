using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    [CreateAssetMenu(fileName = "Terrain Substance Generator", menuName = "Marching Cubes/Substance Generators/Terrain")]
    public class SubstanceGenerator : ScriptableObject
    {
        public enum SubstanceState { Zero, Visible, Invisible}

        public SubstanceState[] state;
        public Curve[] curves;

        public bool heightCurveVisible;
        public Curve heightCurve;

        public void Init()
        {
            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            state = new SubstanceState[SubstanceTable.substances.Count];
            curves = new Curve[SubstanceTable.substances.Count];
            for (int i = 0; i < curves.Length; i++)
            {
                curves[i] = Curve.line;
                curves[i].color = SubstanceTable.substances[i].color;
            }
            heightCurve = Curve.line;

        }

        public void ResetCurve(int i)
        {
            curves[i] = Curve.zero;
            curves[i].color = SubstanceTable.substances[i].color;
            curves[i].Recalculate();
        }

        public void ResetAll()
        {
            for (int i = 0; i < state.Length; i++)
                ResetCurve(i);

            heightCurve = Curve.line;
            heightCurve.Recalculate();
        }

        public class Curve
        {
            public class Point
            {
                public Vector2 pos;
                public Vector2 dir;

                public Point(Vector2 _pos, Vector2 _dir)
                {
                    pos = _pos;
                    if (_dir.x < 0)
                        _dir = -_dir;
                    dir = _dir;
                }
            }

            public List<Point> points;
            public float[] y;
            public Color color;
            public List<Vector2> p;

            public Curve()
            {
                points = new List<Point>();
                y = new float[101];
                color = Color.black;
            }


            public void Add(Point p)
            {
                points.Add(p);
                /*for(int i=0; i<points.Count; i++)
                    if(points[i].pos.x > p.pos.x)
                    {
                        for(int j=points.Count - 1; j>i; j--)
                        {
                            points[j] = points[j - 1];
                        }
                        points[i] = p;
                        return;
                    }*/
            }
            public void Add(Point p, int index)
            {
                points.Add(p);
                for (int i = points.Count - 1; i > index; i--)
                    points[i] = points[i - 1];
                points[index] = p;

            }
            public void Remove(int i)
            {
                points.RemoveAt(i);
            }

            public static Vector2 TimeEvaluate(Point a, Point b, float t)
            {
                //return (1 - t) * (1 - t) *(a.pos +  t * a.dir) + t * t * (b.pos - b.dir + b.dir * t);
                return (1 - t) * (1 - t) * (1 - t) * a.pos + 3 * (1 - t) * (1 - t) * t * (a.pos + a.dir) + 3 * (1 - t) * t * t * (b.pos - b.dir) + t * t * t * b.pos;
            }

            public void Recalculate()
            {
                p = new List<Vector2>();
                y = new float[101];
                if (points[0].pos.x > 0)
                    p.Add(new Vector2(0, points[0].pos.y));
                p.Add(points[0].pos);
                for(int i=1; i<points.Count; i++)
                {
                    for(float t = 0; t<1; t += 0.05f)
                    {
                        p.Add(TimeEvaluate(points[i - 1], points[i], t));
                    }
                }
                p.Add(points[points.Count - 1].pos);
                if (points[points.Count - 1].pos.x < 1)
                    p.Add(new Vector2(1, points[points.Count - 1].pos.y));

                int x = 0;
                int j = 0;
                while (j < 100 && x < p.Count)
                {
                    while(j * 0.01f < p[x].x)
                    {
                        y[j] = Mathf.Lerp(p[x - 1].y, p[x].y, (j * 0.01f - p[x - 1].x) / (p[x].x - p[x - 1].x));
                        j++;
                    }
                    x++;
                }
                y[100] = p[p.Count - 1].y;
            }

            public static Curve line
            {
                get
                {
                    Curve l = new Curve();

                    l.Add(new Point(new Vector2(0, 0), new Vector2(0.4f, 0f)));
                    l.Add(new Point(new Vector2(0.5f, 0.5f), new Vector2(0.0f, 0.1f)));
                    //l.Add(new Point(new Vector2(0.75f, 0.6f), new Vector2(0.1f, 0.0f)));
                    l.Add(new Point(new Vector2(1, 1), new Vector2(0.4f, 0f)));

                    l.Recalculate();

                    return l;
                }
            }
            public static Curve zero
            {
                get
                {
                    Curve l = new Curve();

                    l.Add(new Point(new Vector2(0, 0), new Vector2(0.1f, 0)));
                    l.Add(new Point(new Vector2(1, 0), new Vector2(0.1f, 0)));

                    l.Recalculate();

                    return l;
                }
            }
            public static Curve one
            {
                get
                {
                    Curve l = new Curve();

                    l.Add(new Point(new Vector2(0, 1), new Vector2(0.1f, 0)));
                    l.Add(new Point(new Vector2(1, 1), new Vector2(0.1f, 0)));

                    l.Recalculate();

                    return l;
                }
            }



            public static float InverseF(Point a, Point b, float x)
            {
                /*
                 *  Bezier curve is definied as
                 * 
                 *      F(t) = (1-t)^3 a.pos + t(1-t)^2 (a.pos + a.dir) + t^2(1-t) (b.pos - b.dir) + t^3 b.pos
                 *                       ||                    ||                         ||              || 
                 *                       x1                    x2                         x3              x4
                 *  
                 *  knowing x parameter of F(t) we want to count t that is given by the equation:
                 *  
                 *      (-x1 + x2 - x3 + x4)t^3 + (3x1 - 2x2 + x3)t^2 + (-3x1 + x2)t + x1 - x = 0;
                 * 
                 *  what we do with Cardano algorithm                 * 
                 */

                float A = a.dir.x + b.dir.x;
                float B = a.pos.x - 2 * a.dir.x + b.pos.x - b.dir.x;
                float C = a.dir.x - 2 * a.pos.x;
                float D = a.pos.x - x;

                if (A != 0)
                {
                    B /= A;
                    C /= A;
                    D /= A;

                    float p = C - B * B * 0.3333f;
                    float q = 0.074074f * B * B * B - B * C * 0.3333f + D;

                    float delta = p * p * p * 0.037037f + q * q * 0.25f;

                    if(delta > 0)
                    {
                        delta = Mathf.Sqrt(delta);
                        return Mathf.Pow(-0.5f * q + delta, 0.33333f) + Mathf.Pow(-0.5f * q - delta, 0.33333f) - B * 0.33333f;
                    }
                }
                return 0;
            }

        }
    }
}

