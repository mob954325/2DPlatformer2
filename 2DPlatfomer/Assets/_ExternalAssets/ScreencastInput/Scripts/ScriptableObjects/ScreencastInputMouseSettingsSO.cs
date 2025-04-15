using UnityEngine;

[CreateAssetMenu(fileName = "Screencast Input Mouse Settings", menuName = "Screencast Input/Screencast Input Mouse Settings")]
public class ScreencastInputMouseSettingsSO : ScriptableObject
{
	[Header("Mouse Base")]
	[Tooltip("Sprite displayed for the 'Mouse Base'.")]
	public Sprite MouseBase;
	
	[Header("Left Mouse Button")]
	[Tooltip("Sprite displayed for the 'Left Mouse Button' when idle.")]
	public Sprite LeftButtonIdle;
	
	[Tooltip("Sprite displayed for the 'Left Mouse Button' when pressed.")]
	public Sprite LeftButtonPress;
	
	[Header("Middle Mouse Button")]
	[Tooltip("Sprite displayed for the 'Middle Mouse Button' when idle.")]
	public Sprite MiddleButtonIdle;
	
	[Tooltip("Sprite displayed for the 'Middle Mouse Button' when pressed.")]
	public Sprite MiddleButtonPress;
	
	[Header("Right Mouse Button")]
	[Tooltip("Sprite displayed for the 'Right Mouse Button' when idle.")]
	public Sprite RightButtonIdle;
	
	[Tooltip("Sprite displayed for the 'Right Mouse Button' when pressed.")]
	public Sprite RightButtonPress;
}
