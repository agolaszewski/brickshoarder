namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public interface IFlowComponent
    {
        public Type Type { get; }

        void Build();
    }
}