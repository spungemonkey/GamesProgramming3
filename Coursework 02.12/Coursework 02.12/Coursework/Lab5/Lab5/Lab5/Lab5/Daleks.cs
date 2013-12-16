using Microsoft.Xna.Framework;


namespace Lab5
{
    struct Daleks
    {
        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public bool isActive;

        public void Update(float delta)
        {
 
            position += direction * speed * GameConstants.DalekSpeedAdjustment * delta;

            if (position.X > GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
                //position.X -= 30 * GameConstants.PlayfieldSizeX;
            }
            if (position.X < -GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
                // position.X += 30 * GameConstants.PlayfieldSizeX;
            }

            if (position.Z > GameConstants.PlayfieldSizeY)
            {
                direction.Z = direction.Z * -1;
                //position.Z -= 2 * GameConstants.PlayfieldSizeY;
            }
            if (position.Z < -GameConstants.PlayfieldSizeY)
            {
                direction.Z = direction.Z * -1;
                //position.Z += 2 * GameConstants.PlayfieldSizeY;
            }
        }
    }
}
