namespace Mutators.Services.Selection
{
    /// <summary>
    /// Context for mutator selection.
    /// </summary>
    /// <param name="amountToPick">The amount of mutators that need to be picked.</param>
    public class MutatorSelectionContext(uint amountToPick)
    {
        /// <summary>
        /// The amount of mutators that need to be picked.
        /// </summary>
        public uint AmountToPick { get; } = amountToPick;
    }
}