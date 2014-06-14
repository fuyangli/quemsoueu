using UnityEngine;
using System.Reflection;


/// <summary>
/// Automatically provides a version number to a project and displays
/// it for 20 seconds at the start of the game.
/// </summary>
/// <remarks>
/// Change the first two number to update the major and minor version number.
/// The following number are the build number (which is increased automatically
///  once a day, and the revision number which is increased every second). 
/// </remarks>

// Version History
// 14.06.12.01 - Gabriel Guaitolini - Creation of Assembly Version
[assembly:AssemblyVersion ("14.06.12.01")]
public static class VersionNumber {
	/// <summary>
	/// Can be set to true, in that case the version number will be shown in bottom right of the screen
	/// </summary>
	public static bool ShowVersionInformation = false;
	/// <summary>
	/// Show the version during the first 20 seconds.
	/// </summary>
	public static bool ShowVersionDuringTheFirst20Seconds = true;
	static string version;
	static Rect position = new Rect (0, 0, 100, 20);
	
	/// <summary>
	/// Gets the version.
	/// </summary>
	/// <value>The version.</value>
	public static string Version {
		get {
			if (version == null) {
				version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			}
			return version;
		}
	}
}