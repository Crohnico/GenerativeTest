//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_Weapons : MonoBehaviour
	{
		[Header("Player-X [Weapons]")]
		
		[Space]
		
		[Header("- Weapon Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Equip")]
		public float equipRange = 3f;
	    public PX_WeaponHolder equipLeft, equipRight;
		
		[Header("- Weapon Prefabs")]
		public List<GameObject> weapons = new List<GameObject>();
		
		[Header("- Muzzle Flash")]
		public GameObject muzzleFlash;
		
		[Header("- Aim References")]
		public Transform aimLeftTracker;
		public Transform aimRightTracker;
		public Transform trackerPointLeft;
		public Transform trackerPointRight;
		public Transform crossHair;
		
		[Header("- Tracking")]
		public bool enableWeaponHeadTrack = false;
		
		[Header("- Containers")]
		public Transform weaponContainer;
		public Transform shellContainer;
		public Transform particleContainer;
		
		[Header("- Equip Indicator")]
		public Transform indicator;
		
		[Header("- Ignore Raycast Layer")]
		public LayerMask playerLayer;
		
		//- Hidden Weapon Variables
		
		[HideInInspector]
		public bool
		weaponAssignedLeft,
		weaponAssignedRight,
		shootingLeft, shootingRight;
		
		GameObject 
		closestWeapon,
		hitObjectLeft, 
		hitObjectRight;
		
		bool 
		shotLeft, shotRight, 
		severedLeftArm, severedRightArm;
		
		ConfigurableJoint 
		weaponLeftJoint, weaponRightJoint;
		
		[HideInInspector]
		public Vector3 shootDir;
		
		Quaternion 
		leftArmAimTrackRot, 
		rightArmAimTrackRot;
		
		bool raycastHitObject;
		
		[HideInInspector]
		public bool simpleAIWeaponCheck = false;
		
		
		
		void Start()
		{
			WeaponReferences();
		}
		
		void Update()
		{
			if(dependencies.state.isAlive && !dependencies.state.isKnockedOut)
			{
				WeaponIndicator();
				WeaponEquip();
				WeaponShoot();
			}
			
			if(!dependencies.state.isAlive)
			{
				DropAllWeapons();
			}
		}
		
		void FixedUpdate()
		{
			if(dependencies.state.isAlive && !dependencies.state.isKnockedOut)
			{
				WeaponAim();
			}
		}
		
		
		
		//... Weapons References ...
		void WeaponReferences()
		{
			//... Setup Aim Rotation
			leftArmAimTrackRot =  aimLeftTracker.localRotation;
			rightArmAimTrackRot = aimRightTracker.localRotation;
			
			//... Assign weapon container to all weapons
			foreach(GameObject weapon in weapons)
			{
				weapon.GetComponent<PX_WeaponAttributes>().weaponContainer = weaponContainer;
			}
		}
		
		
		
		//... Weapon Indication ...
		void WeaponIndicator()
		{
			if(weapons.Count != 0 && indicator != null)
			{
				//... Find closest weapon to indicate
				if(weaponAssignedLeft && weaponAssignedRight)
				{
					indicator.gameObject.SetActive(false);
				}
				
				else
				{
					var closestToEquipWeapon = weapons.OrderBy(t=> (t.transform.position - dependencies.player.rootPhysics.transform.position).sqrMagnitude).FirstOrDefault();
							
					//... Indicate when in range
					if(!closestToEquipWeapon.GetComponent<PX_WeaponAttributes>().weaponAssigned && Vector3.Distance(closestToEquipWeapon.transform.position, dependencies.player.rootPhysics.transform.position) <= equipRange)
					{
						if(!weaponAssignedLeft || !weaponAssignedRight)
						{
							indicator.position = closestToEquipWeapon.transform.position;
							indicator.gameObject.SetActive(true);
						}
					}
					
					else
					{
						indicator.gameObject.SetActive(false);
					}
				}
			}
		}
		
		
		
		//... Weapon Equip ...
		void WeaponEquip()
		{
			if(dependencies.state.isAlive)
			{
				//... Weapon left hand
				if(dependencies.inputs.keyEquipLeft_Input && !severedLeftArm)
				{
					//... Unequip
					if(weaponAssignedLeft && !shootingLeft)
					{
						DropWeaponLeft();
					}
						
					else if(weapons.Count != 0)
					{
						//... Find closest weapon
						closestWeapon = weapons.OrderBy(t=> (t.transform.position - dependencies.player.rootPhysics.transform.position).sqrMagnitude).FirstOrDefault();
						
						//... Equip when in range
						if(!closestWeapon.GetComponent<PX_WeaponAttributes>().weaponAssigned && Vector3.Distance(closestWeapon.transform.position, dependencies.player.rootPhysics.transform.position) <= equipRange)
						{
							//... Equip if no weapon or grabbed object
							if(dependencies.controller.grabDetectLeft.connectedJointLeft == null && !weaponAssignedLeft)
							{		
								//... Check and remove if weapon is pierced when picking up
								if(closestWeapon.GetComponent<FixedJoint>() != null)
								{
									closestWeapon.GetComponent<FixedJoint>().breakForce = 0;
								}
								
								//... Disable weapon handle colliders
								if(closestWeapon.GetComponent<PX_WeaponAttributes>().weaponHandleColliders.Length != 0)
								{
									foreach(Collider collider in closestWeapon.GetComponent<PX_WeaponAttributes>().weaponHandleColliders)
									{
										collider.enabled = false;
									}
								}
								
								//... Remove weapon from pickup tracking container
								if(weapons.Contains(closestWeapon.gameObject))
								{
									weapons.Remove(closestWeapon.gameObject);
								}
								
								//... Remove weapon from head tracking container
								if(enableWeaponHeadTrack && dependencies.controller.headTrackContainer.Contains(closestWeapon.gameObject.transform))
								{
									dependencies.controller.headTrackContainer.Remove(closestWeapon.gameObject.transform);
								}
								
								//... Enable Crosshair if equiped weapon is a gun
								if(closestWeapon.GetComponent<PX_WeaponAttributes>().isGun)
								{
									crossHair.gameObject.SetActive(true);
								}
								
								//... Execute in the next frame incase/while pierced joint breaks
								StartCoroutine(waitForBreak());
								IEnumerator waitForBreak()
								{
									yield return new WaitForSeconds(Time.smoothDeltaTime);
									
									//... Set position to hand/weapon held point
									closestWeapon.transform.position = equipLeft.gameObject.transform.position;
									closestWeapon.transform.rotation = equipLeft.gameObject.transform.rotation;
									
									//... Move weapon into player tree
									closestWeapon.transform.parent = equipLeft.gameObject.transform;
									
									//... Create and set joint properties
									weaponLeftJoint = closestWeapon.AddComponent<ConfigurableJoint>();
									
									weaponLeftJoint.xMotion = ConfigurableJointMotion.Locked;
									weaponLeftJoint.yMotion = ConfigurableJointMotion.Locked;
									weaponLeftJoint.zMotion = ConfigurableJointMotion.Locked;
									
									weaponLeftJoint.angularXMotion = ConfigurableJointMotion.Locked;
									weaponLeftJoint.angularYMotion = ConfigurableJointMotion.Locked;
									weaponLeftJoint.angularZMotion = ConfigurableJointMotion.Locked;
									
									//... Connect joint to hand
									weaponLeftJoint.connectedBody = equipLeft.weaponPhysics;
									
									//... Assign weapon attributes
									equipLeft.attackForce = closestWeapon.GetComponent<PX_WeaponAttributes>().attackForce;
									equipLeft.attackPoint = closestWeapon.GetComponent<PX_WeaponAttributes>().attackPoint;
									equipLeft.weaponPhysics = closestWeapon.GetComponent<PX_WeaponAttributes>().weaponPhysics;
									equipLeft.dependencies = dependencies;
									
									weaponLeftJoint.GetComponent<PX_WeaponAttributes>().weaponAssigned = true;
									weaponAssignedLeft = true;
								}
								
								//... Audio
								equipLeft.attackSounds = closestWeapon.GetComponent<PX_WeaponAttributes>().attackSounds;
								
								if(dependencies.sound.soundSource != null)
								{
									dependencies.sound.soundToPlay = dependencies.sound.equipSound;
									dependencies.sound.PlayAudio();
								}
							}
						}
					}
				}
				
				
				
				//... Weapon right hand
				if(dependencies.inputs.keyEquipRight_Input && !severedRightArm)
				{
					//... Unequip
					if(weaponAssignedRight && !shootingRight)
					{
						DropWeaponRight();
					}
					
					else if(weapons.Count != 0)
					{
						//... Find closest weapon
						closestWeapon = weapons.OrderBy(t=> (t.transform.position - dependencies.player.rootPhysics.transform.position).sqrMagnitude).FirstOrDefault();
						
						//... Equip when in range
						if(!closestWeapon.GetComponent<PX_WeaponAttributes>().weaponAssigned && Vector3.Distance(closestWeapon.transform.position, dependencies.player.rootPhysics.transform.position) <= equipRange)
						{
							//... Equip if no weapon or grabbed object
							if(dependencies.controller.grabDetectRight.connectedJointRight == null && !weaponAssignedRight)
							{
								//... Check and remove if weapon is pierced when picking up
								if(closestWeapon.GetComponent<FixedJoint>() != null)
								{
									closestWeapon.GetComponent<FixedJoint>().breakForce = 0;
								}
								
								//... Disable weapon handle colliders
								if(closestWeapon.GetComponent<PX_WeaponAttributes>().weaponHandleColliders.Length != 0)
								{
									foreach(Collider collider in closestWeapon.GetComponent<PX_WeaponAttributes>().weaponHandleColliders)
									{
										collider.enabled = false;
									}
								}
								
								//... Remove weapon from pickup tracking container
								if(weapons.Contains(closestWeapon.gameObject))
								{
									weapons.Remove(closestWeapon.gameObject);
								}
								
								//... Remove weapon from head tracking container
								if(enableWeaponHeadTrack && dependencies.controller.headTrackContainer.Contains(closestWeapon.gameObject.transform))
								{
									dependencies.controller.headTrackContainer.Remove(closestWeapon.gameObject.transform);
								}
								
								//... Enable Crosshair if equiped weapon is a gun
								if(closestWeapon.GetComponent<PX_WeaponAttributes>().isGun)
								{
									crossHair.gameObject.SetActive(true);
								}
								
								//... Execute in the next frame incase/while pierced joint breaks
								StartCoroutine(waitForBreak());
								IEnumerator waitForBreak()
								{
									yield return new WaitForSeconds(Time.smoothDeltaTime);
									
									//... Set position to hand/weapon held point
									closestWeapon.transform.position = equipRight.gameObject.transform.position;
									closestWeapon.transform.rotation = equipRight.gameObject.transform.rotation;
									
									//... Move weapon into player tree
									closestWeapon.transform.parent = equipRight.transform;
									
									//... Create and set joint properties
									weaponRightJoint = closestWeapon.AddComponent<ConfigurableJoint>();
									
									weaponRightJoint.xMotion = ConfigurableJointMotion.Locked;
									weaponRightJoint.yMotion = ConfigurableJointMotion.Locked;
									weaponRightJoint.zMotion = ConfigurableJointMotion.Locked;
									
									weaponRightJoint.angularXMotion = ConfigurableJointMotion.Locked;
									weaponRightJoint.angularYMotion = ConfigurableJointMotion.Locked;
									weaponRightJoint.angularZMotion = ConfigurableJointMotion.Locked;
									
									//... Connect joint to hand
									weaponRightJoint.connectedBody = equipRight.weaponPhysics;
									
									//... Assign weapon attributes
									equipRight.attackForce = closestWeapon.GetComponent<PX_WeaponAttributes>().attackForce;
									equipRight.attackPoint = closestWeapon.GetComponent<PX_WeaponAttributes>().attackPoint;
									equipRight.weaponPhysics = closestWeapon.GetComponent<PX_WeaponAttributes>().weaponPhysics;
									equipRight.dependencies = dependencies;
									
									weaponRightJoint.GetComponent<PX_WeaponAttributes>().weaponAssigned = true;
									weaponAssignedRight = true;
								}
								
								//... Audio
								equipRight.attackSounds = closestWeapon.GetComponent<PX_WeaponAttributes>().attackSounds;
								
								if(dependencies.sound.soundSource != null)
								{
									dependencies.sound.soundToPlay = dependencies.sound.equipSound;;
									dependencies.sound.PlayAudio();
								}
							}
						}
					}
				}
			}
			
			//... Drop weapon left if arm or hand is severed
			if(!severedLeftArm && dependencies.player.armLeftJoint.connectedBody == null || dependencies.player.handLeftJoint.connectedBody == null)
			{
				severedLeftArm = true;
				DropWeaponLeft();
			}
			
			//... Drop weapon right if arm or hand is severed
			if(!severedRightArm && dependencies.player.armRightJoint.connectedBody == null || dependencies.player.handRightJoint.connectedBody == null)
			{
				severedRightArm = true;
				DropWeaponRight();
			}
			
			//... Drop weapons if player is dead
			if(!dependencies.state.isAlive && (!severedLeftArm || !severedRightArm))
			{
				severedLeftArm = true;
				severedRightArm = true;
				
				DropWeaponLeft();
				DropWeaponRight();
			}
		}
		
		
		
		//... Weapon Aim ...
		void WeaponAim()
		{
			//... Left arm aim
			if(weaponAssignedLeft && dependencies.inputs.mouseLeft_input)
			{
				if(raycastHitObject)
				{
					aimLeftTracker.LookAt(shootDir);
				}
				
				else
				{
					aimLeftTracker.LookAt(dependencies.player.rootPhysics.transform.forward * 100);
				}
				
				aimLeftTracker.position = trackerPointLeft.position;
				
				//... Check if weapon is gun
				if(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().isGun)
				{
					shootingLeft = true;
					
					//... Set aim joint drive
					dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
					dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
					dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
					dependencies.player.handLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
					
					//... Copy aim tracker rotation
					dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, 
					dependencies.player.bodyUpperPhysics.transform.localRotation * Quaternion.Inverse(aimLeftTracker.localRotation) * leftArmAimTrackRot, 30f * Time.fixedDeltaTime);
				}
			}
			
			//... Revert if not shooting
			else if(shootingLeft)
			{
				shootingLeft = false;
				
				dependencies.player.armLeftJoint.targetRotation = dependencies.procedural.armLeftTarget;
				
				dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyLowerJointDrive;
				dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyUpperJointDrive;
				dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armLeftJointDrive;
				dependencies.player.handLeftJoint.slerpDrive = dependencies.player.handLeftJointDrive;
			}
			
			
			//... Right arm aim
			if(weaponAssignedRight && dependencies.inputs.mouseRight_input)
			{
				if(raycastHitObject)
				{
					aimRightTracker.LookAt(shootDir);
				}
				
				else
				{
					aimRightTracker.LookAt(dependencies.player.rootPhysics.transform.forward * 100);
				}
				
				aimRightTracker.position = trackerPointRight.position;
			
				//... Check if weapon is gun
				if(weaponRightJoint.GetComponent<PX_WeaponAttributes>().isGun)
				{
					shootingRight = true;
					
					//... Set aim joint drive
					dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
					dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
					dependencies.player.armRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
					dependencies.player.handRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
					
					//... Copy aim tracker rotation
					dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, 
					dependencies.player.bodyUpperPhysics.transform.localRotation * Quaternion.Inverse(aimRightTracker.localRotation) * rightArmAimTrackRot, 30f * Time.fixedDeltaTime);
				}
			}
			
			//... Revert if not shooting
			else if(shootingRight)
			{
				shootingRight = false;
				
				dependencies.player.armRightJoint.targetRotation = dependencies.procedural.armRightTarget;
				
				dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyLowerJointDrive;
				dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyUpperJointDrive;
				dependencies.player.armRightJoint.slerpDrive = dependencies.player.armRightJointDrive;
				dependencies.player.handRightJoint.slerpDrive = dependencies.player.handRightJointDrive;
			}
		}
		
		
		
		//... Weapon Shoot ...
		void WeaponShoot()
		{
			if((weaponAssignedLeft || weaponAssignedRight) && (dependencies.inputs.mouseLeft_input || dependencies.inputs.mouseRight_input))
			{
				//... Raycast from crossHair
				var crossHairRay = dependencies.playerCamera.followCamera.ScreenPointToRay(crossHair.position);
				
				RaycastHit crossHairHit;
				if(Physics.Raycast(crossHairRay.origin, crossHairRay.direction, out crossHairHit, Mathf.Infinity, ~playerLayer))
				{
					shootDir = crossHairHit.point;
					raycastHitObject = true;
					
					//... Weapon Shoot left
					if(weaponAssignedLeft && weaponLeftJoint.GetComponent<PX_WeaponAttributes>().isGun)
					{	
						if(dependencies.inputs.mouseLeft_input)
						{	
							if(!shotLeft)
							{
								shotLeft = true;
								
								//... Detect raycast hit from weapon shooting point to crossHair hit point
								RaycastHit hit;
								if(Physics.Raycast(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, crossHairHit.point - weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, out hit, Mathf.Infinity))
								{
									hitObjectLeft = hit.collider.gameObject;
									
									//... Apply hit force
									if(hitObjectLeft.GetComponent<Rigidbody>() != null)
									{
										hitObjectLeft.GetComponent<Rigidbody>().AddForce(-hit.normal * weaponLeftJoint.GetComponent<PX_WeaponAttributes>().hitForce, ForceMode.Impulse);
									}
									
									//... Muzzle Flash
									var muzzle = Instantiate(muzzleFlash, weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, Quaternion.identity);
									muzzle.transform.parent = particleContainer;
									
									//... Shell drop
									var shell = Instantiate(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shell, weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shellPoint.position, weaponLeftJoint.GetComponent<PX_WeaponAttributes>().shellPoint.rotation);
									shell.transform.parent = shellContainer;
									
									//... Recoil
									dependencies.player.handLeftPhysics.AddForce(-dependencies.player.handLeftPhysics.transform.forward + dependencies.player.handLeftPhysics.transform.up * weaponLeftJoint.GetComponent<PX_WeaponAttributes>().recoilForce, ForceMode.Impulse);
									
									//... Hit Player
									if(hitObjectLeft.gameObject.transform.root != this.gameObject.transform.root && hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>() != null 
									&& (hitObjectLeft.layer == LayerMask.NameToLayer("Player-X") || hitObjectLeft.layer == LayerMask.NameToLayer("Player-X (Other)")))
									{
										//... Head shot
										if(hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.headPhysics.gameObject)
										{
											hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponLeftJoint.GetComponent<PX_WeaponAttributes>().headShotDamage;
											
											if(hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>() && hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isAlive)
											{
												var confetti = Instantiate(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().headShotParticle, hit.point, Quaternion.identity);
												confetti.transform.parent = dependencies.state.particleContainer;
											}
										}
										
										//... Body shot
										else if(hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.rootPhysics.gameObject 
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyLowerPhysics.gameObject 
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyUpperPhysics.gameObject)
										{
											hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponLeftJoint.GetComponent<PX_WeaponAttributes>().bodyShotDamage;
										}
										
										//... Limb shot
										else if(hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armLeftPhysics.gameObject 
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armRightPhysics.gameObject 
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handLeftPhysics.gameObject
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handRightPhysics.gameObject
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legLeftPhysics.gameObject
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legRightPhysics.gameObject
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footLeftPhysics.gameObject
										|| hitObjectLeft == hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footRightPhysics.gameObject)
										{
											hitObjectLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponLeftJoint.GetComponent<PX_WeaponAttributes>().limbShotDamage;
										}
										
										//... Sever player part if explosive
										if(hitObjectLeft.gameObject.GetComponent<PX_ImpactDetect>() != null && weaponLeftJoint.GetComponent<PX_WeaponAttributes>().burst)
										{	
											//... Sever part
											Invoke(nameof(WeaponLeftSever), 0.1f);
										}
									}
									
									//... Hit player particle
									if(hitObjectLeft.layer == LayerMask.NameToLayer("Player-X") || hitObjectLeft.layer == LayerMask.NameToLayer("Player-X (Other)"))
									{
										var blood = Instantiate(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().hitPlayerParticle, hit.point, Quaternion.identity);
										blood.transform.parent = dependencies.state.particleContainer;
										
										//... Bullet hit player audio
										if(dependencies.sound != null)
										{
											dependencies.sound.soundToPlay = weaponLeftJoint.GetComponent<PX_WeaponAttributes>().bulletHitPlayerSound;
											dependencies.sound.soundPoint = hit.point;
											dependencies.sound.PlayAudio();
										}
									}
									
									//... Hit Object Particle
									else
									{
										var spark = Instantiate(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().hitObjectParticle, hit.point, Quaternion.identity);
										spark.transform.parent = dependencies.state.particleContainer;
										
										//... Bullet hit object audio
										if(dependencies.sound != null)
										{
											dependencies.sound.soundToPlay = weaponLeftJoint.GetComponent<PX_WeaponAttributes>().bulletHitObjectSound;
											dependencies.sound.soundPoint = hit.point;
											dependencies.sound.PlayAudio();
										}
									}
									
									//... Audio
									if(dependencies.sound != null)
									{
										dependencies.sound.soundToPlay = weaponLeftJoint.GetComponent<PX_WeaponAttributes>().gunSounds
										[Random.Range(0, weaponLeftJoint.GetComponent<PX_WeaponAttributes>().gunSounds.Length)];
										dependencies.sound.soundPoint = weaponLeftJoint.transform.position;
										dependencies.sound.PlayAudio();
									}
								}
								
								//... Reset after fire rate
								Invoke(nameof(shotRateLeft), weaponLeftJoint.GetComponent<PX_WeaponAttributes>().fireRate);
							}
						}
					}
					
					//... Weapon shoot right
					if(weaponAssignedRight && weaponRightJoint.GetComponent<PX_WeaponAttributes>().isGun)
					{
						if(dependencies.inputs.mouseRight_input)
						{
							if(!shotRight)
							{
								shotRight = true;
								
								//... Detect raycast hit from weapon shooting point to crossHair hit point
								RaycastHit hit;
								if(Physics.Raycast(weaponRightJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, crossHairHit.point - weaponRightJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, out hit, Mathf.Infinity))
								{
									hitObjectRight = hit.collider.gameObject;
									
									//... Apply hit force
									if(hitObjectRight.GetComponent<Rigidbody>() != null)
									{
										hitObjectRight.GetComponent<Rigidbody>().AddForce(-hit.normal * weaponRightJoint.GetComponent<PX_WeaponAttributes>().hitForce, ForceMode.Impulse);
									}
									
									//... Muzzle Flash
									var muzzle = Instantiate(muzzleFlash, weaponRightJoint.GetComponent<PX_WeaponAttributes>().shootPoint.position, Quaternion.identity);
									muzzle.transform.parent = particleContainer;
									
									//... Shell drop
									var shell = Instantiate(weaponRightJoint.GetComponent<PX_WeaponAttributes>().shell, weaponRightJoint.GetComponent<PX_WeaponAttributes>().shellPoint.position, weaponRightJoint.GetComponent<PX_WeaponAttributes>().shellPoint.rotation);
									shell.transform.parent = shellContainer;
									
									//... Recoil
									dependencies.player.handRightPhysics.AddForce(-dependencies.player.handRightPhysics.transform.forward + dependencies.player.handRightPhysics.transform.up * weaponRightJoint.GetComponent<PX_WeaponAttributes>().recoilForce, ForceMode.Impulse);
									
									//... Hit Player
									if(hitObjectRight.gameObject.transform.root != this.gameObject.transform.root && hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>() != null 
									&& (hitObjectRight.layer == LayerMask.NameToLayer("Player-X") || hitObjectRight.layer == LayerMask.NameToLayer("Player-X (Other)")))
									{
										//... Head shot
										if(hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.headPhysics.gameObject)
										{
												hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponRightJoint.GetComponent<PX_WeaponAttributes>().headShotDamage;
												
												if(hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>() && hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isAlive)
												{
													var confetti = Instantiate(weaponRightJoint.GetComponent<PX_WeaponAttributes>().headShotParticle, hit.point, Quaternion.identity);
													confetti.transform.parent = dependencies.state.particleContainer;
												}
										}
										
										//... Body shot
										else if(hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.rootPhysics.gameObject 
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyLowerPhysics.gameObject 
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.bodyUpperPhysics.gameObject)
										{
											hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponRightJoint.GetComponent<PX_WeaponAttributes>().bodyShotDamage;
										}
										
										//... Limb shot
										else if(hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armLeftPhysics.gameObject 
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.armRightPhysics.gameObject 
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handLeftPhysics.gameObject
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.handRightPhysics.gameObject
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legLeftPhysics.gameObject
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.legRightPhysics.gameObject
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footLeftPhysics.gameObject
										|| hitObjectRight == hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().player.footRightPhysics.gameObject)
										{
											hitObjectRight.transform.root.gameObject.GetComponent<PX_Dependencies>().health.playerHealth -= weaponRightJoint.GetComponent<PX_WeaponAttributes>().limbShotDamage;
										}
										
										//... Sever player part if explosive
										if(hitObjectRight.gameObject.GetComponent<PX_ImpactDetect>() != null && weaponRightJoint.GetComponent<PX_WeaponAttributes>().burst)
										{
											//... Sever part
											Invoke(nameof(WeaponRightSever), 0.1f);
										}
									}
									
									//... Hit player particle
									if(hitObjectRight.layer == LayerMask.NameToLayer("Player-X") || hitObjectRight.layer == LayerMask.NameToLayer("Player-X (Other)"))
									{
										var blood = Instantiate(weaponRightJoint.GetComponent<PX_WeaponAttributes>().hitPlayerParticle, hit.point, Quaternion.identity);
										blood.transform.parent = dependencies.state.particleContainer;
										
										//... Bullet hit player audio
										if(dependencies.sound != null)
										{
											dependencies.sound.soundToPlay = weaponRightJoint.GetComponent<PX_WeaponAttributes>().bulletHitPlayerSound;
											dependencies.sound.soundPoint = hit.point;
											dependencies.sound.PlayAudio();
										}
									}
									
									//... Hit Object Particle
									else
									{
										var spark = Instantiate(weaponRightJoint.GetComponent<PX_WeaponAttributes>().hitObjectParticle, hit.point, Quaternion.identity);
										spark.transform.parent = dependencies.state.particleContainer;
										
										//... Bullet hit player audio
										if(dependencies.sound != null)
										{
											dependencies.sound.soundToPlay = weaponRightJoint.GetComponent<PX_WeaponAttributes>().bulletHitObjectSound;
											dependencies.sound.soundPoint = hit.point;
											dependencies.sound.PlayAudio();
										}
									}
									
									//... Audio
									if(dependencies.sound != null)
									{
										dependencies.sound.soundToPlay = weaponRightJoint.GetComponent<PX_WeaponAttributes>().gunSounds
										[Random.Range(0, weaponRightJoint.GetComponent<PX_WeaponAttributes>().gunSounds.Length)];
										dependencies.sound.soundPoint = weaponRightJoint.transform.position;
										dependencies.sound.PlayAudio();
									}
								}
								
								//... Reset after fire rate
								Invoke(nameof(shotRateRight), weaponRightJoint.GetComponent<PX_WeaponAttributes>().fireRate);
							}
						}
					}
				}
				
				else
				{
					raycastHitObject = false;
				}
			}
		}
		
		
		
		//... Reset shoot left
		void shotRateLeft()
		{
			shotLeft = false;
		}
		
		
		
		//... Reset shoot right
		void shotRateRight()
		{
			shotRight = false;
		}
		
		
		
		//... Sever player if weapon left has burst
		void WeaponLeftSever()
		{
			if(hitObjectLeft.gameObject.GetComponent<PX_ImpactDetect>())
			{
				hitObjectLeft.gameObject.GetComponent<PX_ImpactDetect>().DismemberOnCommand();
			}
		}
		
		
		
		//... Sever player if weapon right has burst
		void WeaponRightSever()
		{
			if(hitObjectRight.gameObject.GetComponent<PX_ImpactDetect>())
			{
				hitObjectRight.gameObject.GetComponent<PX_ImpactDetect>().DismemberOnCommand();
			}
		}
		
		
		//... Weapon drop left 
		void DropWeaponLeft()
		{
			if(weaponLeftJoint != null)
			{
				//... Add weapon back into pickup tracking container
				if(!weapons.Contains(weaponLeftJoint.gameObject))
				{
					weapons.Add(weaponLeftJoint.gameObject);
				}
				
				//... Add weapon back into head tracking container
				if(enableWeaponHeadTrack && !dependencies.controller.headTrackContainer.Contains(weaponLeftJoint.gameObject.transform))
				{
					dependencies.controller.headTrackContainer.Add(weaponLeftJoint.gameObject.transform);
				}
				
				//... Enable weapon handle colliders
				if(weaponLeftJoint.gameObject.GetComponent<PX_WeaponAttributes>().weaponHandleColliders.Length != 0)
				{
					foreach(Collider collider in weaponLeftJoint.gameObject.GetComponent<PX_WeaponAttributes>().weaponHandleColliders)
					{
						collider.enabled = true;
					}
				}
				
				//... Move weapon out of player tree if not pierced
				if(!weaponLeftJoint.GetComponent<PX_WeaponAttributes>().pierced)
				{
					weaponLeftJoint.gameObject.transform.parent = weaponContainer;
				}
				
				//... Disable Crosshair if no guns equiped
				if(weaponLeftJoint.GetComponent<PX_WeaponAttributes>().isGun)
				{
					if(weaponRightJoint == null || (weaponRightJoint != null && !weaponRightJoint.GetComponent<PX_WeaponAttributes>().isGun))
					{
						crossHair.gameObject.SetActive(false);
					}
				}

				//... Revert back to default attributes
				equipLeft.attackForce = dependencies.controller.punchForce;
				equipLeft.attackPoint = equipLeft.gameObject.transform;
				equipLeft.weaponPhysics = equipLeft.gameObject.GetComponent<Rigidbody>();
				
				weaponAssignedLeft = false;
				weaponLeftJoint.GetComponent<PX_WeaponAttributes>().weaponAssigned = false;
				
				//... Release weapon
				weaponLeftJoint.breakForce = 0f;
				
				//... Audio
				equipLeft.attackSounds = dependencies.sound.attackSounds;
				
				if(dependencies.sound.soundSource != null)
				{
					dependencies.sound.soundToPlay = dependencies.sound.dropSound;
					dependencies.sound.PlayAudio();
				}
			}
		}
		
		
		
		//... Weapon drop right
		void DropWeaponRight()
		{
			if(weaponRightJoint != null)
			{
				//... Add weapon back into pickup tracking container
				if(!weapons.Contains(weaponRightJoint.gameObject))
				{
					weapons.Add(weaponRightJoint.gameObject);
				}
				
				//... Add weapon back into head tracking container
				if(enableWeaponHeadTrack && !dependencies.controller.headTrackContainer.Contains(weaponRightJoint.gameObject.transform))
				{
					dependencies.controller.headTrackContainer.Add(weaponRightJoint.gameObject.transform);
				}
				
				//... Enable weapon handle colliders
				if(weaponRightJoint.gameObject.GetComponent<PX_WeaponAttributes>().weaponHandleColliders.Length != 0)
				{
					foreach(Collider collider in weaponRightJoint.gameObject.GetComponent<PX_WeaponAttributes>().weaponHandleColliders)
					{
						collider.enabled = true;
					}
				}
				
				//... Move weapon out of player tree if not pierced
				if(!weaponRightJoint.GetComponent<PX_WeaponAttributes>().pierced)
				{
					weaponRightJoint.gameObject.transform.parent = weaponContainer;
				}
				
				//... Disable Crosshair if no guns equiped
				if(weaponRightJoint.GetComponent<PX_WeaponAttributes>().isGun)
				{
					if(weaponLeftJoint == null || (weaponRightJoint != null && !weaponLeftJoint.GetComponent<PX_WeaponAttributes>().isGun))
					{
						crossHair.gameObject.SetActive(false);
					}
				}
				
				//... Revert back to default attributes
				equipRight.attackForce = dependencies.controller.punchForce;
				equipRight.attackPoint = equipRight.gameObject.transform;
				equipRight.weaponPhysics = equipRight.gameObject.GetComponent<Rigidbody>();
				
				weaponAssignedRight = false;
				weaponRightJoint.GetComponent<PX_WeaponAttributes>().weaponAssigned = false;
				
				//... Release weapon
				weaponRightJoint.breakForce = 0f;
				
				//... Audio
				equipRight.attackSounds = dependencies.sound.attackSounds;
				
				if(dependencies.sound.soundSource != null)
				{
					dependencies.sound.soundToPlay = dependencies.sound.dropSound;
					dependencies.sound.PlayAudio();
				}
			}
		}
		
		//... Drop all weapons when dead
		void DropAllWeapons()
		{
			if(weaponAssignedLeft)
			{
				DropWeaponLeft();
			}
			
			if(weaponAssignedRight)
			{
				DropWeaponRight();
			}
		}
	}
}
