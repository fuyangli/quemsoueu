using UnityEngine;
using System.Reflection;


// Version History
// 14.06.16.01 - Gabriel Guaitolini - Melhoria de Cores para Jogadores
// 14.06.12.01 - Gabriel Guaitolini - Creation of Assembly Version
[assembly:AssemblyVersion ("14.06.16.01")]
public static class VersionNumber {
	/// <summary>
	/// Can be set to true, in that case the version number will be shown in bottom right of the screen
	/// </summary>
	public static bool ShowVersionInformation = false;
	/// <summary>
	/// Show the version during the first 20 seconds.
	/// </summary>
	public static bool ShowVersionDuringTheFirst20Seconds = true;
	static string _version;
	static Rect _position = new Rect (0, 0, 100, 20);
	
	/// <summary>
	/// Gets the version.
	/// </summary>
	/// <value>The version.</value>
	public static string Version {
		get { return _version ?? (_version = Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
	}
}