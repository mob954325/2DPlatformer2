using UnityEngine;

[CreateAssetMenu(fileName = "Screencast Input Text Settings", menuName = "Screencast Input/Screencast Input Text Settings")]
public class ScreencastInputTextSettingsSO : ScriptableObject
{
	[Header("Display Settings")]
	[Tooltip("Duration of seconds in which input keys will be displayed.")]
	public float DisplayTime = 3f;

	[Header("Log Settings")]
	[Tooltip("Indicates whether input should be logged to a text file.")]
	public bool LogEnabled = false;
}
