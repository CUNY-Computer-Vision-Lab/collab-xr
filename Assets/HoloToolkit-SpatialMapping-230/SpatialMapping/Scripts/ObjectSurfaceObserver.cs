// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Academy.HoloToolkit.Unity
{
    public class ObjectSurfaceObserver : SpatialMappingSource
    {
        [Tooltip("The room model to use when loading meshes in Unity.")]
        public GameObject roomModel;

        // Use this for initialization.
        private void Start()
        {
#if UNITY_EDITOR
            // When in the Unity editor, try loading saved meshes from a model.
            Load(roomModel);

            if (GetMeshFilters().Count > 0)
            {
                SpatialMappingManager.Instance.SetSpatialMappingSource(this);
            }
#endif
        }

        /// <summary>
        /// Send mesh to collaborator.
        /// </summary>
        void TransferMesh()
        {
            GameObject roomObject = GameObject.FindWithTag("SRMesh");
            PhotonView photonView = PhotonView.Get(roomObject);

            MeshFilter[] meshFilters = roomObject.GetComponentsInChildren<MeshFilter>();

            Debug.Log("Found " + meshFilters.Length + " mesh chunks. Combining...");

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].mesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }

            Mesh mesh = new Mesh();

            mesh.CombineMeshes(combine);

            byte[] serialized = MeshSerializer.WriteMesh(mesh, true);

            Debug.Log("Sending combined and serialized mesh.");

            photonView.RPC("TransferMesh", RpcTarget.All, serialized);
        }

        /// <summary>
        /// Loads the SpatialMapping mesh from the specified room object.
        /// </summary>
        /// <param name="roomModel">The room model to load meshes from.</param>
        public void Load(GameObject roomModel)
        {
            if (roomModel == null)
            {
                Debug.Log("No room model specified.");
                return;
            }

            GameObject roomObject = GameObject.Instantiate(roomModel);
            Cleanup();

            try
            {
                MeshFilter[] roomFilters = roomObject.GetComponentsInChildren<MeshFilter>();

                foreach (MeshFilter filter in roomFilters)
                {
                    GameObject surface = AddSurfaceObject(filter.sharedMesh, "roomMesh-" + SurfaceObjects.Count, transform);
                    Renderer renderer = surface.GetComponent<MeshRenderer>();

                    if (SpatialMappingManager.Instance.DrawVisualMeshes == false)
                    {
                        renderer.enabled = false;
                    }

                    if (SpatialMappingManager.Instance.CastShadows == false)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }

                    // Reset the surface mesh collider to fit the updated mesh. 
                    // Unity tribal knowledge indicates that to change the mesh assigned to a
                    // mesh collider, the mesh must first be set to null.  Presumably there
                    // is a side effect in the setter when setting the shared mesh to null.
                    MeshCollider collider = surface.GetComponent<MeshCollider>();
                    collider.sharedMesh = null;
                    collider.sharedMesh = surface.GetComponent<MeshFilter>().sharedMesh;
                }

                // Collab-xr -- send mesh data to remote agent
                this.TransferMesh();
            }
            catch
            {
                Debug.Log("Failed to load object " + roomModel.name);
            }
            finally
            {
                if (roomModel != null && roomObject != null)
                {
                    GameObject.DestroyImmediate(roomObject);
                }
            }
        }
    }
}