//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using TMPro;
using PlayerX;

namespace PlayerX
{
	public class PX_Score : MonoBehaviour
	{
		[Header("Player-X [Score]")]
		
		[Space]
		
		[Header("Properties")]
		public TextMeshProUGUI scoreText;
	    public int score = 0;
		
		//- Hidden Variables
		Animator anim;
		
		
		void Start()
		{
			anim = GetComponent<Animator>();
		}
		
		//... Add Score
	    public void AddScore(int Scored)
	    {
	        score += Scored;
			scoreText.SetText("- " + score + " -");
			
			anim.Play("Scored");
	    }
	}
}
