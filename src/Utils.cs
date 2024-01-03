namespace BeeWorld;

public static class Utils
{
    public static void MapTextureColor(Texture2D texture, int alpha, Color32 to, bool apply = true)
    {
        var colors = texture.GetPixels32();

        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].a == alpha)
            {
                colors[i] = to;
            }
        }
        
        texture.SetPixels32(colors);

        if (apply)
        {
            texture.Apply(false);
        }
    }

    public static BodyChunk ClosestAttackableChunk(Room room, Vector2 pos, float range)
    {
        BodyChunk closestChunk = null;
        var closestDist = float.MaxValue;

        foreach (var obj in room.updateList)
        {
            if (obj is Creature creature and not Player and not Fly && !creature.dead)
            {
                foreach (var chunk in creature.bodyChunks)
                {
                    var dist = Custom.Dist(chunk.pos, pos); 
                    if (dist < closestDist && dist < range)
                    {
                        closestChunk = chunk;
                        closestDist = dist;
                    }
                }
            }
        }

        return closestChunk;
    }
    
        //verify plots by inserting the commented functions into this plotter https://rechneronline.de/function-graphs/
    public static class LerpCurves
    {
        public static float SofterInSofterOut01(float valueFrom0to1) => (-Mathf.Cos(valueFrom0to1 * Mathf.PI)) * 0.5F + 0.5F;  //    ( -cos(x*pi)*0.5+0.5)

        public static float SofterInSofterOut10(float valueFrom0to1) => 1F - (-Mathf.Cos(valueFrom0to1 * Mathf.PI)) * 0.5F + 0.5F;  //   1-( -cos(x*pi)*0.5+0.5)

        public static float SoftestInSoftOut01(float valueFrom0to1) => Mathf.Pow((-Mathf.Cos(valueFrom0to1 * Mathf.PI)) * 0.5F + 0.5F, 2F);  //     (-cos(x*pi)*0.5+0.5)^2

        public static float SoftestInSoftOut10(float valueFrom0to1) => 1F - Mathf.Pow((-Mathf.Cos(valueFrom0to1 * Mathf.PI)) * 0.5F + 0.5F, 2F);  //    1-(-cos(x*pi)*0.5+0.5)^2

        //pretty much the same as SofterInSofterOut
        public static float SmoothStep01(float valueFrom0to1) => valueFrom0to1 * valueFrom0to1 * (3 - 2 * valueFrom0to1);  //     (x*x * (3 - 2*x))

        public static float SmoothStep10(float valueFrom0to1) => 1F - valueFrom0to1 * valueFrom0to1 * (3 - 2 * valueFrom0to1);  //    1-(x*x * (3 - 2*x))

        //Symmetric, SoftestInSoftOut is not
        public static float SmootherStep01(float valueFrom0to1) => valueFrom0to1 * valueFrom0to1 * valueFrom0to1 * (valueFrom0to1 * (6F * valueFrom0to1 - 15F) + 10F);  //     x*x*x * (x* (6*x - 15) + 10)

        public static float SmootherStep10(float valueFrom0to1) => 1F - valueFrom0to1 * valueFrom0to1 * valueFrom0to1 * (valueFrom0to1 * (6F * valueFrom0to1 - 15F) + 10F);  //    1-x*x*x * (x* (6*x - 15) + 10)

        public static float Linear01(float valueFrom0to1) => valueFrom0to1;  //        x

        public static float SoftInHardOut01(float valueFrom0to1) => Mathf.Pow(valueFrom0to1, 3);  //        x^3

        public static float SoftInHardOut10(float valueFrom0to1) => 1 - Mathf.Pow(valueFrom0to1, 3);  //    1-x^3

        public static float HardInSoftOut01(float valueFrom0to1) => 1 - Mathf.Pow(1 - valueFrom0to1, 3);  //     1-(1-x)^3

        public static float HardInSoftOut10(float valueFrom0to1) => Mathf.Pow(1 - valueFrom0to1, 3);  //    (1-x)^3
    }
}