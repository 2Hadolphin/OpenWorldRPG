using UnityEngine;
using System;
using System.Collections.Generic;
using Return;

namespace Return.Physical
{
    public class SurfaceIdentifier : MonoBehaviour
    {
        /// <summary>
        /// List of surfaces of the GameObject.
        /// </summary>
        [SerializeField]
        [Tooltip("List of surfaces of the GameObject.")]
        private SurfaceData[] m_SurfaceList = new SurfaceData[1];

        #region PROPERTIES

        /// <summary>
        /// Returns the current active terrain.
        /// </summary>
        private Terrain ActiveTerrain => GetComponent<Terrain>();

        /// <summary>
        /// Returns true if the surface is on terrain, false otherwise.
        /// </summary>
        public bool IsTerrain => GetComponent<Terrain>() != null;

        /// <summary>
        /// Returns the array of Materials found in the object.
        /// </summary>
        public Material[] Materials
        {
            get
            {
                Renderer r = gameObject.GetComponent<Renderer>();
                return r != null ? r.sharedMaterials : null;
            }
        }

        /// <summary>
        /// Returns the array of Textures found in the object.
        /// </summary>
        public Texture2D[] Textures
        {
            get
            {
                if (!IsTerrain || ActiveTerrain == null)
                    return null;

                TerrainLayer[] terrainLayers = ActiveTerrain.terrainData.terrainLayers;
                Texture2D[] tex = new Texture2D[terrainLayers.Length];

                for (int i = 0; i < terrainLayers.Length; i++)
                    tex[i] = terrainLayers[i].diffuseTexture;

                return tex;
            }
        }

        #region EDITOR

        /// <summary>
        /// Returns the number of surfaces on the object.
        /// </summary>
        public int SurfacesCount => m_SurfaceList.Length;

        #endregion

        #endregion

        /// <summary>
        /// Returns true if the surface allows decals to be drawn on it, false otherwise.
        /// </summary>
        /// <param name="triangleIndex">The hit triangle index.</param>
        public bool AllowDecals(int triangleIndex = -1)
        {
            if (m_SurfaceList == null)
                return false;

            return !IsTerrain && m_SurfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].AllowDecals;
        }

        /// <summary>
        /// Returns true if projectiles can penetrate this surface, false otherwise.
        /// </summary>
        /// <param name="triangleIndex">The hit triangle index.</param>
        public bool CanPenetrate(int triangleIndex = -1)
        {
            if (m_SurfaceList == null)
                return false;

            return !IsTerrain && m_SurfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].Penetration;
        }

        /// <summary>
        /// Returns the density of this surface.
        /// </summary>
        /// <param name="triangleIndex">The hit triangle index.</param>
        public float Density(int triangleIndex = -1)
        {
            if (m_SurfaceList == null)
                return 1;

            if (!IsTerrain)
            {
                return m_SurfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].Density;
            }
            return 1;
        }

        /// <summary>
        /// Returns the SurfaceType at given position.
        /// </summary>
        /// <param name="position">The contact position. (if the GameObject is a Terrain)</param>
        /// <param name="triangleIndex">The hit triangle index.</param>
        /// <returns></returns>
        public SurfaceType GetSurfaceType(Vector3 position, int triangleIndex = -1)
        {
            if (m_SurfaceList == null || m_SurfaceList.Length <= 0)
                return null;

            if (IsTerrain)
            {
                int index = SurfaceUtility.GetMainTexture(position, ActiveTerrain.transform.position, ActiveTerrain.terrainData);
                return index < m_SurfaceList.Length ? m_SurfaceList[index].SurfaceType : null;

            }
            return m_SurfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].SurfaceType;
        }

        /// <summary>
        /// Resets the Component.
        /// </summary>
        public void Reset()
        {
            m_SurfaceList = GetSurfaceList();
        }

        /// <summary>
        /// Caches all surfaces found on the GameObject.
        /// </summary>
        private SurfaceData[] GetSurfaceList()
        {
            SurfaceData[] surfaces;

            // Is this component attached to a terrain?
            if (IsTerrain)
            {
                TerrainLayer[] terrainLayers = ActiveTerrain.terrainData.terrainLayers;
                surfaces = new SurfaceData[terrainLayers.Length];

                for (int i = 0; i < terrainLayers.Length; i++)
                    surfaces[i] = new SurfaceData();
            }
            else
            {
                Renderer r = gameObject.GetComponent<Renderer>();

                if (r && r.sharedMaterials.Length > 0)
                {
                    surfaces = new SurfaceData[r.sharedMaterials.Length];

                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                        surfaces[i] = new SurfaceData();
                }
                else
                {
                    surfaces = new[] { new SurfaceData() };
                }
            }
            return surfaces;
        }
    }

    public static class ColliderExtension
    {
        /// <summary>
        /// Cached surfaces.
        /// </summary>
        private static readonly Dictionary<int, SurfaceIdentifier> m_SurfaceIdentifiers = new Dictionary<int, SurfaceIdentifier>();

        /// <summary>
        /// Instead of trying to get the Surface Identifier using GetComponent() every time we need to check a object,
        /// a dictionary is much more performance efficient because we have to check a object only once.
        /// However it's not possible to detect changes in the object (add a SurfaceIdentifier at runtime) once it has been registered.
        /// </summary>
        public static SurfaceIdentifier GetSurface(this Collider col)
        {
            var id = col.GetInstanceID();

            if (!m_SurfaceIdentifiers.TryGetValue(id, out var identifier))
            {
                identifier = col.GetComponentInParent<SurfaceIdentifier>();

                if (identifier.NotNull())
                    m_SurfaceIdentifiers.Add(id, identifier);
            }

            return identifier;
        }
    }

    public static class SurfaceUtility
    {
        /// <summary>
        /// Returns the zero-based index of the most dominant texture on the main terrain at this world position.
        /// </summary>
        /// <param name="worldPos">The position on the terrain surface used to calculate the dominant texture.</param>
        /// <param name="terrainPos">The current terrain position.</param>
        /// <param name="terrainData">The current terrain data.</param>
        public static int GetMainTexture(Vector3 worldPos, Vector3 terrainPos, TerrainData terrainData)
        {
            float[] mix = GetTextureMix(worldPos, terrainPos, terrainData);

            float maxMix = 0;
            int maxIndex = 0;

            // Loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; n++)
            {
                if (!(mix[n] > maxMix))
                    continue;

                maxIndex = n;
                maxMix = mix[n];
            }
            return maxIndex;
        }

        /// <summary>
        /// Returns an array containing the relative mix of textures on the main terrain at this world position.
        /// The number of values in the array will equal the number of textures added to the terrain.
        /// </summary>
        /// <param name="worldPos">The position on the terrain surface used to calculate the dominant texture.</param>
        /// <param name="terrainPos">The current terrain position.</param>
        /// <param name="terrainData">The current terrain data.</param>
        private static float[] GetTextureMix(Vector3 worldPos, Vector3 terrainPos, TerrainData terrainData)
        {
            // Calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            // Get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatMapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // Extract the 3D array data to a 1D array
            float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatMapData[0, 0, n];
            }
            return cellMix;
        }

        /// <summary>
        /// Returns the zero-based index of the material in the given triangle.
        /// </summary>
        /// <param name="triangleIndex">The triangle index.</param>
        /// <param name="gameObject">Target object.</param>
        /// <returns></returns>
        public static int GetMaterialIndex(int triangleIndex, GameObject gameObject)
        {
            // Return 0 if the mesh hasn't any sub meshes
            if (triangleIndex == -1)
                return 0;

            Mesh mesh = GetSharedMesh(gameObject);

            if (!mesh || !mesh.isReadable)
                return 0;

            int[] hitTriangle =
            {
                mesh.triangles[triangleIndex * 3],
                mesh.triangles[triangleIndex * 3 + 1],
                mesh.triangles[triangleIndex * 3 + 2]
            };

            // Loop through each sub mesh
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subMeshTris = mesh.GetTriangles(i);

                // Loop through each triangle in the sub mesh
                for (int t = 0; t < subMeshTris.Length; t += 3)
                {
                    // Return the index of which sub mesh the triangle is inscribed in.
                    if (subMeshTris[t] == hitTriangle[0] && subMeshTris[t + 1] == hitTriangle[1] && subMeshTris[t + 2] == hitTriangle[2])
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the sharedMesh in the object.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <returns></returns>
        private static Mesh GetSharedMesh(GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter)
            {
                return meshFilter.sharedMesh;
            }

            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            return !skinnedMeshRenderer ? null : skinnedMeshRenderer.sharedMesh;
        }
    }
}

