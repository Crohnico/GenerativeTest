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
	public class PX_WeaponHolder : MonoBehaviour
	{
		[Header("Player-X [Weapon Holder]")]
		
		[Space]
		
		[Header("- Weapon Holder Dependencies")]
		public PX_Dependencies dependencies;
		
		//- Hidden Variables
		
		[HideInInspector]
		public float 
		attackForce = 100f;
		
		[HideInInspector]
		public Rigidbody weaponPhysics;
		
		[HideInInspector]
		public Transform attackPoint;
		
		[HideInInspector]
		public AudioSource weaponAudioSource;
		
		[HideInInspector]
		public AudioClip[] attackSounds;
		
		
		
		void Awake()
		{
			//... Set default attack attributes (Fist Punching)
			attackForce = dependencies.controller.punchForce;
			weaponPhysics = GetComponent<Rigidbody>();
			attackPoint = transform;
			weaponAudioSource = this.GetComponent<AudioSource>();
			attackSounds = dependencies.sound.attackSounds;
		}
	}
}
