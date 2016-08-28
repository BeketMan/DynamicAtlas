using UnityEngine;
using System.Collections;
using HeuristicMethod = MaxRectsBinPack.FreeRectChoiceHeuristic;

public class Example : MonoBehaviour {

	public int sizeAtlas;
	public string nameAtlas;
	public HeuristicMethod method;
	public Texture2D[] sources;
	public SpriteRenderer previewAtlas, previewSource;

	DynamicAtlas atlas;
	string KeyPlayerPrefs = "ExampleAtlasPathInfo";

	int indexShowed = 0;

	public void Create() {
		atlas = new DynamicAtlas(sizeAtlas, sizeAtlas, nameAtlas, method);
		// or
		// atlas = new DynamicAtlas(sizeAtlas, sizeAtlas, nameAtlas);
		// or
		// atlas = new DynamicAtlas(sizeAtlas, nameAtlas);

		Debug.Log(string.Format("Create atlas: {0}, method: {1}", atlas.Name, atlas.Method));

		Show();
	}

	void Show() {
		previewAtlas.sprite = Sprite.Create(atlas.Texture, atlas.Rect, new Vector2(0.5f, 0.5f));
		previewAtlas.sprite.name = atlas.Name;
	}

	public void Apply() {
		if (AtlasIsNull())
			return;

		atlas.Apply();
	}

	public void Add() {
		if (AtlasIsNull())
			return;

		if (atlas.Lenght >= sources.Length) {
			Debug.Log("You insert all array resources");
			return;
		}

		bool isInsert = atlas.Insert(sources[atlas.Lenght]);

		string strIsInsert = (isInsert) ? "insert" : "NOT insert (not enough space in atlas)";
		Debug.Log(string.Format("Sourse {0} {1} - Occupancy {2}", sources[atlas.Lenght - 1].name, strIsInsert, atlas.Occupancy));
	}

	public void ShowNext() {
		if (AtlasIsNull() || atlas.Lenght == 0)
			return;

		if (indexShowed >= atlas.Lenght)
			indexShowed = 0;
		
		var infoSource = atlas[indexShowed++];
		// or
		// var infoSource = atlas.Get(indexShowed++);
		// or
		// var infoSource = atlas.Get(sources[indexShowed++].name);

		previewSource.sprite = infoSource.GetSprite();

		Debug.Log(string.Format("Get sprite: {0}, rect: {1}", infoSource.Name, infoSource.Rect));
	}

	IEnumerator CoroutineShowSources() {
		for (int i = 0; i < atlas.Lenght; i++) {
			var infoSource = atlas[i];
			previewSource.sprite = infoSource.GetSprite();

			Debug.Log(string.Format("Get sprite: {0}, rect: {1}", infoSource.Name, infoSource.Rect));
            
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void SaveOnDisk() {
		if (AtlasIsNull())
			return;

		DynamicAtlas.FileInfo info = DynamicAtlas.Save(atlas);

		string json = JsonUtility.ToJson(info);
		Debug.Log("Save complite, infoFile: " + json);

		PlayerPrefs.SetString(KeyPlayerPrefs, json);
		PlayerPrefs.Save();
	}

	public void LoadFromDisk() {
		string json = PlayerPrefs.GetString(KeyPlayerPrefs);

		if (string.IsNullOrEmpty(json)) {
			Debug.Log("FileInfo is not exists in PlayerPrefs!");
			return;
		}

		DynamicAtlas.FileInfo info = JsonUtility.FromJson<DynamicAtlas.FileInfo>(json);

		DynamicAtlas loadAtlas = DynamicAtlas.Load(info);

		if (loadAtlas == null) {
			Debug.Log(string.Format("Load atlas {0} is not exists!", info.Name));
			return;
		}

		atlas = loadAtlas;
		Show();

		Debug.Log(string.Format("Load atlas name: {0}, method: {1}", atlas.Name, atlas.Method));
	}

	public void DeleteFromDisk() {
		string json = PlayerPrefs.GetString(KeyPlayerPrefs);

		if (string.IsNullOrEmpty(json)) {
			Debug.Log("FileInfo is not exists in PlayerPrefs!");
			return;
		}

		DynamicAtlas.FileInfo info = JsonUtility.FromJson<DynamicAtlas.FileInfo>(json);

		bool isDelete = DynamicAtlas.Delete(info);

		if (isDelete == false) {
			Debug.Log(string.Format("Atlas {0} is not exists!", info.Name));
			return;
		}

		PlayerPrefs.DeleteKey(KeyPlayerPrefs);
		PlayerPrefs.Save();

		Debug.Log(string.Format("Delete atlas: {0}", atlas.Name));
	}

	bool AtlasIsNull() {
		if (atlas == null) {
			Debug.Log("Atlas is null, first create or load atlas.");
			return true;
		}
		return false;
	}
}
