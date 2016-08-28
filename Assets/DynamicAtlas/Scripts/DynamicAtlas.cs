/*
 	Based on the Public Domain MaxRectsBinPack.cs source by Sven Magnus
	http://wiki.unity3d.com/index.php/MaxRectsBinPack
 
 	This wrapper by Yuri Beketov
 	for any questions email me beketovman@gmail.com
 	
	This wrapper also public domain.

	DynamicAtlas v.1.0
*/
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using HeuristicMethod = MaxRectsBinPack.FreeRectChoiceHeuristic;

[System.Serializable]
public class DynamicAtlas {

	/// <summary>
	/// Gets main texture atlas.
	/// </summary>
	/// <value>The texture.</value>
	public Texture2D Texture { get; private set; }

	/// <summary>
	/// Gets the main texture atlas name.
	/// </summary>
	/// <value>The name.</value>
	public string Name { get { return Texture.name; } }

	/// <summary>
	/// Gets the main texture atlas size.
	/// </summary>
	/// <value>The rect.</value>
	public Rect Rect { get { return new Rect(0, 0, Texture.width, Texture.height); } }

	/// <summary>
	/// Gets a value indicating whether this atlas is applied.
	/// </summary>
	/// <value><c>true</c> if this atlas is applied; otherwise, <c>false</c>.</value>
	public bool IsApplied { get; private set; }

	/// <summary>
	/// Gets the pack method.
	/// </summary>
	/// <value>The method.</value>
	public HeuristicMethod Method { get { return method; } }

	/// <summary>
	/// Gets occupancy in the atlas.
	/// </summary>
	/// <value>The occupancy.</value>
	public float Occupancy { get { return rectsPack.Occupancy(); } }

	/// <summary>
	/// Gets the lenght of resources in the atlas.
	/// </summary>
	/// <value>The lenght.</value>
	public int Lenght { get { return rectsPack.usedRectangles.Count; } }

	[SerializeField]
	HeuristicMethod method;
	[SerializeField]
	MaxRectsBinPack rectsPack;
	[SerializeField]
	List<string> names;

	public DynamicAtlas(int size, string name) : this(width: size, height: size, name: name) {
	}

	public DynamicAtlas(int width, int height, string name, HeuristicMethod method = HeuristicMethod.RectBestShortSideFit) {
		Texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
		Texture.name = name;
		this.method = method;

		rectsPack = new MaxRectsBinPack(width, height, false);
		names = new List<string>();
	}

	#region Write

	/// <summary>
	/// Insert the source (texture) in atlas.
	/// </summary>
	/// <param name="source">Texture2D</param>
	/// <returns>Return true if inserted else false. </returns>
	public bool Insert(Texture2D source) {
		return Write(source);
	}

	/// <summary>
	/// Actually apply all previous Insert changes.
	/// </summary>
	public void Apply() {
		Texture.Apply();
		IsApplied = true;
	}

	bool Write(Texture2D source) {
		Rect newRect = rectsPack.Insert(source.width, source.height, method);

		if (newRect.height == 0)
			return false;

		names.Add(source.name);

		Color[] colors = source.GetPixels();
		Texture.SetPixels((int)newRect.x, (int)newRect.y, (int)newRect.width, (int)newRect.height, colors);

		IsApplied = false;

		return true;
	}

	#endregion

	#region Read

	/// <summary>
	/// Get the source from atlas by name.
	/// Automatically applies all previous Insert changes.
	/// </summary>
	/// <param name="name">Source name.</param>
	/// <returns>Return SourceInfo if source founded else null. </returns>
	public SourceInfo Get(string name) {
		int index = names.FindIndex(findName => findName == name);
        
		if (index == -1)
			return null;
		
		return Get(index);
	}

	public SourceInfo this [int index] {
		get { return Get(index); }
	}

	/// <summary>
	/// Get the source from atlas by index.
	/// Automatically applies all previous Insert changes.
	/// </summary>
	/// <param name="index">Source index.</param>
	/// <returns>Return SourceInfo.</returns>
	public SourceInfo Get(int index) {
		if (IsApplied == false)
			Apply();

		return new SourceInfo(Texture, names[index], rectsPack.usedRectangles[index]);
	}

	#endregion

	#region File

	/// <summary>
	/// Save to disk the DynamicAtlas.
	/// </summary>
	/// <param name="atlas">DynamicAtlas.</param>
	/// <param name="info">Custom FileInfo.</param>
	/// <returns>Return FileInfo (Path and Name of saved atlas).</returns>
	public static FileInfo Save(DynamicAtlas atlas, FileInfo info = null) {
		if (info == null)
			info = new FileInfo(atlas.Texture.name);

		if (Directory.Exists(info.Path) == false)
			Directory.CreateDirectory(info.Path);

		if (atlas.IsApplied == false)
			atlas.Apply();

		byte[] bytes = atlas.Texture.EncodeToPNG();
		string json = JsonUtility.ToJson(atlas);

		File.WriteAllBytes(info.PathTexture, bytes);
		File.WriteAllText(info.PathData, json);

		return info;
	}

	/// <summary>
	/// Load from disk the DynamicAtlas.
	/// </summary>
	/// <param name="info">FileInfo.</param>
	/// <returns>Return DynamicAtlas.</returns>
	public static DynamicAtlas Load(FileInfo info) {
		if (File.Exists(info.PathTexture) == false || File.Exists(info.PathData) == false)
			return null;
		
		byte[] bytes = File.ReadAllBytes(info.PathTexture);
		string json = File.ReadAllText(info.PathData);

		DynamicAtlas atlas = JsonUtility.FromJson<DynamicAtlas>(json);
		atlas.Texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
		atlas.Texture.LoadImage(bytes);
		atlas.Texture.name = info.Name;

		return atlas;
	}

	/// <summary>
	/// Delete the DynamicAtlas on disk by FileInfo.
	/// </summary>
	/// <param name="info">FileInfo.</param>
	/// <returns>Return true if has been deleted else false.</returns>
	public static bool Delete(FileInfo info) {
		if (File.Exists(info.PathTexture) == false && File.Exists(info.PathData) == false)
			return false;

		File.Delete(info.PathTexture);
		File.Delete(info.PathData);

		return true;
	}

	#endregion

	[System.Serializable]
	public class FileInfo {

		static string defaultPath = Application.persistentDataPath + "/DynamicAtlases/";
		static string extensionTexture = ".png", extentionData = ".json";
		
		[SerializeField]
		string name;
		[SerializeField]
		string path;

		public string Name { get { return name; } }

		public string Path { get { return path; } }

		public string PathTexture { get { return Path + Name + extensionTexture; } }

		public string PathData { get { return Path + Name + extentionData; } }

		public FileInfo(string name, string path = null) {
			this.name = name;
			this.path = (string.IsNullOrEmpty(path)) ? defaultPath : path;
		}
	}

	public class SourceInfo {

		public string Name { get; private set; }

		public Rect Rect { get; private set; }

		Texture2D texture;

		public SourceInfo(Texture2D texture, string name, Rect rect) {
			this.texture = texture;
			Name = name;
			Rect = rect;
		}

		public Sprite GetSprite(Vector2 pilot) {
			Sprite sprite = UnityEngine.Sprite.Create(texture, Rect, pilot);
			sprite.name = Name;

			return sprite;
		}

		public Sprite GetSprite() {
			return GetSprite(new Vector2(0.5f, 0.5f));
		}
	}
}