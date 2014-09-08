using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectEngine
{
    public class DrawablePlane : DrawableObj
    {
        public DrawablePlane(Game game, string textureName, string normalMapName, bool useTransparency)
            : base(game,
                new Vertex[] {
		            // Front Face
		            new Vertex(-1.0f, -1.0f, 0.0f, 0.0f, 1.0f),
		            new Vertex(-1.0f,  1.0f, 0.0f, 0.0f, 0.0f),
		            new Vertex( 1.0f,  1.0f, 0.0f, 1.0f, 0.0f),
		            new Vertex( 1.0f, -1.0f, 0.0f, 1.0f, 1.0f)},
                new int[] {
		            // Front Face
		            0,  1,  2,
		            0,  2,  3}, textureName, normalMapName, useTransparency)
        { }
    }
}
