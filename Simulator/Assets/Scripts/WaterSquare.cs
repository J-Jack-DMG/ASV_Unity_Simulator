using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterSquare
{
    public Transform squareTransform;
	public MeshFilter waterMeshFilter;
    // Lenght of the square
	private float size;
	// Resolution of the sqare
	public float spacing;
	private int width;
    // Center of the water position
    public Vector3 centerPos;
    //The vertices of this square
    public Vector3[] vertices;

    public WaterSquare(GameObject waterSquareObj, float size, float spacing)
    {
        this.size = size;
        this.spacing = spacing;
        this.squareTransform = waterSquareObj.transform;
        this.waterMeshFilter = squareTransform.GetComponent<MeshFilter>();

        // The width of the water plane
        width = (int)(size / spacing);
        width += 1;
        float offset = -((width - 1) * spacing) / 2;

        Vector3 newPos = new Vector3(offset, squareTransform.position.y, offset);
        squareTransform.position += newPos;

        //Save the center position of the square
        this.centerPos = waterSquareObj.transform.localPosition;

        // Generate the mash of the water
        GenerateMesh();
        
        // Save the vertices
        this.vertices = waterMeshFilter.mesh.vertices;
    }
 
	public void MoveWater(Vector3 waterPos)
    {
        // obtaing the vertices of the water plane
        Vector3[] vertices = waterMeshFilter.mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
			Vector3 vertex = vertices[i];

            //From local to global
            //Vector3 vertexGlobal = squareTransform.TransformPoint(vertex);

            Vector3 vertexGlobal = vertex + centerPos + waterPos;

            //Unnecessary because no rotation nor scale
            //Vector3 vertexGlobalTest2 = squareTransform.rotation * Vector3.Scale(vertex, squareTransform.localScale) + squareTransform.position;

            //Debug 
            // if (i == 0)
            // {
            //     //Debug.Log(vertexGlobal + " " + vertexGlobalTest);
            // }

            //Get the water height at this coordinate
            vertex.y = WaterManagerObject.current.GetWaveYPos(vertexGlobal);

            //From global to local - not needed if we use the saved local x,z position
            //vertices[i] = transform.InverseTransformPoint(vertex);

            //Don't need to go from global to local because the y pos is always at 0
            vertices[i] = vertex;
            // Debug.Log("Update parameter ? Configuration : " + WaterController.current.configuration);
        }

		waterMeshFilter.mesh.vertices = vertices;

        waterMeshFilter.mesh.RecalculateNormals();
	}

    //Generate the water mesh
    public void GenerateMesh()
    {
        //Vertices
        List<Vector3[]> verts = new List<Vector3[]>();
		//Triangles
		List<int> tris = new List<int>();
		//Texturing
		List<Vector2> uvs = new List<Vector2>();
		
		for (int z = 0; z < width; z++)
        {
			
			verts.Add(new Vector3[width]);
			
			for (int x = 0; x < width; x++)
            {
				Vector3 current_point = new Vector3();
				
				//Get the corrdinates of the vertice
				current_point.x = x * spacing;
				current_point.z = z * spacing;
				current_point.y = squareTransform.position.y;
				
				verts[z][x] = current_point;
				
				uvs.Add(new Vector2(x,z));
				
				//Don't generate a triangle the first coordinate on each row
				//Because that's just one point
				if (x <= 0 || z <= 0)
                {
					continue;
				}

				//Each square consists of 2 triangles

				//The triangle south-west of the vertice
				tris.Add(x 		+ z * width);
				tris.Add(x 		+ (z-1) * width);
				tris.Add((x-1) 	+ (z-1) * width);
				
				//The triangle west-south of the vertice
				tris.Add(x 		+ z * width);
				tris.Add((x-1) 	+ (z-1) * width);
				tris.Add((x-1)	+ z * width);
			}
		}
		
		//Unfold the 2d array of verticies into a 1d array.
		Vector3[] unfolded_verts = new Vector3[width * width];

        int i = 0;
		foreach (Vector3[] v in verts)
        {
			//Copies all the elements of the current 1D-array to the specified 1D-array
			v.CopyTo(unfolded_verts, i * width);

            i++;
		}
		
		//Generate the mesh object
		Mesh newMesh = new Mesh();
        newMesh.vertices = unfolded_verts;
        newMesh.uv = uvs.ToArray();
        newMesh.triangles = tris.ToArray();

        //Ensure the bounding volume is correct
        newMesh.RecalculateBounds();
        //Update the normals to reflect the change
        newMesh.RecalculateNormals();


		//Add the generated mesh to this GameObject
		waterMeshFilter.mesh.Clear();
		waterMeshFilter.mesh = newMesh;
		waterMeshFilter.mesh.name = "Water Mesh";

        // Debug.Log(waterMeshFilter.mesh.vertices.Length);
	}
}