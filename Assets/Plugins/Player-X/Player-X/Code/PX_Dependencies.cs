//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_Dependencies : MonoBehaviour
	{
		[Header("Player-X [Dependencies]")]
		
		[Space]
		
		public PX_Inputs inputs;
		public PX_Camera playerCamera;
	    public PX_State state;
		public PX_Player player;
		public PX_Controller controller;
		public PX_Procedural procedural;
		public PX_Health health;
		public PX_Dismemberment dismember;
		public PX_Weapons weapons;
		public PX_OutOfBounds outOfBound;
		public PX_Sound sound;
	}
}
