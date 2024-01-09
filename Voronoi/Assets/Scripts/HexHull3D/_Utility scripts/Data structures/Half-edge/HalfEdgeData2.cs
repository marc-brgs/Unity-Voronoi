using System.Collections.Generic;

namespace Habrador_Computational_Geometry
{
    public class HalfEdgeData2
    {
        public HashSet<HalfEdgeVertex2> vertices;

        public HashSet<HalfEdgeFace2> faces;

        public HashSet<HalfEdge2> edges;
    }


    public class HalfEdgeVertex2
    {
        public MyVector2 position;

        public HalfEdge2 edge;

        public HalfEdgeVertex2(MyVector2 position)
        {
            this.position = position;
        }
    }



    public class HalfEdgeFace2
    {
        public HalfEdge2 edge;
        
        public HalfEdgeFace2(HalfEdge2 edge)
        {
            this.edge = edge;
        }
    }


    public class HalfEdge2
    {
        public HalfEdgeVertex2 v;
        public HalfEdgeFace2 face;
        public HalfEdge2 nextEdge;
        public HalfEdge2 oppositeEdge;
        public HalfEdge2 prevEdge;

        public HalfEdge2(HalfEdgeVertex2 v)
        {
            this.v = v;
        }
    }
}
