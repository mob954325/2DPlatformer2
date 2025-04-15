using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

internal sealed class ScreencastMouse : MonoBehaviour
{
    #region " Inspector Variables "    		
    	
	[Header("References")]
	[Tooltip("Reference to an Image component in your scene window for the 'Mouse Base'.")]
	[SerializeField] private Image _mouseBase;
	
	[Tooltip("Reference to an Image component in your scene window for the 'Left Mouse Button'.")]
	[SerializeField] private Image _leftMouseButton;
	
	[Tooltip("Reference to an Image component in your scene window for the 'Middle Mouse Button'.")]
	[SerializeField] private Image _middleMouseButton;
	
	[Tooltip("Reference to an Image component in your scene window for the 'Right Mouse Button'.")]
	[SerializeField] private Image _rightMouseButton;
    	
	[Header("Settings")]
	[SerializeField] private ScreencastInputMouseSettingsSO _screencastInputSettings;
    
    #endregion   
    
	private void Update()
	{
		var lSettings = _screencastInputSettings;
		
		_mouseBase.sprite = lSettings.MouseBase;
		_leftMouseButton.sprite = lSettings.LeftButtonIdle;
		_middleMouseButton.sprite = lSettings.MiddleButtonIdle;
		_rightMouseButton.sprite = lSettings.RightButtonIdle;
		
#if ENABLE_LEGACY_INPUT_MANAGER
		// Old system input backends are enabled.				
		
		if (Input.GetMouseButton(0))
		_leftMouseButton.sprite = _screencastInputSettings.LeftButtonPress;
			
		if (Input.GetMouseButton(1))
		_rightMouseButton.sprite = _screencastInputSettings.RightButtonPress;
			
		if (Input.GetMouseButton(2))
		_middleMouseButton.sprite = _screencastInputSettings.MiddleButtonPress;	
#endif

#if ENABLE_INPUT_SYSTEM
		// New input system backends are enabled.
				
		if (Mouse.current.leftButton.isPressed)
			_leftMouseButton.sprite = _screencastInputSettings.LeftButtonPress;
			
		if (Mouse.current.rightButton.isPressed)
			_rightMouseButton.sprite = _screencastInputSettings.RightButtonPress;
			
		if (Mouse.current.middleButton.isPressed)
			_middleMouseButton.sprite = _screencastInputSettings.MiddleButtonPress;
#endif	
	}
}
