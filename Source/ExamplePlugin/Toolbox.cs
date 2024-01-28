namespace Faithful
{
    internal class Toolbox
    {
        // Tool references
        public Behaviour behaviour;
        public Utils utils;

        // Constructor
        public Toolbox()
        {
            // Create tools
            behaviour = new Behaviour(this);
            utils = new Utils(this);

            Log.Debug("Toolbox built");
        }
    }
}
