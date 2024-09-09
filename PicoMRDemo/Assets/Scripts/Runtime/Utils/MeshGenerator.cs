/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PicoMRDemo.Runtime.Utils
{
    public static class MeshGenerator
    {
        public static GameObject GenerateSkirtingLine(Vector3 center, Vector2 extent, Material material,
            float standardWidth = 10f, float standardHeight = 10f, float fixedWidth = 0.01f, float fixedHeight = 0.08f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentY = extent.y * 0.5f;
            var realCenter = center - new Vector3(0, halfExtentY, 0) + new Vector3(0, fixedHeight * 0.5f, fixedWidth * 0.5f);
            var realHalfExtentX = extent.x * 0.5f;
            var realHalfExtentY = fixedHeight * 0.5f;
            var realHalfExtentZ = fixedWidth * 0.5f;

            var bottomY = realCenter.y - realHalfExtentY;
            var topY = realCenter.y + realHalfExtentY;
            var leftX = realCenter.x - realHalfExtentX;
            var rightX = realCenter.x + realHalfExtentX;
            var farZ = realCenter.z - realHalfExtentZ;
            var nearZ = realCenter.z + realHalfExtentZ;

            var node1 = new Vector3(rightX, topY, nearZ);
            var node2 = new Vector3(rightX, topY, farZ);
            var node3 = new Vector3(leftX, topY, farZ);
            var node4 = new Vector3(leftX, topY, nearZ);
            var node5 = new Vector3(leftX, topY, nearZ);
            var node6 = new Vector3(leftX, bottomY, nearZ);
            var node7 = new Vector3(rightX, bottomY, nearZ);
            var node8 = new Vector3(rightX, topY, nearZ);
            
            points.Add(node1);
            points.Add(node2);
            points.Add(node3);
            points.Add(node4);
            points.Add(node5);
            points.Add(node6);
            points.Add(node7);
            points.Add(node8);
            
            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = fixedHeight / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(w, 0);
            uvs[1] = new Vector2(w, h);
            uvs[2] = new Vector2(0, h);
            uvs[3] = new Vector2(0, 0);
            uvs[4] = new Vector2(0, h);
            uvs[5] = new Vector2(0, 0);
            uvs[6] = new Vector2(w, 0);
            uvs[7] = new Vector2(w, h);
            
            var meshObject = new GameObject("SkirtingLine");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = new[] { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4 };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;

        }
        public static GameObject GenerateQuadMesh(Vector3 center, Vector2 extent, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentX = extent.x * 0.5f;
            var halfExtentY = extent.y * 0.5f;
            var expand = 0.01f;
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y - halfExtentY - expand, center.z));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y - halfExtentY - expand, center.z));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y + halfExtentY + expand, center.z));
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y + halfExtentY + expand, center.z));
            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = extent.y / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(w, 0);
            uvs[2] = new Vector2(w, h);
            uvs[3] = new Vector2(0, h);
            
            var meshObject = new GameObject("QuadMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = tr.Triangulate();

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
        public static GameObject GeneratePolygonMesh(IList<Vector3> points, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            var meshObject = new GameObject("GenerateMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            var pArray = points.ToArray();
            var minX = points[0].x;
            var maxX = points[0].x;
            var minY = points[0].y;
            var maxY = points[0].y;
            foreach (var point in points)
            {
                if (point.x < minX)
                    minX = point.x;
                if (maxX < point.x)
                    maxX = point.x;
                if (point.y < minY)
                    minY = point.y;
                if (maxY < point.y)
                    maxY = point.y;
            }
            var u = (maxX - minX) / standardWidth;
            var v = (maxY - minY) / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2((pArray[i].x - minX)/ (maxX - minX) * u, (pArray[i].y - minY) / (maxY - minY) * v);
            }
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = tr.Triangulate();

            mesh.triangles = triangles;
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
        public static GameObject GeneratePlane(Vector3 center, Vector2 extent, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentX = extent.x * 0.5f;
            var halfExtentY = extent.y * 0.5f;
            var expand = 0.00f;
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y, center.z - halfExtentY - expand));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y, center.z - halfExtentY - expand));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y, center.z + halfExtentY + expand));
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y, center.z + halfExtentY + expand));
            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = extent.y / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(w, 0);
            uvs[2] = new Vector2(w, h);
            uvs[3] = new Vector2(0, h);
            
            var meshObject = new GameObject("PlaneMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            // var tr = new Triangulator(pArray);
            int[] triangles = new int[] { 0, 3, 2, 2, 1, 0 };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
    }
    
    internal class Triangulator
    {
        private List<Vector3> m_points = new List<Vector3>();
     
        public Triangulator (Vector3[] points) {
            m_points = new List<Vector3>(points);
        }
     
        public int[] Triangulate() {
            List<int> indices = new List<int>();
     
            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();
     
            int[] V = new int[n];
            if (Area() > 0) {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }
     
            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2; ) {
                if ((count--) <= 0)
                    return indices.ToArray();
     
                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;
     
                if (Snip(u, v, w, nv, V)) {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }
     
            // indices.Reverse();
            return indices.ToArray();
        }
     
        private float Area () {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++) {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }
     
        private bool Snip (int u, int v, int w, int n, int[] V) {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++) {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }
     
        private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;
     
            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;
     
            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;
     
            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}