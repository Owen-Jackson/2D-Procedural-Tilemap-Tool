using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDisplay : MonoBehaviour {
    public Material material;
    [SerializeField]
    Vector2 startPos;
    [SerializeField]
    float cellSize;

    [SerializeField]
    int gridWidth;
    [SerializeField]
    int gridHeight;
    
	// Use this for initialization
	void Start () {
		
	}

    private void OnPostRender()
    {
        if(!material)
        {
            Debug.Log("no material");
            return;
        }
        //GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        //GL.modelview = transform.localToWorldMatrix;
        GL.PushMatrix();
        //GL.LoadPixelMatrix();
        //GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(Color.red);

        for(int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                GL.Vertex(new Vector2(startPos.x + j * cellSize, startPos.y + i * cellSize));    //vertical start
                GL.Vertex(new Vector2(startPos.x + j * cellSize, startPos.y + (i + 1) * cellSize));  //vertical end

                GL.Vertex(new Vector2(startPos.x + j * cellSize, startPos.y + i * cellSize));  //horizontal start
                GL.Vertex(new Vector2(startPos.x + (j + 1) * cellSize, startPos.y + i * cellSize));    //horizontal end


                //GL.Vertex(new Vector2(startPos.x + (j + 1) * cellSize, startPos.y + (i + 1) * cellSize));  //top right
                //GL.Vertex(new Vector2(startPos.x + (j + 1) * cellSize, startPos.y + i * cellSize));    //bottom right
            }   
        }

        GL.End();
        GL.PopMatrix();
    }
}
