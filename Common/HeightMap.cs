using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common
{
    /// <summary>
    /// Class for a simple plane
    /// </summary>
    public class HeightMap : CustomObject
    {
        public Texture2D TextureMap { get; set; }
        public float ZeroLevel { get; set; }
        public float ElevationScale { get; set; }
        private Color[] HeightMapData { get; set; }

        /// <summary>
        /// Constructor to generate the plane geometry
        /// </summary>
        /// <param name="segments">The number of rows/columns</param>
        public HeightMap(Texture2D heightMap, int segments = 99, float zeroLevel = 1, float elevationScale = 0.1f)
        {
            TextureMap = heightMap; // store the heightmap;
            HeightMapData = new Color[TextureMap.Width*TextureMap.Height];
            TextureMap.GetData<Color>(HeightMapData);
            ZeroLevel = zeroLevel;
            ElevationScale = elevationScale;

            int rowCount = segments + 1;
            float fSegments = segments;
            // Initialize the arrays
            Indices = new int[6 * segments * segments];
            Vertices = new VertexPositionNormalTexture[(rowCount) * (rowCount)];
            // Populate the vertices
            for (int i = 0; i <= segments; i++)
                for (int j = 0; j <= segments; j++)
                {
                    Vector2 textureCoord = new Vector2(j / fSegments, i / fSegments);
                    Vertices[i * rowCount + j] = new VertexPositionNormalTexture(
                        new Vector3(-1 + 2 * j / fSegments,
                                    GetElevationBilinear(textureCoord),
                                    -1 + 2 * i / fSegments), // Position
                        Vector3.Up, // Normal - Zero instead
                        textureCoord); // Texture
                }
            // Populate the indices
            int index = 0;
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < segments; j++)
                {
                    Indices[index++] = i * rowCount + j;
                    Indices[index++] = i * rowCount + j + 1;
                    Indices[index++] = (i + 1) * rowCount + j + 1;

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(
                        (Vertices[Indices[index - 1]].Position -
                        Vertices[Indices[index - 3]].Position),
                        (Vertices[Indices[index - 2]].Position -
                        Vertices[Indices[index - 3]].Position)
                        ));
                    Vertices[Indices[index - 3]].Normal += normal;
                    Vertices[Indices[index - 2]].Normal += normal;
                    Vertices[Indices[index - 1]].Normal += normal;

                    Indices[index++] = i * rowCount + j;
                    Indices[index++] = (i + 1) * rowCount + j + 1;
                    Indices[index++] = (i + 1) * rowCount + j;

                    normal = Vector3.Normalize(Vector3.Cross(
                        (Vertices[Indices[index - 1]].Position -
                        Vertices[Indices[index - 3]].Position),
                        (Vertices[Indices[index - 2]].Position -
                        Vertices[Indices[index - 3]].Position)
                        ));
                    Vertices[Indices[index - 3]].Normal += normal;
                    Vertices[Indices[index - 2]].Normal += normal;
                    Vertices[Indices[index - 1]].Normal += normal;
                }

            foreach (VertexPositionNormalTexture vertex in Vertices)
                vertex.Normal.Normalize();
        }

        /// <summary>
        /// Performs bilinear interpolation to give the elevation at the specified (u,v)
        /// parametric position
        /// </summary>
        /// <param name="textureCoords">The parametric position on the map</param>
        /// <returns>Elevation at the point</returns>
        public float GetElevationBilinear(Vector2 textureCoords)
        {
            // Change the textureCoords from (0,1) to (0,{width;height}) range
            textureCoords *= new Vector2(TextureMap.Width - 1, TextureMap.Height - 1);
            int imageRow = (int)(textureCoords.Y);
            int imageCol = (int)(textureCoords.X);
            Vector2 uv = textureCoords - new Vector2(imageCol, imageRow);
            return ((HeightMapData[imageRow * TextureMap.Width + imageCol].R * (1 - uv.X) * (1 - uv.Y) +
                          HeightMapData[imageRow * TextureMap.Width + (imageCol + 1) % TextureMap.Width].R * uv.X * (1 - uv.Y) +
                          HeightMapData[(imageRow + 1) % TextureMap.Height * TextureMap.Width + imageCol].R * (1 - uv.X) * uv.Y +
                          HeightMapData[(imageRow + 1) % TextureMap.Height * TextureMap.Width + (imageCol + 1) % TextureMap.Width].R * uv.X * uv.Y) / 128f -
                          ZeroLevel) * ElevationScale;
        }

        /// <summary>
        /// Performs bilinear interpolation to give the elevation at the specified (u,v)
        /// parametric position
        /// </summary>
        /// <param name="textureCoords">The parametric position on the map</param>
        /// <returns>Elevation at the point</returns>
        public float GetElevationNearest(Vector2 textureCoords)
        {
            // Change the textureCoords from (0,1) to (0,{width;height}) range
            textureCoords *= new Vector2(TextureMap.Width - 1, TextureMap.Height - 1);
            int imageRow = (int)(textureCoords.Y + 0.5f);
            int imageCol = (int)(textureCoords.X + 0.5f);
            return (HeightMapData[imageRow * TextureMap.Width + imageCol].R / 128f -
                          ZeroLevel) * ElevationScale;
        }
    }
}
