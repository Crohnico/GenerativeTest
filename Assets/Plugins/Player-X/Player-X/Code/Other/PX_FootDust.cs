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
	public class PX_FootDust : MonoBehaviour
	{
		[Header("Player-X [Foot Dust]")]
		
		[Space]
		
		[Header("- Foot Dust Dependencies")]
	    public PX_Dependencies dependencies;
		
		[Header("- Dust")]
		public GameObject dustParticle;
		
		
		
		//... Spawn dust particles at feet when run input and player moving
	    void OnCollisionEnter(Collision col)
	    {
	        if(dependencies.inputs.keyRun_Input && dependencies.controller.moveDir != Vector3.zero && dependencies.state.Grounded() && col.gameObject.transform.root != this.gameObject.transform.root)
			{
				var dust = Instantiate(dustParticle, transform.position + new Vector3(0f, -0.4f, 0f), transform.rotation);
				dust.transform.parent = dependencies.state.particleContainer;
			}
	    }
	}
}
