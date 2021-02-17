namespace CAARA
{
    class GraphVertexInfo
    {
        public GraphVertex Vertex { get; set; }

        public bool IsUnvisited { get; set; }

        public int EdgesWeightSum { get; set; }

        public GraphVertex PreviousVertex { get; set; }

        public GraphVertexInfo(GraphVertex vertex)
        {
            Vertex = vertex;
            IsUnvisited = true;
            EdgesWeightSum = int.MaxValue;
            PreviousVertex = null;
        }
    }
}