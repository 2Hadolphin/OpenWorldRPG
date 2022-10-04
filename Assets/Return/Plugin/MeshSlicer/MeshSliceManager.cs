#if UNITY_EDITOR

using BzKovSoft.CharacterSlicer;
using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.EventHandlers;
using BzKovSoft.ObjectSlicer.Polygon;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using Sirenix.OdinInspector;
using Return;
//using Return.Editors;
using UnityEditor;
using UnityEngine.Assertions;

[Serializable]
public class MeshSliceManager
{
	public MeshSliceManager(SkinnedMeshRenderer skinnedMeshRenderer)
    {
		SkinnedMeshRenderer = skinnedMeshRenderer;
		_sliceTrys = new Queue<SliceTry>();
	}

	[Tooltip("Root transform of character.")]
	[BoxGroup("Config")]
	public GameObject CharacterRoot;

	[OnValueChanged(nameof(LoadSkinnedRenderer))]
    [Tooltip("Target skinned renderer to slice.")]
    [BoxGroup("Config")]
	public SkinnedMeshRenderer SkinnedMeshRenderer;

	void LoadSkinnedRenderer()
    {
		if (SkinnedMeshRenderer == null)
			return;

		if (CharacterRoot == null)
			CharacterRoot = SkinnedMeshRenderer.transform.root.gameObject;

		if (defaultSliceMaterial == null)
			defaultSliceMaterial = SkinnedMeshRenderer.sharedMaterial;
    }


	[BoxGroup("SliceConfig")]
	[Tooltip("Confirm to slice mesh.")]
	[Button]
	void Slice()
    {
		Slice(Plane.TransformPlane(),  null);
	}

	[Tooltip("Coordinate to split mesh.")]
	[BoxGroup("SliceConfig")]
	[HorizontalGroup("SliceConfig/Plane")]
	public Transform Plane;

	bool planeNull => Plane == null;

	[ShowIf(nameof(planeNull))]
	[Button]
	[HorizontalGroup("SliceConfig/Plane")]
	void Create()
	{
		Plane = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
		Plane.SetWorldPR(SkinnedMeshRenderer.rootBone);
		Plane.localScale = new(0.1f, 0.1f, 0.1f);

		Selection.activeGameObject = Plane.gameObject;
		EditorGUIUtility.PingObject(Plane);
	}

	[BoxGroup("SliceConfig")]
	public Side SaveMeshSide=Side.Right;

	/// <summary>
	/// Material that will be applied after slicing
	/// </summary>

	[BoxGroup("SliceConfig")]
	public Material defaultSliceMaterial;
	[BoxGroup("SliceConfig")]
	public bool asynchronously = false;
	[BoxGroup("SliceConfig")]
	public bool useLazyRunner;

	public Mesh CombinedMesh;

	[Button]
	void ArchiveMeshes(bool combineSubmesh = true)
	{

        CombinedMesh = Combine(SplitMeshs.ToArray());
        return;

        var newMesh = new Mesh();

		var length = SplitMeshs.Count;

		var combines = new List<CombineInstance>(length*2);


		for (int i = 0; i < length; i++)
        {
			var mesh = SplitMeshs[i];

			//mesh.RecalculateNormals();
			var combine = new CombineInstance()
			{
				mesh = mesh,
				transform = Matrix4x4.identity,
				subMeshIndex = 0
			};
			combines.Add(combine);
			continue;
			//var subMeshCount = mesh.subMeshCount;

   //         for (int s = 0; s < subMeshCount; s++)
   //         {
			//	var combine = new CombineInstance()
			//	{
			//		mesh = mesh,
			//		transform = Matrix4x4.identity,
			//		subMeshIndex = s
			//	};
			//	combines.Add(combine);
			//}
		



		}

		newMesh.CombineMeshes(combines.ToArray(), true, false,false);
        var sm = SkinnedMeshRenderer.sharedMesh;
		newMesh.bindposes = sm.bindposes;
		newMesh.SetBoneWeights(sm.GetBonesPerVertex(), sm.GetAllBoneWeights());
		//newMesh.boneWeights = sm.boneWeights;

		newMesh.RecalculateNormals();


        CombinedMesh = newMesh;
	}

	public Mesh Combine(params Mesh[] meshs)
    {
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var normals = new List<Vector3>();
		var tangents = new List<Vector4>();

		var colors = new List<Color>();

		var uv = new List<Vector2>();
		var uv2 = new List<Vector2>();
		var uv3 = new List<Vector2>();
		var uv4 = new List<Vector2>();

		var bindposes = new List<Matrix4x4>();
		var boneWeights = new List<BoneWeight>();

		Debug.Log(meshs.Length);

		int verticeCount=0;

        foreach (var mesh in meshs)
        {
			if (mesh.vertices.Length > 0)
				vertices.AddRange(mesh.vertices);


			if (mesh.triangles.Length > 0)
            {
				var tris = mesh.triangles;

				foreach (var tri in tris)
                {
					triangles.Add(tri + verticeCount);
				}
			}
      

			if (mesh.tangents.Length > 0)
				tangents.AddRange(mesh.tangents);

			if (mesh.colors.Length > 0)
				colors.AddRange(mesh.colors);

			if (mesh.normals.Length > 0)
				normals.AddRange(mesh.normals);

			var length = mesh.bindposes.Length;

			if(bindposes.Count>0)
			for (int i = 0; i < length; i++)
            {
				Assert.IsTrue(mesh.bindposes[i] == bindposes[i]);
            }

			if (mesh.bindposes.Length>0)
				bindposes.AddRange(mesh.bindposes);

			if (mesh.boneWeights.Length > 0)
				boneWeights.AddRange(mesh.boneWeights);

			#region UV

			if (mesh.uv.Length > 0)
				uv.AddRange(mesh.uv);

			if (mesh.uv2.Length > 0)
				uv2.AddRange(mesh.uv2);

			if (mesh.uv3.Length > 0)
				uv3.AddRange(mesh.uv3);

			if (mesh.uv4.Length > 0)
				uv4.AddRange(mesh.uv4);

			#endregion

			verticeCount += mesh.vertexCount;
		}

		var newMesh = new Mesh()
		{
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
		};

		if (tangents.Count > 0)
			newMesh.tangents = tangents.ToArray();

		if (normals.Count > 0)
			newMesh.normals = normals.ToArray();

		if (colors.Count > 0)
			newMesh.colors = colors.ToArray();

        //if (bindposes.Count > 0)
        //	newMesh.bindposes = bindposes.ToArray();

        if (boneWeights.Count > 0)
            newMesh.boneWeights = boneWeights.ToArray();

        newMesh.bindposes = SkinnedMeshRenderer.sharedMesh.bindposes;

		if (uv.Count > 0)
			newMesh.uv = uv.ToArray();

		if (uv2.Count > 0)
			newMesh.uv2 = uv2.ToArray();

		if (uv3.Count > 0)
			newMesh.uv3 = uv3.ToArray();

		if (uv4.Count > 0)
			newMesh.uv4 = uv4.ToArray();

		newMesh.RecalculateBounds();
		//newMesh.bounds = SkinnedMeshRenderer.sharedMesh.bounds;
		//newMesh.RecalculateNormals();
		return newMesh;
	}

	[SerializeField]
	public List<Mesh> SplitMeshs=new ();

	public Mesh Pos;
	public Mesh Neg;

	Queue<SliceTry> _sliceTrys;
	[SerializeField]
	[HideInInspector]
	public SliceTry lastSuccessfulSlice;


	/// <summary>
	/// Start slicing process
	/// </summary>
	/// <param name="addData">You can pass any object. You will </param>
	/// <returns>Returns true if pre-slice conditions was succeeded and task was added to the queue</returns>
	private void StartSlice(BzSliceTryData sliceTryData, IBzSliceAdapter[] adapters, Action<BzSliceTryResult> callBack)
	{
		Renderer[] renderers = CharacterRoot.GetComponentsInChildren<Renderer>();
		SliceTryItem[] items = new SliceTryItem[renderers.Length];

		for (int i = 0; i < renderers.Length; i++)
		{
			var renderer = renderers[i];

			var adapterAndMesh = GetAdapterAndMesh(renderer);

			if (adapterAndMesh == null)
				continue;

			Mesh mesh = adapterAndMesh.mesh;
			IBzSliceAdapter adapter = adapters == null ? adapterAndMesh.adapter : adapters[i];

			var configuration = renderer.gameObject.GetComponent<BzSliceConfiguration>();

			var confDto = configuration == null ?
				BzSliceConfiguration.GetDefault() : configuration.GetDto();

            var meshDissector = new MeshDissector(mesh, sliceTryData.plane, renderer.sharedMaterials, adapter, confDto)
            {
                DefaultSliceMaterial = defaultSliceMaterial
            };

            var sliceTryItem = new SliceTryItem
            {
                meshRenderer = renderer,
                meshDissector = meshDissector
            };
            items[i] = sliceTryItem;
		}

        var sliceTry = new SliceTry
        {
            items = items,
            callBack = callBack,
            sliceData = sliceTryData,
            position = CharacterRoot.transform.position,
            rotation = CharacterRoot.transform.rotation
        };

        if (asynchronously)
		{
			StartWorker(WorkForWorker, sliceTry);
			_sliceTrys.Enqueue(sliceTry);
		}
		else
		{
			Work(sliceTry);
			SliceTryFinished(sliceTry);
		}
	}

	protected virtual AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
    {
		if(renderer is SkinnedMeshRenderer skinnedMeshRenderer&&skinnedMeshRenderer)
        {
			var result = new AdapterAndMesh();
			result.mesh = skinnedMeshRenderer.sharedMesh;
			result.adapter = new BzSliceSkinnedMeshAdapter(skinnedMeshRenderer);
			return result;
		}

		if (renderer is MeshRenderer meshRenderer && meshRenderer)
		{
			var result = new AdapterAndMesh
			{
				mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh
			};
			result.adapter = new BzSliceMeshFilterAdapter(result.mesh.vertices, meshRenderer);
			return result;
		}

		return null;
	}

	/// <summary>
	/// You need to override this to use your thead pool
	/// </summary>
	/// <param name="method">method that you need to call</param>
	/// <param name="obj">object that you need to pass to method</param>
	protected virtual void StartWorker(Action<object> method, object obj)
	{
		ThreadPool.QueueUserWorkItem(new WaitCallback(method), obj);
	}

	void WorkForWorker(object obj)
	{
		try
		{
			var sliceTry = (SliceTry)obj;
			Work(sliceTry);
			sliceTry.Finished = true;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogException(ex);
		}
	}

	void Work(SliceTry sliceTry)
	{
		bool somethingOnNeg = false;
		bool somethingOnPos = false;
		for (int i = 0; i < sliceTry.items.Length; i++)
		{
			var sliceTryItem = sliceTry.items[i];

			if (sliceTryItem == null)
				continue;

			var meshDissector = sliceTryItem.meshDissector;
			sliceTryItem.SliceResult = meshDissector.Slice();

			if (sliceTryItem.SliceResult == SliceResult.Neg |
				sliceTryItem.SliceResult == SliceResult.Duplicate |
				sliceTryItem.SliceResult == SliceResult.Sliced)
			{
				somethingOnNeg = true;
			}

			if (sliceTryItem.SliceResult == SliceResult.Pos |
				sliceTryItem.SliceResult == SliceResult.Duplicate |
				sliceTryItem.SliceResult == SliceResult.Sliced)
			{
				somethingOnPos = true;
			}
		}

		sliceTry.sliced = somethingOnNeg & somethingOnPos;

		if (sliceTry.sliced)
		{
			//sliceTry.sliceData.componentManager.OnSlicedWorkerThread(sliceTry.items);
		}

		OnSliceFinishedWorkerThread(sliceTry.sliced, sliceTry.sliceData.addData);
	}

	/// <summary>
	/// Called when sliced process finished. If the flag Asynchronously is true, then this method will also called asynchronously
	/// </summary>
	protected virtual void OnSliceFinishedWorkerThread(bool sliced, object addData) { }

	//void Update()
	//{
	//	Profiler.BeginSample("GetFinishedTask");
	//	var sliceTry = GetFinishedTask();
	//	Profiler.EndSample();

	//	if (sliceTry == null)
	//		return;

	//	Profiler.BeginSample("SliceTryFinished");
	//	SliceTryFinished(sliceTry);
	//	Profiler.EndSample();
	//}

	private void SliceTryFinished(SliceTry sliceTry)
	{
		BzSliceTryResult result = null;
		if (sliceTry.sliced)
		{
			Profiler.BeginSample("ApplyChanges");
			result = ApplyChanges(sliceTry);
			Profiler.EndSample();
		}

		if (result == null)
		{
			result = new BzSliceTryResult(false, sliceTry.sliceData.addData);
		}
		else
		{
			//lastSuccessfulSlice = sliceTry;
			//result.outObjectNeg.GetComponent<MeshSliceManager>().lastSuccessfulSlice = sliceTry;
			//result.outObjectPos.GetComponent<MeshSliceManager>().lastSuccessfulSlice = sliceTry;

			//Profiler.BeginSample("InvokeEvents");
			//InvokeEvents(result.outObjectNeg, result.outObjectPos);
			//Profiler.EndSample();
		}

		Profiler.BeginSample("OnSliceFinished");
		OnSliceFinished(result);
		Profiler.EndSample();

		if (result.sliced)
		{
			var runnerNeg = result.outObjectNeg.GetComponent<LazyActionRunner>();
			if (runnerNeg != null)
				runnerNeg.RunLazyActions();
			var runnerPos = result.outObjectPos.GetComponent<LazyActionRunner>();
			if (runnerPos != null)
				runnerPos.RunLazyActions();
		}

		if (sliceTry.callBack != null)
		{
			Profiler.BeginSample("CallBackMethod");
			sliceTry.callBack(result);
			Profiler.EndSample();
		}
	}

	//private void InvokeEvents(GameObject resultNeg, GameObject resultPos)
	//{
	//	var events = resultNeg.GetComponents<IBzObjectSlicedEvent>();
	//	for (int i = 0; i < events.Length; i++)
	//		events[i].ObjectSliced(CharacterRoot, resultNeg, resultPos);
	//}

	private BzSliceTryResult ApplyChanges(SliceTry sliceTry)
	{
		// duplicate object
		//GameObject resultObjNeg, resultObjPos;
		//GetNewObjects(out resultObjNeg, out resultObjPos);
		//var renderersNeg = GetRenderers(resultObjNeg);
		//var renderersPos = GetRenderers(resultObjPos);
		//if (useLazyRunner)
		//{
		//	resultObjNeg.AddComponent<LazyActionRunner>();
		//	resultObjPos.AddComponent<LazyActionRunner>();
		//}

		//if (renderersNeg.Length != renderersPos.Length |
		//	renderersNeg.Length != sliceTry.items.Length)
		//{
		//	// if something wrong happaned with object, and during slicing it was changed
		//	// reject this slice try
		//	return null;
		//}

		//Profiler.BeginSample("ComponentManager.OnSlicedMainThread");
		//sliceTry.sliceData.componentManager.OnSlicedMainThread(resultObjNeg, resultObjPos, renderersNeg, renderersPos);
		//Profiler.EndSample();

		BzSliceTryResult result = new BzSliceTryResult(true, sliceTry.sliceData.addData);
		result.meshItems = new BzMeshSliceResult[sliceTry.items.Length];

		for (int i = 0; i < sliceTry.items.Length; i++)
		{
			var sliceTryItem = sliceTry.items[i];
			if (sliceTryItem == null)
				continue;

			//var rendererNeg = renderersNeg[i];
			//var rendererPos = renderersPos[i];

			if (sliceTryItem.SliceResult == SliceResult.Sliced)
			{
				Neg=sliceTryItem.meshDissector.BuildNegMesh();
				Pos= sliceTryItem.meshDissector.BuildPosMesh();

				if (SaveMeshSide.HasFlag(Side.Left))
					SplitMeshs.Add(Neg);

				if (SaveMeshSide.HasFlag(Side.Right))
					SplitMeshs.Add(Pos);

				//negMesh.CombineMeshes()
				var itemResult = GetItemResult(sliceTryItem);//, rendererNeg, rendererPos);
				result.meshItems[i] = itemResult;
			}

            //if (sliceTryItem.SliceResult == SliceResult.Neg)
            //    DeleteRenderer(rendererPos);

            //if (sliceTryItem.SliceResult == SliceResult.Pos)
            //	DeleteRenderer(rendererNeg);
        }

	    

		result.outObjectNeg = CharacterRoot;
		result.outObjectPos = CharacterRoot;



		return result;
	}

	private static void DeleteRenderer(Renderer renderer)
	{
		if (renderer == null)
			return;

		var ob = renderer.gameObject;

		if (ob.TryGetComponent<MeshFilter>(out var mf))
		{
#if UNITY_EDITOR
			GameObject.DestroyImmediate(mf);
#else
			GameObject.Destroy(mf);
#endif
		}

#if UNITY_EDITOR
		GameObject.DestroyImmediate(renderer);
#else
			GameObject.Destroy(renderer);
#endif

	}

	/// <summary>
	/// Prepare data that will bu used for slicing
	/// </summary>
	/// <param name="plane"></param>
	protected virtual BzSliceTryData PrepareData(Plane plane)
	{
		// remember some date. Later we could use it after the slice is done.
		// here I add Stopwatch object to see how much time it takes
		ResultData addData = new ResultData();
		addData.stopwatch = System.Diagnostics.Stopwatch.StartNew();

        //// collider we want to participate in slicing
        //var collidersArr = GetComponentsInChildren<Collider>();

       // create component manager.
       //var componentManager = new mFirstPersonCharacterSliceManager(this.CharacterRoot, plane, collidersArr);

        return new BzSliceTryData()
		{
			//componentManager = componentManager,
			plane = plane,
			addData = addData,
		};
	}

	/// <summary>
	/// Called when sliced process finished
	/// </summary>
	protected virtual void OnSliceFinished(BzSliceTryResult result)
	{

	}

	public void Slice(Plane plane, Action<BzSliceTryResult> callBack)
	{
		if (this == null)  // if this component was destroied
			return;

		if (defaultSliceMaterial == null)
			throw new InvalidOperationException("DefaultSliceMaterial == null");

		var data = PrepareData(plane);
		if (data == null)
		{
			if (callBack != null)
				callBack(null);
			return;
		}

		//if (!data.componentManager.Success)
		//{
		//	if (callBack != null)
		//		callBack(new BzSliceTryResult(false, data.addData));
		//	return;
		//}

		StartSlice(data, null, callBack);
	}

	public void RepeatSlice(Plane plane, IBzSliceAdapter[] adapters)
	{
		var data = PrepareData(plane);
		if (data == null)
		{
			throw new InvalidOperationException("PrepareData returned null");
		}

		//if (!data.componentManager.Success)
		//{
		//	throw new InvalidOperationException("ComponentManager failure");
		//}

		var asyncEnabled = asynchronously;
		asynchronously = false;
		StartSlice(data, adapters, null);
		asynchronously = asyncEnabled;
	}

	private SliceTry GetFinishedTask()
	{
		if (_sliceTrys.Count == 0)
			return null;

		var sliceTry = _sliceTrys.Peek();

		if (sliceTry == null || !sliceTry.Finished)
			return null;

		_sliceTrys.Dequeue();

		return sliceTry;
	}

	private static BzMeshSliceResult GetItemResult(SliceTryItem sliceTryItem)/*, Renderer rendererNeg, Renderer rendererPos)*/
	{
        var itemResult = new BzMeshSliceResult
        {
            //rendererNeg = rendererNeg,
            //rendererPos = rendererPos
        };

        if (sliceTryItem.meshDissector.Configuration.CreateCap)
		{
			var sliceEdgeNegResult = new BzSliceEdgeResult[sliceTryItem.meshDissector.CapsNeg.Count];
			for (int i = 0; i < sliceEdgeNegResult.Length; i++)
			{
				var edgeResult = MakeEdgeResult(sliceTryItem.meshDissector.CapsNeg[i]);
				sliceEdgeNegResult[i] = edgeResult;
			}
			itemResult.sliceEdgesNeg = sliceEdgeNegResult;

			var sliceEdgePosResult = new BzSliceEdgeResult[sliceTryItem.meshDissector.CapsPos.Count];
			for (int i = 0; i < sliceEdgePosResult.Length; i++)
			{
				var edgeResult = MakeEdgeResult(sliceTryItem.meshDissector.CapsPos[i]);
				sliceEdgePosResult[i] = edgeResult;
			}
			itemResult.sliceEdgesPos = sliceEdgePosResult;
		}

		return itemResult;
	}

	private static BzSliceEdgeResult MakeEdgeResult(PolyMeshData polyMeshData)
	{
		var result = new BzSliceEdgeResult();
		result.vertices = polyMeshData.vertices;
		result.normals = polyMeshData.normals;
		result.boneWeights = polyMeshData.boneWeights;
		return result;
	}

	public override string ToString()
	{
		// prevent from accessing the name in debuge mode.
		return GetType().Name;
	}

	protected class AdapterAndMesh
	{
		public IBzSliceAdapter adapter;
		public Mesh mesh;
	}

	// Sample of data that can be attached to slice request.
	// In this the Stopwatch is used to time duration of slice operation.
	class ResultData
	{
		public System.Diagnostics.Stopwatch stopwatch;
	}

    public class SliceTryItem
    {
        public MeshDissector meshDissector;
        public Renderer meshRenderer;
        public SliceResult SliceResult;
    }

    public class SliceTry
    {
        public SliceTryItem[] items;
        bool _finished;
        public BzSliceTryData sliceData;
        public Action<BzSliceTryResult> callBack;
        public bool sliced;
        public Vector3 position;
        public Quaternion rotation;

        public bool Finished
        {
            get
            {
                Thread.MemoryBarrier();
                return _finished;
            }
            set
            {
                _finished = value;
                Thread.MemoryBarrier();
            }
        }
    }
}

#endif