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
	public class PX_WeaponAttributes : MonoBehaviour
	{
		[Header("Player-X [Weapon Attributes]")]
		
		[Space]
		
		[Header("- Melee Attack")]
		public bool isBlade = false;
	    public float attackForce;
		
		[Header("- Melee References")]
		public Transform attackPoint;
		public Rigidbody weaponPhysics;
		
		[Header("- Melee Damage")]
		public float impactRequiredDamage = 50f;
		public float headHitDamage = 20;
		public float bodyHitDamage = 10f;
		public float limbHitDamage = 5f;
		
		[Header("- Melee Audio")]
		public AudioClip[] attackSounds;
		
		[Header("- Piercing")]
		public bool canPierce = false;
		public float pierceForceRequired = 25f;
		public float breakForceRequired = 30000f;
		public GameObject piercePlayerParticle;
		public GameObject pierceObjectParticle;
		
		[Header("- Gun")]
		public bool isGun = false;
		public bool burst = false;
		public float fireRate = 0.25f;
		public float recoilForce = 10f;
		public float hitForce = 100f;
		public GameObject shell;
		
		[Header("- Shoot References")]
		public Transform shootPoint;
		public Transform shellPoint;
		
		[Header("- Bullet Damage")]
		public float headShotDamage = 100f;
		public float bodyShotDamage = 30f;
		public float limbShotDamage = 10f;
		
		[Header("- Particles")]
		public GameObject muzzleParticle;
		public GameObject hitPlayerParticle;
		public GameObject hitObjectParticle;
		public GameObject headShotParticle;
		
		[Header("- Handle Colliders")]
		public Collider[] weaponHandleColliders;
		
		[Header("- Gun Audio")]
		public AudioClip[] gunSounds;
		public AudioClip bulletHitPlayerSound;
		public AudioClip bulletHitObjectSound;
		
		//- Hidden Weapon Attribute variable
		
		[HideInInspector]
		public bool 
		weaponAssigned = false,
		pierced = false;
		
		[HideInInspector]
		public Transform weaponContainer;
		Transform weaponPrevParent;
		
		LayerMask thisWeaponLayerRecord;
		
		
		//... Record this weapon's layer
		void Start()
		{
			thisWeaponLayerRecord = this.gameObject.layer;
		}
		
		
		
		//... Pierce on collision if enabled
		void OnCollisionEnter(Collision col)
		{
			var impactMagnitude = col.relativeVelocity.magnitude;
			
			//... Sever if weapon is blade
			if(isBlade && impactMagnitude >= impactRequiredDamage && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject)
			{
				if(col.gameObject.GetComponent<PX_ImpactDetect>() != null)
				{
					col.gameObject.GetComponent<PX_ImpactDetect>().DismemberOnCommand();
				}
			}
			
			//... Deduct health
			else if(impactMagnitude >= impactRequiredDamage && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject)
			{
				if(col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>() != null)
				{
					//... Head hit
					if(col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.headPhysics.gameObject)
					{
						col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= headHitDamage;
					}
					
					//... Body hit
					if(col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.rootPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyLowerPhysics.gameObject  
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyUpperPhysics.gameObject )
					{
						col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= bodyHitDamage;
					}
					
					//... Limb hit
					if(col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armLeftPhysics.gameObject  
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armRightPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handLeftPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handRightPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legLeftPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legRightPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footLeftPhysics.gameObject 
					|| col.gameObject == col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footRightPhysics.gameObject)
					{
						col.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= limbHitDamage;
					}
				}
			}
			
			//... Piercing
			if(canPierce && !pierced)
			{
				if(impactMagnitude >= pierceForceRequired && col.gameObject.GetComponent<Rigidbody>() != null && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject
				&& (col.gameObject.layer == LayerMask.NameToLayer("Player-X") || col.gameObject.layer == LayerMask.NameToLayer("Player-X (Other)") || col.gameObject.layer == LayerMask.NameToLayer("Objects")))
				{
					pierced = true;
					
					//... Create joint
					weaponPhysics.gameObject.AddComponent<FixedJoint>().connectedBody = col.gameObject.GetComponent<Rigidbody>();
					weaponPhysics.gameObject.GetComponent<FixedJoint>().enablePreprocessing = false;
					
					weaponPrevParent = transform.parent;
					transform.parent = col.gameObject.transform;
					
					if(col.gameObject.layer == LayerMask.NameToLayer("Player-X") || col.gameObject.layer == LayerMask.NameToLayer("Player-X (Other)"))
					{
						//... Pierce particle
						var stabParticle = Instantiate(piercePlayerParticle, col.gameObject.transform.position, Quaternion.identity);
						
						stabParticle.transform.parent = col.gameObject.transform;
						stabParticle.transform.localScale = new Vector3(1, 1, 1);
					}
					
					else if(col.gameObject.layer == LayerMask.NameToLayer("Objects"))
					{
						//... Pierce particle
						var stabParticle = Instantiate(pierceObjectParticle, attackPoint.position, Quaternion.identity);
						
						stabParticle.transform.parent = col.gameObject.transform;
						stabParticle.transform.localScale = new Vector3(1, 1, 1);
					}
			
					Invoke(nameof(SetBreakForce), 0.3f);
				}
			}
		}
		
		//... Set piercing joint break force
		void SetBreakForce()
		{
			if(weaponPhysics.gameObject.GetComponent<FixedJoint>() != null)
			{
				weaponPhysics.gameObject.GetComponent<FixedJoint>().breakForce = breakForceRequired;
			}
		}
		
		void LateUpdate()
		{
			if(canPierce)
			{
				//... Check and set parent
				if(pierced && weaponPhysics.GetComponent<FixedJoint>() == null)
				{
					pierced = false;
					
					if(!weaponAssigned)
					{
						transform.parent = weaponContainer;
					}
					
					else
					{
						transform.parent = weaponPrevParent;
					}
				}
				
				//... Break joint if not pierced anymore
				else if(!pierced && weaponPhysics.GetComponent<FixedJoint>() != null)
				{
					weaponPhysics.gameObject.GetComponent<FixedJoint>().breakForce = 0f;
				}
			}
			
			//... Disable object out bound when part of player tree
			if(weaponAssigned || pierced && GetComponent<PX_ObjectOutOfBound>().enabled)
			{
				GetComponent<PX_ObjectOutOfBound>().enabled = false;
			}
			
			//... Enable object out bound when NOT part of player tree
			else if(!weaponAssigned && !GetComponent<PX_ObjectOutOfBound>().enabled)
			{
				GetComponent<PX_ObjectOutOfBound>().enabled = true;
			}
			
			//... Set respective new layer when weapon assigned
			if(weaponAssigned && !pierced)
			{
				this.gameObject.layer = this.gameObject.transform.root.gameObject.layer;
				
				foreach(Transform child in this.gameObject.transform.GetComponentsInChildren<Transform>())
				{
					child.gameObject.layer = this.gameObject.transform.root.gameObject.layer;
				}
			}
			
			//... Revert to respective previous layer when weapon unassigned
			else if(!weaponAssigned && !pierced)
			{
				this.gameObject.layer = thisWeaponLayerRecord;
				
				foreach(Transform child in this.gameObject.transform.GetComponentsInChildren<Transform>())
				{
					child.gameObject.layer = thisWeaponLayerRecord;
				}
			}
		}
	}
}
