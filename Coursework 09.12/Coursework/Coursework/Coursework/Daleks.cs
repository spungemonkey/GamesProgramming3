using Microsoft.Xna.Framework;


namespace Lab5
{
    struct Daleks
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3 rotation;
        public float speed;
        public bool isActive;
        public float posSpeed;
        public float negSpeed;
        public bool up;

        public void Update(float delta)
        {
            position += direction * speed * GameConstants.DalekSpeedAdjustment * delta;
            posSpeed = GameConstants.posSpeed * delta;
            negSpeed = GameConstants.negSpeed * delta;

            if (up)
            {
                position.Y += posSpeed;
            }
            else
            {
                position.Y += negSpeed;
            }

            if (position.X > GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
            }
            if (position.X < -GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
            }


            if (position.Y >= GameConstants.PlayfieldmaxY)
            {
                up = false;
            }
            if (position.Y <= GameConstants.PlayfieldminY)
            {
                up = true;
            }


        }
    }
}
