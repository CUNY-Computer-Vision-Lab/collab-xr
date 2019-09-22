using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Academy.HoloToolkit.Unity;
using Photon.Pun;
using System;

/// <summary>
/// The SurfaceManager class allows applications to scan the environment for a specified amount of time 
/// and then process the Spatial Mapping Mesh (find planes, remove vertices) after that time has expired.
/// </summary>
public class PlaySpaceManager : Singleton<PlaySpaceManager>
{
    [Tooltip("When checked, the SurfaceObserver will stop running after a specified amount of time.")]
    public bool limitScanningByTime = true;

    [Tooltip("How much time (in seconds) that the SurfaceObserver will run after being started; used when 'Limit Scanning By Time' is checked.")]
    public float scanTime = 30.0f;

    [Tooltip("Material to use when rendering Spatial Mapping meshes while the observer is running.")]
    public Material defaultMaterial;

    [Tooltip("Optional Material to use when rendering Spatial Mapping meshes after the observer has been stopped.")]
    public Material secondaryMaterial;

    [Tooltip("Minimum number of floor planes required in order to exit scanning/processing mode.")]
    public uint minimumFloors = 1;

    [Tooltip("Minimum number of wall planes required in order to exit scanning/processing mode.")]
    public uint minimumWalls = 1;

    public Camera backgroundCamera;
    //public RawImage rawImage;
    public static PhotonView photonView;

    /// <summary>
    /// GameObject initialization.
    /// </summary>
    private void Start()
    {
        // Update surfaceObserver and storedMeshes to use the same material during scanning.
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMaterial);

        photonView = PhotonView.Get(this);

        Debug.Log("STARTING TIMER");

        var timer = new System.Threading.Timer(
            e => this.sendMesh(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5)
        );
    }

    void sendMesh()
    {
        Debug.Log("Sending Mes 1");

        GameObject[] meshChunks = GameObject.FindGameObjectsWithTag("PhotonMesh");

        Debug.Log("making meshfilters list");

        List<MeshFilter> meshFilters = new List<MeshFilter>();

        foreach (var item in meshChunks)
        {
            MeshFilter _meshFilter = item.GetComponent<MeshFilter>();
            if (_meshFilter)
            {
                meshFilters.Add(_meshFilter);
            }
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        int i = 0;
        while (i < meshFilters.Count)
        {
            Debug.Log(i.ToString());
            combine[i].mesh = meshFilters[i].mesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        Debug.Log("mesh count: ");
        Debug.Log(meshFilters.Count.ToString());

        Mesh mesh = new Mesh();

        mesh.CombineMeshes(combine);

        byte[] serialized = MeshSerializer.WriteMesh(mesh, true);

        photonView.RPC("GetStreamData", RpcTarget.All, serialized);

    }

}