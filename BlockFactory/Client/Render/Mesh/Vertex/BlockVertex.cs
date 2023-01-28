﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockFactory.Client.Render.Mesh.Vertex
{
    public struct BlockVertex
    {
        [VertexAttribute(0)]
        public Vector3 Pos;
        [VertexAttribute(1)]
        public Vector3 Color;
        [VertexAttribute(2)]
        public Vector3 Uv;

        public BlockVertex(Vector3 pos, Vector3 color, Vector3 uv)
        {
            Pos = pos;
            Color = color;
            Uv = uv;
        }

        public static implicit operator BlockVertex((Vector3 pos, Vector3 color, Vector3 uv) values)
        {
            return new BlockVertex(values.pos, values.color, values.uv);
        }

        public static implicit operator BlockVertex((float X, float Y, float Z, float R, float G, float B, float UvX, float UvY, float Layer) values)
        {
            return new BlockVertex((values.X, values.Y, values.Z), (values.R, values.G, values.B), (values.UvX, values.UvY, values.Layer));
        }

        public static implicit operator BlockVertex((float X, float Y, float Z, float R, float G, float B, float UvX, float UvY) values)
        {
            return new BlockVertex((values.X, values.Y, values.Z), (values.R, values.G, values.B), (values.UvX, values.UvY, 0f));
        }
    }
}
