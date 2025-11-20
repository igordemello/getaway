using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterableGlass : MonoBehaviour
{
    public int Sectors = 3;
    public int DetailsPerSector = 3;
    public float SimplifyThreshold = 0.05f;
    public bool GlassSides = true;
    public Material GlassSidesMaterial;
    public float GlassThickness = 0.01f;
    public bool ShatterButNotBreak = false;
    public bool SlightlyRotateGibs = true;
    public bool DestroyGibs = true;
    public float AfterSeconds = 5f;
    public bool GibsOnSeparateLayer = false;
    public int GibsLayer = 0;
    public float Force = 100f;
    public bool AdoptFragments = false;
    Vector2[] Bounds = new Vector2[4];
    float Area = 1f;
    Material GlassMaterial;
    AudioSource SoundEmitter;

    void Start()
    {
        float u = Mathf.Abs(transform.lossyScale.x / 2f);
        float v = Mathf.Abs(transform.lossyScale.y / 2f);

        Area = u * v;

        Bounds[0] = new Vector2(u, v);
        Bounds[1] = new Vector2(-u, v);
        Bounds[2] = new Vector2(-u, -v);
        Bounds[3] = new Vector2(u, -v);

        SoundEmitter = GetComponent<AudioSource>();

        if (GetComponent<Renderer>() == null || GetComponent<MeshFilter>() == null)
        {
            Debug.LogError(gameObject.name + ": No Renderer and/or MeshFilter components!");
            Destroy(gameObject);
            return;
        }

        GlassMaterial = GetComponent<Renderer>().material;

        if (GlassSides && GlassSidesMaterial == null)
        {
            Debug.LogError(gameObject.name + ": GlassSide material must be assigned! Glass will be destroyed.");
            Destroy(gameObject);
        }
    }

    public void Shatter2D(Vector2 HitPoint)
    {
        Shatter(HitPoint, transform.forward);
    }

    public void Shatter3D(ShatterableGlassInfo Inf)
    {

        Transform Parent = gameObject.transform.parent;

        bool Sucsess = true;

        while (Parent != null)
        {
            if (Parent.localScale.x != 1f || Parent.localScale.y != 1f || Parent.localScale.y != 1f)
                Sucsess = false;
            Parent = Parent.parent;
        }

        if (!Sucsess)
            Debug.LogWarning(gameObject.name + ": scale of all parents in hierarchy recommended to be {1, 1, 1}. Glass may shatter weirdly.");

        Vector3 A = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 B = transform.TransformPoint(new Vector3(0.5f, -0.5f));

        float b = Vector3.Distance(Inf.HitPoint, A);
        float c = Vector3.Distance(B, A);
        float a = Vector3.Distance(Inf.HitPoint, B);

        float p = (a + b + c) / 2f;

        float S = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));

        float h = 2 / c * S;

        float u = Mathf.Sqrt(b * b - h * h);

        h -= Mathf.Abs(transform.lossyScale.y / 2f);
        u -= Mathf.Abs(transform.lossyScale.x / 2f);

        Shatter(new Vector2(u * Mathf.Sign(transform.lossyScale.x), h * Mathf.Sign(transform.lossyScale.y)), Inf.HitDirrection);
    }

    public void Shatter(Vector2 HitPoint, Vector3 ForceDirrection)
    {
        int BaseLinesCount = 4 + (Sectors - 1) * 4;
        BaseLine[] BaseLines = new BaseLine[BaseLinesCount];

        for (int j = 0; j < 4; j++)
        {
            BaseLines[j * Sectors] = new BaseLine(HitPoint, Bounds[j], DetailsPerSector);

            float Margin = 1f / Sectors;
            float Ratio = Margin;

            for (int i = 1; i < Sectors; i++)
            {
                BaseLines[j * Sectors + i] = new BaseLine(HitPoint, Vector2.Lerp(Bounds[j], Bounds[(j + 1) % 4], Ratio), DetailsPerSector);
                Ratio += Margin;
            }
        }

        List<Figure> Figures = new List<Figure>();

        for (int i = 0; i < BaseLinesCount; i++)
        {
            int k = (i + 1) % BaseLinesCount;

            float a = Vector2.Distance(HitPoint, BaseLines[i].Points[DetailsPerSector]);
            float b = Vector2.Distance(HitPoint, BaseLines[k].Points[DetailsPerSector]);
            float c = Vector2.Distance(BaseLines[i].Points[DetailsPerSector], BaseLines[k].Points[DetailsPerSector]);

            float p = (a + b + c) * 0.5f;

            float S = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));

            if (S < Area * SimplifyThreshold)
                Figures.Add(new Figure(new Vector2[] { BaseLines[i].Points[DetailsPerSector], BaseLines[k].Points[DetailsPerSector], HitPoint }, DetailsPerSector / 2));
            else
            {
                Figures.Add(new Figure(new Vector2[] { BaseLines[i].Points[1], BaseLines[k].Points[1], HitPoint }, 1));

                for (int j = 1; j < DetailsPerSector; j++)
                {
                    Vector2[] Points = new Vector2[4];

                    Points[0] = BaseLines[i].Points[j];
                    Points[1] = BaseLines[(i + 1) % BaseLinesCount].Points[j];
                    Points[2] = BaseLines[i].Points[j + 1];
                    Points[3] = BaseLines[(i + 1) % BaseLinesCount].Points[j + 1];

                    Figures.Add(new Figure(Points, i + 1));
                }
            }
        }
        
        foreach (Figure Fig in Figures)
        {
            GameObject Obj = new GameObject("GlassGib");
            Obj.transform.rotation = transform.rotation;
            Obj.transform.position = transform.position;
            if (AdoptFragments)
                Obj.transform.parent = transform.parent;

            MeshFilter Filter = Obj.AddComponent<MeshFilter>();

            MeshRenderer Rnd = Obj.AddComponent<MeshRenderer>();

            if (GlassSides)
                Rnd.materials = new Material[2] { GlassMaterial, GlassSidesMaterial };
            else
                Rnd.material = GlassMaterial;

            Mesh Model = Fig.GenerateMesh(GlassSides, GlassThickness / 2f, new Vector2(transform.lossyScale.x, transform.lossyScale.y));
            Filter.sharedMesh = Model;

            if (!ShatterButNotBreak)
            {
                Fig.GenerateCollider(GlassThickness, Obj);

                Rigidbody Rig = Obj.AddComponent<Rigidbody>();

                Rig.AddForce(ForceDirrection * Random.Range(Force, Force * 1.5f) / Fig.ForceScale);

                if (GibsOnSeparateLayer)
                    Obj.layer = GibsLayer;

                if (DestroyGibs)
                {
                    float AfterSecondsMargin = AfterSeconds * 0.1f;
                    Destroy(Obj, Random.Range(AfterSeconds - AfterSecondsMargin, AfterSeconds + AfterSecondsMargin));
                }
            }
            else if (SlightlyRotateGibs)
                Obj.transform.Rotate(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));

        }

        if (SoundEmitter)
            SoundEmitter.Play();

        Destroy(GetComponent<Renderer>());
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<ShatterableGlass>());

        if (ShatterButNotBreak)
            gameObject.tag = "Untagged";
        else
        {
            Destroy(GetComponent<MeshCollider>());
            if (SoundEmitter)
            {
                if (SoundEmitter.clip)
                    Destroy(gameObject, SoundEmitter.clip.length);
                else
                    Debug.Log(gameObject.name + ": AudioSource component is present, but SoundClip is not set.");
            }
            else
                Destroy(gameObject);
        }
    }

    class Figure
    {
        public Vector2[] Points;
        public int ForceScale;

        public Figure(Vector2[] Points, int ForceScale)
        {
            this.Points = Points;
            this.ForceScale = ForceScale;
        }

        public void GenerateCollider(float GlassThickness, GameObject Obj)
        {
            BoxCollider Col = Obj.AddComponent<BoxCollider>();

            float a = Vector2.Distance(Points[2], Points[0]);
            float b = Vector2.Distance(Points[2], Points[1]);
            float c = Vector2.Distance(Points[1], Points[0]);

            float p = a + b + c;

            float ox = (a * Points[0].x + b * Points[1].x + c * Points[2].x) / p;
            float oy = (a * Points[0].y + b * Points[1].y + c * Points[2].y) / p;

            p /= 2f;

            float r = Mathf.Sqrt(((p - a) * (p - b) * (p - c)) / p);

            r *= Mathf.Sqrt(2);
            Col.center = new Vector3(ox, oy, 0f);
            Col.size = new Vector3(r, r, GlassThickness);
        }

        public Mesh GenerateMesh(bool GenerateGlassSides, float GlassHalfThickness, Vector2 UVScale)
        {
            Mesh Model = new Mesh();

            Model.name = "GlassGib";

            if (GenerateGlassSides)
                Model.subMeshCount = 2;

            bool IsTriangle = Points.Length == 3;

            Vector3[] Vertices = new Vector3[IsTriangle ? GenerateGlassSides ? 9 : 3 : GenerateGlassSides ? 12 : 4];
            Vector2[] Map = new Vector2[Vertices.Length];

            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = Points[i];
                Map[i] = new Vector2(Points[i].x / UVScale.x, Points[i].y / UVScale.y) + new Vector2(0.5f, 0.5f);
            }

            int[] MainTriangles;

            if (IsTriangle)
                MainTriangles = new int[3] { 2, 1, 0 };          
            else
                MainTriangles = new int[6] { 0, 1, 2, 3, 2, 1 };

            if (GenerateGlassSides)
            {
                int[] TrianglesSide;

                if (IsTriangle)
                {
                    for (int i = 0; i < 3; i++)
                        GlassSideVertex(Points[i], ref Vertices[i * 2 + 3], ref Vertices[i * 2 + 4], GlassHalfThickness);

                    TrianglesSide = new int[18] { 3, 4, 5, 4, 6, 5, 3, 4, 7, 7, 8, 4, 5, 6, 8, 8, 7, 5 };
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        GlassSideVertex(Points[i], ref Vertices[i * 2 + 4], ref Vertices[i * 2 + 5], GlassHalfThickness);

                    TrianglesSide = new int[24] { 7, 5, 4, 6, 7, 4, 11, 7, 6, 10, 11, 6, 10, 11, 9, 9, 8, 10, 8, 9, 5, 8, 4, 5 };

                }
                Model.vertices = Vertices;
                Model.SetTriangles(MainTriangles, 0);
                Model.SetTriangles(TrianglesSide, 1);
            }
            else
            {
                Model.vertices = Vertices;
                Model.triangles = MainTriangles;
            }

            Model.uv = Map;

            return Model;
        }

        void GlassSideVertex(Vector2 Ref, ref Vector3 A, ref Vector3 B, float GlassHalfThickness)
        {
            A = new Vector3(Ref.x, Ref.y, GlassHalfThickness);
            B = new Vector3(Ref.x, Ref.y, -GlassHalfThickness);
        }
    }

    class BaseLine
    {
        public Vector2[] Points;

        public BaseLine(Vector2 HitPoint, Vector2 End, int Count)
        {
            Points = new Vector2[Count + 1];
            Points[0] = HitPoint;
            Points[Count] = End;

            float Margin = 1f / Count;
            float Ratio = Margin;


            float Angle = Mathf.Atan2(Mathf.Max(HitPoint.y, End.y) - Mathf.Min(HitPoint.y, End.y), Mathf.Max(End.x, HitPoint.x) - Mathf.Min(HitPoint.x, End.x));

            float Pi4 = Mathf.PI / 4f;
            float Pi2 = Mathf.PI / 2f;

            if (Angle > Pi4)
            {
                Angle = Pi2 - Angle;
            }
            float Roll = Angle / Pi4;

            for (int i = 0; i < Count - 1; i++)
            {
                Points[i + 1] = Vector2.Lerp(HitPoint, End, Ratio * Mathf.Lerp(1f, Mathf.Sqrt(2) / 2f, Roll));
                Ratio += Margin;
            }
        }
    }
}

public class ShatterableGlassInfo
{
    public Vector3 HitPoint;
    public Vector3 HitDirrection;

    public ShatterableGlassInfo(Vector3 HitPoint, Vector3 HitDirrection)
    {
        this.HitPoint = HitPoint;
        this.HitDirrection = HitDirrection;
    }
}