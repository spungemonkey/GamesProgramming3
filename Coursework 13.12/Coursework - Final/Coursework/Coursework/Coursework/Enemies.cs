using Microsoft.Xna.Framework;


namespace Lab5
{
    struct Enemies
    {
        //Variables
        public Vector3 position; //Enemy Position
        public Vector3 direction; //Enemy direction
        public Vector3 rotation; //Enemy rotation
        public float speed; //Enemy speed
        public bool isActive; //Enemy boolean to determine if enemy is active
        public float posSpeed; //Positive (up on the y-axis) speed
        public float negSpeed; // Negative (down on the y-axis) speed
        public bool up; //determines if enemy moves up or down

        public void Update(float delta)
        {

            position += direction * speed * GameConstants.DFGSpeedAdjustment * delta; //update position of the enemy

            posSpeed = GameConstants.posSpeed * delta; //sets posSpeed. Multiplies by delta for smooth movement
            negSpeed = GameConstants.negSpeed * delta; //sets negSpeed. Multiplies by delta for smooth movement

            //determines what direction the enemy moves depending on the valye of "up"
            if (up)
            {
                position.Y += posSpeed;
            }
            else
            {
                position.Y += negSpeed;
            }


            //changes direction depending on location in the world. If it is bigger than the playing field it changes direction
            if (position.X > GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
            }
            if (position.X < -GameConstants.PlayfieldSizeX)
            {
                direction.X = direction.X * -1;
            }


            //alters value of Up depending on position
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
