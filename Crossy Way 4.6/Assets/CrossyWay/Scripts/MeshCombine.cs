using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MeshCombine : MonoBehaviour {

	// Use this for initialization
/*
	void Start () {
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach (var meshRenderer in meshRenderers)
		{
			foreach (var material in meshRenderer.sharedMaterials)
				if (material != null && !combines.ContainsKey(material))
					combines.Add(material, new List<CombineInstance>());
		}
		
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		foreach(var filter in meshFilters)
		{
			if (filter.sharedMesh == null)
				continue;
			var filterRenderer = filter.GetComponent<Renderer>();
			if (filterRenderer.sharedMaterial == null)
				continue;
			if (filterRenderer.sharedMaterials.Length > 1)
				continue;
			CombineInstance ci = new CombineInstance
			{
				mesh = filter.sharedMesh,
				transform = myTransform*filter.transform.localToWorldMatrix
			};
			combines[filterRenderer.sharedMaterial].Add(ci);
			
			Destroy(filterRenderer);
		}
		
		foreach(Material m in combines.Keys)
		{
			var go = new GameObject("Combined mesh");
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			
			var filter = go.AddComponent<MeshFilter>();
			filter.mesh.CombineMeshes(combines[m].ToArray(), true, true);
			
			var arenderer = go.AddComponent<MeshRenderer>();
			arenderer.material = m;
		}
	}*/

	public static void CreatePrefab(GameObject go, string pathFolder, string nameFolder, string tag){
		#if UNITY_EDITOR
		Mesh mesh = CombineMeshes(go);
		
		string fullPath = pathFolder+"/"+nameFolder+"/"+go.name;
		
		AssetDatabase.CreateFolder(pathFolder+"/"+nameFolder, go.name); 
		
		AssetDatabase.CreateFolder(fullPath, "MeshData");
		AssetDatabase.CreateFolder(fullPath, "Material");
		
		AssetDatabase.CreateAsset(mesh, fullPath+"/MeshData/"+go.name+"_cmplt.asset");
		
		MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
		List<Material> mats = new List<Material>();
		foreach(MeshRenderer mr in meshRenderers){
			foreach(Material mat in mr.materials){
				if(mats.Count == 0){
					mats.Add(mat);
				}
				else{
					foreach(Material m in mats){
						if(m == mat)
							goto skipMat;
					}
					mats.Add(mat);
				skipMat:
						Debug.Log("Ignore this message. Same Material");
				}
			}
		}
		
		
		foreach(Material mat in mats.ToArray()){
			AssetDatabase.CreateAsset(mat, fullPath+"/Material/"+mat.name.Remove(mat.name.Length - 11)+"_cmplt.asset");
		}
		
		
		GameObject obj = new GameObject(go.name);
		obj.AddComponent<MeshFilter>();
		obj.AddComponent<MeshRenderer>();
		
		obj.GetComponent<MeshFilter>().mesh = mesh;
		obj.GetComponent<MeshRenderer>().materials = mats.ToArray();
		
		obj.AddComponent<MeshCollider>();
		
		//This so heavy
		UnityEngine.Object dummyPrefab = PrefabUtility.CreateEmptyPrefab(fullPath+"/"+go.name+"_cmplt.prefab");
		GameObject prefab = PrefabUtility.ReplacePrefab(obj, dummyPrefab, ReplacePrefabOptions.ConnectToPrefab) as GameObject;
		prefab.tag = tag;
		//Heavy part end
		
		Destroy(obj);
		#endif
	}

	public static void CreatePrefab(GameObject go, string pathFolder, string nameFolder){
#if UNITY_EDITOR	
		CreatePrefab(go, pathFolder, nameFolder, "Untagged");
#endif
	}

	public static Mesh CombineMeshes(GameObject aGo) {
		
		#if UNITY_EDITOR
		MeshRenderer[] meshRenderers = aGo.GetComponentsInChildren<MeshRenderer>(false);
		int totalVertexCount = 0;
		int totalMeshCount = 0;
		
		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					totalVertexCount += filter.sharedMesh.vertexCount;
					totalMeshCount++;
				}
			}
		}
		
		if(totalMeshCount == 0) {
			Debug.LogWarning("No meshes found in children. There's nothing to combine.");
			return null;
		}
		if(totalMeshCount == 1) {
			Debug.LogWarning("Only 1 mesh found in children. There's nothing to combine.");
			return null;
		}
		if(totalVertexCount > 65535) {
			Debug.LogError("There are too many vertices to combine into 1 mesh ("+totalVertexCount+"). The max. limit is 65535");
			return null;
		}
#endif
		Mesh mesh = new Mesh();
		
		#if UNITY_EDITOR
		Matrix4x4 myTransform = aGo.transform.worldToLocalMatrix;
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uv1s = new List<Vector2>();
		List<Vector2> uv2s = new List<Vector2>();
		Dictionary<Material, List<int>> subMeshes = new Dictionary<Material, List<int>>();
		
		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					MergeMeshInto(filter.sharedMesh, meshRenderer.sharedMaterials, myTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s, subMeshes);
					if(filter.gameObject != aGo) {
						filter.gameObject.SetActive(false);
					}
				}
			}
		}
		
		mesh.vertices = vertices.ToArray();
		if(normals.Count>0) mesh.normals = normals.ToArray();
		if(uv1s.Count>0) mesh.uv = uv1s.ToArray();
		if(uv2s.Count>0) mesh.uv2 = uv2s.ToArray();
		mesh.subMeshCount = subMeshes.Keys.Count;
		Material[] materials = new Material[subMeshes.Keys.Count];
		int mIdx = 0;
		foreach(Material m in subMeshes.Keys) {
			materials[mIdx] = m;
			mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
		}
		
		if(meshRenderers != null && meshRenderers.Length > 0) {
			MeshRenderer meshRend = aGo.GetComponent<MeshRenderer>();
			if(meshRend == null) meshRend = aGo.AddComponent<MeshRenderer>();
			meshRend.sharedMaterials = materials;
			
			MeshFilter meshFilter = aGo.GetComponent<MeshFilter>();
			if(meshFilter == null) meshFilter = aGo.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
		}
#endif
		return mesh;

	}

	private static void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes) {
		
		#if UNITY_EDITOR
		if(meshToMerge == null) return;
		int vertexOffset = vertices.Count;
		Vector3[] vs = meshToMerge.vertices;
		
		for(int i=0;i<vs.Length;i++) {
			vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);
		}
		vertices.AddRange(vs);
		
		Quaternion rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
		Vector3[] ns = meshToMerge.normals;
		if(ns!=null && ns.Length>0) {
			for(int i=0;i<ns.Length;i++) ns[i] = rotation * ns[i];
			normals.AddRange(ns);
		}
		
		Vector2[] uvs = meshToMerge.uv;
		if(uvs!=null && uvs.Length>0) uv1s.AddRange(uvs);
		uvs = meshToMerge.uv2;
		if(uvs!=null && uvs.Length>0) uv2s.AddRange(uvs);
		
		for(int i=0;i<ms.Length;i++) {
			if(i<meshToMerge.subMeshCount) {
				int[] ts = meshToMerge.GetTriangles(i);
				if(ts.Length>0) {
					if(ms[i]!=null && !subMeshes.ContainsKey(ms[i])) {
						subMeshes.Add(ms[i], new List<int>());
					}
					List<int> subMesh = subMeshes[ms[i]];
					for(int t=0;t<ts.Length;t++) {
						ts[t] += vertexOffset;
					}
					subMesh.AddRange(ts);
				}
			}
		}
#endif
	}
}
