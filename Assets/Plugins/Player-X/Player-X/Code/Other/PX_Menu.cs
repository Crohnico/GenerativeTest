//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerX;

namespace PlayerX
{
	public class PX_Menu : MonoBehaviour
	{
		[Header("Player-X [Menu]")]
		
		[Space]
		
		[Header("- Button Indicators")]
		public GameObject indicatorDemo1;
		public GameObject indicatorDemo2;
		public GameObject indicatorDemo3;
		public GameObject indicatorDemo4;
		public GameObject indicatorExit;
		
		[Header("- Audio")]
		public AudioSource buttonSound;
		
		[Header("- Head Track Animation")]
		public Transform trackPoint;
		public Transform headTracker;
		public ConfigurableJoint headJoint;
		
		Quaternion headTrackInversedRot;
		
		
		//... Cursor
	    void Start()
	    {
	        Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
			
			//... Store head tracker start local rotation
			headTrackInversedRot = Quaternion.Inverse(headTracker.localRotation);
	    }
		
		//... Head Track Animation
		void FixedUpdate()
		{
			headTracker.transform.LookAt(trackPoint.position);
			headJoint.targetRotation = Quaternion.Slerp(headJoint.targetRotation, Quaternion.Inverse(headTracker.localRotation) * headTrackInversedRot, 100 * Time.fixedDeltaTime);
		}
		
		
		
		
		//... Indicator Demo 1 button
		public void indicateDemo1ButtonOn()
		{
			indicatorDemo1.SetActive(true);
			buttonSound.Play();
		}
		
		public void indicateDemo1ButtonOff()
		{
			indicatorDemo1.SetActive(false);
		}
		
		
		
		
		//... Indicator Demo 2 button
		public void indicateDemo2ButtonOn()
		{
			indicatorDemo2.SetActive(true);
			buttonSound.Play();
		}
		
		public void indicateDemo2ButtonOff()
		{
			indicatorDemo2.SetActive(false);
		}
		
		
		
		
		//... Indicator Demo 3 button
		public void indicateDemo3ButtonOn()
		{
			indicatorDemo3.SetActive(true);
			buttonSound.Play();
		}
		
		public void indicateDemo3ButtonOff()
		{
			indicatorDemo3.SetActive(false);
		}
		
		
		
		
		//... Indicator Demo 4 button
		public void indicateDemo4ButtonOn()
		{
			indicatorDemo4.SetActive(true);
			buttonSound.Play();
		}
		
		public void indicateDemo4ButtonOff()
		{
			indicatorDemo4.SetActive(false);
		}
		
		
		
		
		//... Indicator exit button
		public void indicateExitOn()
		{
			indicatorExit.SetActive(true);
			buttonSound.Play();
		}
		
		public void indicateExitOff()
		{
			indicatorExit.SetActive(false);
		}
	    
		
		
		
		//... Load demo 1
	    public void LoadDemoGame1()
	    {
	        SceneManager.LoadScene(1);
	    }
		
		//... Load demo 2
	    public void LoadDemoGame2()
	    {
	        SceneManager.LoadScene(2);
	    }
		
		//... Load demo 3
	    public void LoadDemoGame3()
	    {
	        SceneManager.LoadScene(3);
	    }
		
		//... Load demo 4
	    public void LoadDemoGame4()
	    {
	        SceneManager.LoadScene(4);
	    }


	    
		//... Exit
	    public void ExitGame()
	    {
	        Application.Quit();
	    }
	}
}
