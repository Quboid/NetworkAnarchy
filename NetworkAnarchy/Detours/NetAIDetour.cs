using NetworkAnarchy.Redirection;

namespace NetworkAnarchy.Detours
{
    [TargetType(typeof(NetAI))]
    public class NetAIDetour : NetAI
    {
        [RedirectMethod]
        public override bool BuildOnWater()
        {
            return true;
        }
    }
}
