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
	public class PX_DrawRope : MonoBehaviour
	{
		[Header("Player-X [Draw Rope]")]
		
		[Space]
		
		[Header("Rope Properties")]
	    public LineRenderer line;
		public Transform ropeStart, ropeEnd;
		public Transform[] ropeSegments;
		
		
		//... Draw LineRenderer ropes
	    void LateUpdate()
	    {
			line.SetPosition(0, ropeStart.position);
			
	        for (int i = 1; i < ropeSegments.Length - 1; i++)
	        {
				if(i != ropeSegments.Length)
				{
					line.SetPosition(i, ropeSegments[i + 1].position);
				}
			}
			
			line.SetPosition(line.positionCount - 1, ropeEnd.position);
	    }
		
		
		//... Draw lines for reference
		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
	        Gizmos.DrawLine(ropeStart.position, ropeEnd.position);
		}
	}
}
