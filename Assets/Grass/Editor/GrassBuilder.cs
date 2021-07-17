using UnityEngine;
using UnityEngine.Rendering;

public static class GrassBuilder
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
        public Vector2 uv;
    }
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct GeneratedVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }
    
    private const int SOURCE_VERTEX_STRIDE = sizeof(float) * (3 + 2);
    private const int SOURCE_INDEX_STRIDE = sizeof(int);
    private const int GENERATED_VERTEX_STRIDE = sizeof(float) * (3 + 3 + 2);
    private const int GENERATED_INDEX_STRIDE = sizeof(int);

    private static void DecomposeMesh(Mesh mesh, int subMeshIndex, out SourceVertex[] vertices, out int[] indices)
    {
        SubMeshDescriptor subMeshDescriptor = mesh.GetSubMesh(subMeshIndex);

        Vector3[] meshVertices = mesh.vertices;
        Vector2[] meshUVs = mesh.uv;
        int[] meshIndices = mesh.triangles;

        vertices = new SourceVertex[subMeshDescriptor.vertexCount];
        indices = new int[subMeshDescriptor.indexCount];
        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            int wholeMeshIndex = i + subMeshDescriptor.firstVertex;
            vertices[i] = new SourceVertex { position = meshVertices[wholeMeshIndex], uv = meshUVs[wholeMeshIndex] };
        }

        for (int i = 0; i < subMeshDescriptor.indexCount; ++i)
        {
            indices[i] = meshIndices[i + subMeshDescriptor.indexStart] + subMeshDescriptor.baseVertex -
                         subMeshDescriptor.firstVertex;
        }
    }

    private static Mesh ComposeMesh(GeneratedVertex[] vertices, int[] indices)
    {
        Mesh mesh = new Mesh();
        Vector3[] meshVertices = new Vector3[vertices.Length];
        Vector3[] meshNormals = new Vector3[vertices.Length];
        Vector2[] meshUVs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; ++i)
        {
            meshVertices[i] = vertices[i].position;
            meshNormals[i] = vertices[i].normal;
            meshUVs[i] = vertices[i].uv;
        }
        mesh.SetVertices(meshVertices);
        mesh.SetNormals(meshNormals);
        mesh.SetUVs(0, meshUVs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0, true);
        mesh.Optimize();
        return mesh;
    }

    public static bool Run(GrassBakeSettings settings, out Mesh generatedMesh)
    {
        DecomposeMesh(settings.grassBladeMesh, 0, out SourceVertex[] sourceGrassBladeVertices, out int[] sourceGrassBladeIndices);

        int numBlades = (int)(settings.extents.x * settings.extents.y / (settings.density * settings.density));
        
        GeneratedVertex[] generatedVertices = new GeneratedVertex[numBlades * sourceGrassBladeVertices.Length];
        int[] generatedIndices = new int[numBlades * sourceGrassBladeIndices.Length];
        
        GraphicsBuffer sourceGrassBladeVertexBuffer  = new GraphicsBuffer(GraphicsBuffer.Target.Structured, sourceGrassBladeIndices.Length, SOURCE_VERTEX_STRIDE);
        GraphicsBuffer sourceGrassBladeIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, sourceGrassBladeIndices.Length, SOURCE_INDEX_STRIDE);
        GraphicsBuffer generatedVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, generatedVertices.Length, GENERATED_VERTEX_STRIDE);
        GraphicsBuffer generatedIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, generatedIndices.Length, GENERATED_INDEX_STRIDE);

        ComputeShader shader = settings.computeShader;
        int idGrassKernel = shader.FindKernel("CSMain");
        
        shader.SetBuffer(idGrassKernel, "_SourceGrassBladeVertices", sourceGrassBladeVertexBuffer);
        shader.SetBuffer(idGrassKernel, "_SourceGrassBladeIndices", sourceGrassBladeIndexBuffer);
        shader.SetBuffer(idGrassKernel, "_GeneratedVertices", generatedVertexBuffer);
        shader.SetBuffer(idGrassKernel, "_GeneratedIndices", generatedIndexBuffer);
        shader.SetVector("_MinMaxRandomScale", settings.minMaxScale);
        shader.SetVector("_Extents", settings.extents);
        shader.SetFloat("_Density", settings.density);
        shader.SetInt("_NumBlades", numBlades);
        shader.SetInt("_NumGrassBladeVertices", sourceGrassBladeVertices.Length);
        shader.SetInt("_NumGrassBladeIndices", sourceGrassBladeIndices.Length);
        
        sourceGrassBladeVertexBuffer.SetData(sourceGrassBladeVertices);
        sourceGrassBladeIndexBuffer.SetData(sourceGrassBladeIndices);
        
        shader.GetKernelThreadGroupSizes(idGrassKernel, out uint threadGroupSize, out _, out _);
        int dispatchSize = Mathf.CeilToInt(numBlades);
        shader.Dispatch(idGrassKernel, dispatchSize, 1, 1);
        
        generatedVertexBuffer.GetData(generatedVertices);
        generatedIndexBuffer.GetData(generatedIndices);

        generatedMesh = ComposeMesh(generatedVertices, generatedIndices);
        
        generatedVertexBuffer.Release();
        generatedIndexBuffer.Release();

        return true;
    }
}
