using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshManager
{
    //Generates the mesh that's below the water
    public class MeshManagerUtils 
    {
        //The boat transform needed to get the global position of a vertice
        private Transform boatTrans;
        //Coordinates of all vertices in the original boat
        Vector3[] boatVertices;
        //Positions in allVerticesArray, such as 0, 3, 5, to build triangles
        int[] boatTriangles;
        //The boats rigidbody
        private Rigidbody boatRB;

        //So we only need to make the transformation from local to global once
        public Vector3[] boatVerticesGlobal;
        //Find all the distances to water once because some triangles share vertices, so reuse
        float[] allDistancesToWater;

        //The part of the boat that's under water - needed for calculations of length / volume
        private Mesh underWaterMesh;
        public List<TriangleData> underWaterTriangleData = new List<TriangleData>();
        public List<Vector3> underWaterVertices = new List<Vector3>();
        private List<Vector3> nearUnderWaterVertices = new List<Vector3>();
        private float nearEpsilon = 0.01f; //1e-6

        

        //To approximate the underwater volume/length we need a mesh collider
        private MeshCollider underWaterMeshCollider;

         //To connect the submerged triangles with the original triangles
        public List<int> indexOfOriginalTriangle = new List<int>();
        //The total area of the entire boat
        public float boatArea;

        public Vector3 auxUnderWaterCenter;
        public float underWaterVolume;
        public Vector3 centerOfBuoyancy;
        float timeSinceStart;

        public MeshManagerUtils(GameObject boatObj, GameObject underWaterObj, Rigidbody boatRB) 
        {
            //Get the transform
            boatTrans = boatObj.transform;

            //Get the rigid body
            this.boatRB = boatRB;

            //Get the meshcollider
            underWaterMeshCollider = underWaterObj.GetComponent<MeshCollider>();

            //Save the mesh
            underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;

            //Init the arrays and lists
            boatVertices = boatObj.GetComponent<MeshFilter>().mesh.vertices;
            boatTriangles = boatObj.GetComponent<MeshFilter>().mesh.triangles;

            //The boat vertices in global position
            boatVerticesGlobal = new Vector3[boatVertices.Length];
            //Find all the distances to water once because some triangles share vertices, so reuse
            allDistancesToWater = new float[boatVertices.Length];

        }

        //Generate the underwater mesh 
        public void GenerateUnderwaterMesh()
        {
            //Reset
            underWaterTriangleData.Clear();
            underWaterVertices.Clear();
            nearUnderWaterVertices.Clear();

            indexOfOriginalTriangle.Clear();

            //Make sure we find the distance to water with the same time
            timeSinceStart = Time.time;


            //Find all the distances to water once because some triangles share vertices, so reuse
            for (int j = 0; j < boatVertices.Length; j++)
            {
                //The coordinate should be in global position
                Vector3 globalPos = boatTrans.TransformPoint(boatVertices[j]);

                //Save the global position so we only need to calculate it once here
                //And if we want to debug we can convert it back to local
                boatVerticesGlobal[j] = globalPos;

                allDistancesToWater[j] = WaterManagerObject.current.DistanceToWater(globalPos);
                if (allDistancesToWater[j] < 0f){
                    underWaterVertices.Add(new Vector3(globalPos.x, globalPos.y, globalPos.z));
                    if (Mathf.Abs(allDistancesToWater[j]) < nearEpsilon){ // those points that are close to the surface
                        nearUnderWaterVertices.Add(new Vector3(globalPos.x, globalPos.y, globalPos.z));
                    }
                }
            }

            //Add the triangles
            AddTriangles();
        }

        public Vector3 CreateAuxUnderWaterCenter(){
            Vector3 max_z = new Vector3(0f, 0f, 0f);
            Vector3 min_z = new Vector3(0f, 0f, 0f);
            
            Vector3 max_x = new Vector3(0f, 0f, 0f);
            Vector3 min_x = new Vector3(0f, 0f, 0f);
            
            float x_pos = 0;
            float y_pos = 0;
            float z_pos = 0;

            int tmp_max_z = 1;
            int tmp_min_z = 1;
            int tmp_max_x = 1;
            int tmp_min_x = 1;

            if (nearUnderWaterVertices.Count > 1){
                for (int i = 1; i < nearUnderWaterVertices.Count; i++){
                    // search for the max z points
                    if (nearUnderWaterVertices[i].z > nearUnderWaterVertices[tmp_max_z].z){
                        tmp_max_z = i;
                    }
                    // search for the min z points
                    if (nearUnderWaterVertices[i].z < nearUnderWaterVertices[tmp_min_z].z){
                        tmp_min_z = i;
                    }
                    // search for the max x points
                    if (nearUnderWaterVertices[i].x > nearUnderWaterVertices[tmp_max_x].x){
                        tmp_max_x = i;
                    }
                    // search for the min x points
                    if (nearUnderWaterVertices[i].x < nearUnderWaterVertices[tmp_min_x].x){
                        tmp_min_x = i;
                    }
                    continue;
                }

                x_pos = (nearUnderWaterVertices[tmp_max_x].x + nearUnderWaterVertices[tmp_min_x].x) * 0.5f;
                z_pos = (nearUnderWaterVertices[tmp_max_z].z + nearUnderWaterVertices[tmp_min_z].z) * 0.5f;
                y_pos = (nearUnderWaterVertices[tmp_max_x].y + nearUnderWaterVertices[tmp_max_z].y + nearUnderWaterVertices[tmp_min_x].y + nearUnderWaterVertices[tmp_min_z].y) * 0.25f;
                //y_pos = WaterManagerObject.current.GetWaveYPos(new Vector3(x_pos, 0f, z_pos), timeSinceStart);
                Vector3 tmp_waterCenter = new Vector3(x_pos, y_pos, z_pos);
                tmp_waterCenter = CheckIsValid(tmp_waterCenter, "The water Center");
                return tmp_waterCenter;
            }
            else{
                return boatRB.centerOfMass;
            }    
        }

    
        public BuoyancyData retrieveBuoyancyDate(Vector3 aux, Vector3 aux_default){
        

                List<Vector3> c_tets = new List<Vector3>(); // list of tetrahedrons' centroid
                List<float> v_tets = new List<float>(); // list of tetrahedrons' volume

                Vector3 w_c_tets = new Vector3(); // the weighted centroid
                float s_v_tets = 0; // the sum of the volumes
                // Debug.Log("In the function");
                // Vector3 tmp_p1;
                // Vector3 tmp_p2;
                // Vector3 tmp_p3;

                for (int i = 0; i < underWaterTriangleData.Count; i++){
                    /// if all the vertices of the triangle are below the level of water 
                    /// then the volume and the centroid of the associeted tetrahedron will be calculeted
                
                    v_tets.Add(VolumeTetrahedron(   underWaterTriangleData[i].p1, 
                                                    underWaterTriangleData[i].p2, 
                                                    underWaterTriangleData[i].p3, 
                                                    aux));
                    c_tets.Add(CentroidTetrahedron( underWaterTriangleData[i].p1, 
                                                    underWaterTriangleData[i].p2, 
                                                    underWaterTriangleData[i].p3, 
                                                    aux));
                
                }
                for (int i = 0; i < v_tets.Count; i++){
                    w_c_tets += c_tets[i] * v_tets[i];
                    s_v_tets += v_tets[i];
                }

                BuoyancyData tmp_b_struct;
                if ( s_v_tets == 0){
                    tmp_b_struct.b_center = aux_default;
                    tmp_b_struct.v_submerged =  s_v_tets;
                }
                else{
                    tmp_b_struct.b_center = w_c_tets / s_v_tets;
                    tmp_b_struct.v_submerged = s_v_tets;
                }

                return tmp_b_struct;
            }

        

        //Add all the triangles that's part of the underwater meshes
        private void AddTriangles()
        {
            //List that will store the data we need to sort the vertices based on distance to water
            List<VertexData> vertexData = new List<VertexData>();

            //Add fake data that will be replaced
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());


            //Loop through all the triangles (3 vertices at a time = 1 triangle)
            int i = 0;
            int triangleCounter = 0;
            while (i < boatTriangles.Length)
            {
                //Loop through the 3 vertices
                for (int x = 0; x < 3; x++)
                {
                    //Save the data we need
                    vertexData[x].distance = allDistancesToWater[boatTriangles[i]];

                    vertexData[x].index = x;

                    vertexData[x].globalVertexPos = boatVerticesGlobal[boatTriangles[i]];

                    i++;
                }


                //Create the triangles that are below the waterline

                //All vertices are underwater
                if (vertexData[0].distance < 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    Vector3 p1 = vertexData[0].globalVertexPos;
                    Vector3 p2 = vertexData[1].globalVertexPos;
                    Vector3 p3 = vertexData[2].globalVertexPos;

                    //Save the triangle
                    underWaterTriangleData.Add(new TriangleData(p1, p2, p3, boatRB));

                    indexOfOriginalTriangle.Add(triangleCounter);
                }
                //1 or 2 vertices are below the water
                else
                {
                    //Sort the vertices
                    vertexData.Sort((x, y) => x.distance.CompareTo(y.distance));

                    vertexData.Reverse();

                    //One vertice is above the water, the rest is below
                    if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesOneAboveWater(vertexData, triangleCounter);
                    }
                    //Two vertices are above the water, the other is below
                    else if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesTwoAboveWater(vertexData, triangleCounter);
                    }
                }

                triangleCounter += 1;
            }
        }



        //Build the new triangles where one of the old vertices is above the water
        private void AddTrianglesOneAboveWater(List<VertexData> vertexData, int triangleCounter)
        {
            //H is always at position 0
            Vector3 H = vertexData[0].globalVertexPos;

            //Left of H is M
            //Right of H is L

            //Find the index of M
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }

            //We also need the heights to water
            float h_H = vertexData[0].distance;
            float h_M = 0f;
            float h_L = 0f;

            Vector3 M = Vector3.zero;
            Vector3 L = Vector3.zero;

            //This means M is at position 1 in the List
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].globalVertexPos;
                L = vertexData[2].globalVertexPos;

                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].globalVertexPos;
                L = vertexData[1].globalVertexPos;

                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }


            //Now we can calculate where we should cut the triangle to form 2 new triangles
            //because the resulting area will always form a square

            //Point I_M
            Vector3 MH = H - M;

            float t_M = -h_M / (h_H - h_M);

            Vector3 MI_M = t_M * MH;

            Vector3 I_M = MI_M + M;


            //Point I_L
            Vector3 LH = H - L;

            float t_L = -h_L / (h_H - h_L);

            Vector3 LI_L = t_L * LH;

            Vector3 I_L = LI_L + L;

            // Add to list udner water vertices
            // underWaterVertices.Add(I_L);
            // underWaterVertices.Add(I_M);

            Vector3 tmp_IL = I_L;//boatTrans.TransformPoint(boatVertices[j]);
            float tmp_dist_IL = WaterManagerObject.current.DistanceToWater(tmp_IL);
            if (Mathf.Abs(tmp_dist_IL) < nearEpsilon){ // those points that are close to the surface
                nearUnderWaterVertices.Add(new Vector3(tmp_IL.x, tmp_IL.y, tmp_IL.z));
            }

            Vector3 tmp_IM = I_M;//boatTrans.TransformPoint(boatVertices[j]);
            float tmp_dist_IM = WaterManagerObject.current.DistanceToWater(tmp_IM);
            if (Mathf.Abs(tmp_dist_IM) < nearEpsilon){ // those points that are close to the surface
                nearUnderWaterVertices.Add(new Vector3(tmp_IM.x, tmp_IM.y, tmp_IM.z));
            }


            //Save the data, such as normal, area, etc      
            //2 triangles below the water  
            underWaterTriangleData.Add(new TriangleData(M, I_M, I_L, boatRB));
            underWaterTriangleData.Add(new TriangleData(M, I_L, L, boatRB));

            indexOfOriginalTriangle.Add(triangleCounter);
            //Add 2 times because 2 submerged triangles need to connect to the same original triangle
            indexOfOriginalTriangle.Add(triangleCounter);
        }



        //Build the new triangles where two of the old vertices are above the water
        private void AddTrianglesTwoAboveWater(List<VertexData> vertexData, int triangleCounter)
        {
            //H and M are above the water
            //H is after the vertice that's below water, which is L
            //So we know which one is L because it is last in the sorted list
            Vector3 L = vertexData[2].globalVertexPos;

            //Find the index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }


            //We also need the heights to water
            float h_L = vertexData[2].distance;
            float h_H = 0f;
            float h_M = 0f;

            Vector3 H = Vector3.zero;
            Vector3 M = Vector3.zero;

            //This means that H is at position 1 in the list
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].globalVertexPos;
                M = vertexData[0].globalVertexPos;

                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].globalVertexPos;
                M = vertexData[1].globalVertexPos;

                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }


            //Now we can find where to cut the triangle

            //Point J_M
            Vector3 LM = M - L;

            float t_M = -h_L / (h_M - h_L);

            Vector3 LJ_M = t_M * LM;

            Vector3 J_M = LJ_M + L;


            //Point J_H
            Vector3 LH = H - L;

            float t_H = -h_L / (h_H - h_L);

            Vector3 LJ_H = t_H * LH;

            Vector3 J_H = LJ_H + L;


            // underWaterVertices.Add(J_M);
        

            Vector3 tmp_JM = J_M;//boatTrans.TransformPoint(boatVertices[j]);
            float tmp_dist_JM = WaterManagerObject.current.DistanceToWater(tmp_JM);
            if (Mathf.Abs(tmp_dist_JM) < nearEpsilon){ // those points that are close to the surface
                nearUnderWaterVertices.Add(new Vector3(tmp_JM.x, tmp_JM.y, tmp_JM.z));
            }

            Vector3 tmp_JH = J_H;//boatTrans.TransformPoint(boatVertices[j]);
            float tmp_dist_JH = WaterManagerObject.current.DistanceToWater(tmp_JH);
            if (Mathf.Abs(tmp_dist_JH) < nearEpsilon){ // those points that are close to the surface
                nearUnderWaterVertices.Add(new Vector3(tmp_JH.x, tmp_JH.y, tmp_JH.z));
            }

            //Save the data, such as normal, area, etc
            //1 triangle above the water
            underWaterTriangleData.Add(new TriangleData(L, J_H, J_M, boatRB));

            indexOfOriginalTriangle.Add(triangleCounter);
        }

        

        public Vector3 MeshCenterOfGravity(Mesh mesh, Vector3 offset){
            List<Vector3> c_tets = new List<Vector3>(); // list of tetrahedrons' centroid
            List<float> v_tets = new List<float>(); // list of tetrahedrons' volume

            Vector3 w_c_tets = new Vector3(); // the weighted centroid
            float s_v_tets = 0; // the sum of the volumes

            Vector3 tmp_p1;
            Vector3 tmp_p2;
            Vector3 tmp_p3;

            for (int i = 0; i < mesh.triangles.Length; i+=3){
                tmp_p1 = mesh.vertices[mesh.triangles[i]];
                tmp_p2 = mesh.vertices[mesh.triangles[i+1]];
                tmp_p3 = mesh.vertices[mesh.triangles[i+2]];

                v_tets.Add(VolumeTetrahedron(tmp_p1, tmp_p2, tmp_p3, new Vector3(0f, 0f, 0f)));
                c_tets.Add(CentroidTetrahedron(tmp_p1, tmp_p2, tmp_p3, new Vector3(0f, 0f, 0f)));
            }

            for (int i = 0; i < v_tets.Count; i++){
                w_c_tets += c_tets[i] * v_tets[i];
                s_v_tets += v_tets[i];
            }

            return (w_c_tets / s_v_tets) - offset;
        }
  
        public float VolumeTetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 aux){
            // Calculate the three non-coplanar vecotr of the Tetrahedron
            Vector3 v1 = p1 - aux;
            Vector3 v2 = p2 - aux;
            Vector3 v3 = p3 - aux;
            // Calculate the determinant of the three vector, this returns the values of the parallelopiped
            float v321 = v3.x * v2.y * v1.z;
            float v231 = v2.x * v3.y * v1.z;
            float v312 = v3.x * v1.y * v2.z;
            float v132 = v1.x * v3.y * v2.z;
            float v213 = v2.x * v1.y * v3.z;
            float v123 = v1.x * v2.y * v3.z;
            // Divide by 6 the result of the determinant, it results to be the volume of the tetrahedron
            return (1.0f/6.0f)*(-v321 + v231 + v312 - v132 - v213 + v123);
        }


        public Vector3 CentroidTetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 aux){
            // The fourth points, is consider to be the origin (0, 0, 0)
            float tet_centroid_x = (p1.x + p2.x + p3.x + aux.x) * 0.25f; // * 1/4
            float tet_centroid_y = (p1.y + p2.y + p3.y + aux.y) * 0.25f; // * 1/4
            float tet_centroid_z = (p1.z + p2.z + p3.z + aux.z) * 0.25f; // * 1/4
            Vector3 tet_centroid = new Vector3(tet_centroid_x, tet_centroid_y, tet_centroid_z);
            return tet_centroid;
        }



        //Display the underwater mesh
        public void DisplayMesh(Mesh mesh, string name, List<TriangleData> triangesData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            
            //Build the mesh
            for (int i = 0; i < triangesData.Count; i++)
            {
                //From global coordinates to local coordinates
                Vector3 p1 = boatTrans.InverseTransformPoint(triangesData[i].p1);
                Vector3 p2 = boatTrans.InverseTransformPoint(triangesData[i].p2);
                Vector3 p3 = boatTrans.InverseTransformPoint(triangesData[i].p3);

                vertices.Add(p1);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p2);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p3);
                triangles.Add(vertices.Count - 1);
            }

            //Remove the old mesh
            mesh.Clear();

            //Give it a name
            mesh.name = name;

            //Add the new vertices and triangles
            mesh.vertices = vertices.ToArray();

            mesh.triangles = triangles.ToArray();

            //Important to recalculate bounds because we need the bounds to calculate the length of the underwater mesh
            mesh.RecalculateBounds();
        }


        private static Vector3 CheckIsValid(Vector3 v, string name_vector)
        {
            if (!float.IsNaN(v.x + v.y + v.z))
            {
                return v;
            }
            else
            {
                Debug.Log(name_vector += " force is NaN");

                return Vector3.zero;
            }
        }

    }
}