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
	public class PX_Stain : MonoBehaviour
	{
		[Header("Player-X [Stain]")]
		
		[Space]
		
		[Header("- Stain Properties")]
	    public float stainDuration = 6f;
		public float fadeSpeed = 1f;
		public Vector3 startSize = new Vector3(0.1f, 0.05f, 0.1f);
		public float minSize = 0.01f;
		public float maxSize = 0.12f;
		public Material lightVariant, darkVariant;
		public string containerName = new string("Stain_Container");
		
		//- Hidden Stain Variables
		
		bool fading = false;
		
		
		
		void Awake()
		{
			transform.localScale = startSize;
			transform.parent = GameObject.Find(containerName).transform;
		}
		
		//... Apply variation to stains
	    void Start()
	    {
			var randomMat = Random.Range(0, 3);
			
			if(randomMat == 0)
			{
				this.gameObject.GetComponent<MeshRenderer>().material = lightVariant;
			}
			
			else
			{
				this.gameObject.GetComponent<MeshRenderer>().material = darkVariant;
			}
			
			var scaleVariant = Random.Range(minSize, maxSize);
			transform.localScale = transform.localScale + new Vector3(scaleVariant, transform.localScale.y, scaleVariant);
			
	        Invoke(nameof(StartFade), stainDuration);
	    }
		
		
		void StartFade()
		{
			fading = true;
		}

	    //... Reduce scale, fade until destroyed
	    void Update()
	    {
	        if(fading)
			{
				transform.localScale -= new Vector3(fadeSpeed, fadeSpeed / 2, fadeSpeed) * Time.deltaTime;
				
				if(transform.localScale.y <= 0)
				{
					Destroy(this.gameObject);
				}
			}
	    }
	}
}
