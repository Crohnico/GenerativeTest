//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using Unity.Mathematics;

namespace PlayerX
{
    public class PX_Stabilizer
    {
        public float frequency = 0.001f;

        const float cutOff = 1.0f;


        public float3 Step(float stepTime, float3 stepPos)
        {
            var timeElapsed = stepTime - stabilize.time;

            if (timeElapsed < 1e-5f) return stabilize.a;

            var a = (stepPos - stabilize.a) / timeElapsed;
            var smooth_a = math.lerp(stabilize.b, a, Alpha(timeElapsed, cutOff));

            var b = frequency * math.length(smooth_a);
            var smooth_b = math.lerp(stabilize.a, stepPos, Alpha(timeElapsed, b));

            stabilize = (stepTime, smooth_b, smooth_a);

            return smooth_b;
        }


        static float Alpha(float timeElapsed, float cutoff)
        {
            var x = 2 * math.PI * cutoff * timeElapsed;
            return x / (x + 1);
        }


        (float time, float3 a, float3 b) stabilize;
    }
}