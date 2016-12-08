using UnityEngine;
using System.Collections;
using UnityEditor;

//[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor {

	private SerializedObject _manager;
	private SerializedProperty keyNewLine;
	private SerializedProperty trainCreationMode;
	private SerializedProperty boardWidth;
	private SerializedProperty boardWidthEffective;
	private SerializedProperty boardLong;

	
	private SerializedProperty grass;
	private SerializedProperty deepGrass;
	private SerializedProperty grassChance;
	private SerializedProperty minLongGrass;
	private SerializedProperty maxLongGrass;
	private SerializedProperty tree;
	private SerializedProperty maxTreePerLine;

	private string arrayTreeSizePath = "tree.Array.size";
	private string arrayTreeData = "tree.Array.data[{0}]";

	private GameObject[] GetArray(string arraySizePath, string arrayData){
		var arrayCount = _manager.FindProperty(arraySizePath).intValue;
		var gameObjectArray = new GameObject[arrayCount];

		for(var i = 0; i < arrayCount; i++){
			gameObjectArray[i] = _manager.FindProperty(string.Format(arrayData, i)).objectReferenceValue as GameObject;
		}

		return gameObjectArray;
	}

	private void SetGameObjectArrayAtIndex(string arrayData ,int index, GameObject go){
		_manager.FindProperty(string.Format(arrayData, index)).objectReferenceValue = go;
	}

	public void OnEnable(){
		_manager = new SerializedObject(target);

		keyNewLine = _manager.FindProperty("pressNForNewLine");
		trainCreationMode = _manager.FindProperty("trainCreationMode");
		boardWidth = _manager.FindProperty("boardWidth");
		boardWidthEffective = _manager.FindProperty("boardWidthEffective");
		boardLong = _manager.FindProperty("longPerBoard");
		grass = _manager.FindProperty("grass");
		deepGrass = _manager.FindProperty("deepGrass");
		grassChance = _manager.FindProperty("grassChance");
		minLongGrass = _manager.FindProperty("minLongGrass");
		maxLongGrass = _manager.FindProperty("maxLongGrass");
		tree = _manager.FindProperty("tree");
		maxTreePerLine = _manager.FindProperty("maxTreePerLine");
	}

	public override void OnInspectorGUI(){

		EditorGUILayout.PropertyField(keyNewLine);
		EditorGUILayout.PropertyField(trainCreationMode);
		EditorGUILayout.PropertyField(boardWidth);
		EditorGUILayout.PropertyField(boardWidthEffective);
		EditorGUILayout.PropertyField(boardLong);
		EditorGUILayout.PropertyField(grass);
		EditorGUILayout.PropertyField(deepGrass);
		EditorGUILayout.PropertyField(grassChance);
		EditorGUILayout.PropertyField(minLongGrass);
		EditorGUILayout.PropertyField(maxLongGrass);
		EditorGUILayout.PropertyField(tree);
		EditorGUILayout.PropertyField(maxTreePerLine);
	}
}
