using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace MeshManager
{
    public class VertexData
    {
        //The distance to water
        public float distance;
        //We also need to store a index so we can form clockwise triangles
        public int index;
        //The global Vector3 position of the vertex
        public Vector3 globalVertexPos;
    }
    public struct TriangleData
    {
        //The corners of this triangle in global coordinates
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        //The center of the triangle
        public Vector3 center;

        //The distance to the surface from the center of the triangle
        public float distanceToSurface;

        //The normal to the triangle
        public Vector3 normal;

        
        public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3, Rigidbody boatRB)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            //Center of the triangle
            this.center = (p1 + p2 + p3) / 3f;

            //Distance to the surface from the center of the triangle
            this.distanceToSurface = Mathf.Abs(WaterManagerObject.current.DistanceToWater(this.center));

            //Normal to the triangle
            this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        }

    }

    public struct BuoyancyData
    {
        public Vector3 b_center;
        public float v_submerged;
        
        public BuoyancyData(Vector3 b_c, float v_d){
            b_center = b_c;
            v_submerged = v_d;
        }
    }
}