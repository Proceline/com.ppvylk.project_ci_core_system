namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public class SpawnableGridUnit : GridUnit
    {
        protected override void HandleDeath()
        {
            //Don't relay anything to the gamemanager.
        }
    }
}
