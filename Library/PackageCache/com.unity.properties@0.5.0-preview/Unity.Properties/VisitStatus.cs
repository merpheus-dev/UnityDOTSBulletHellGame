namespace Unity.Properties
{
    public enum VisitStatus
    {
        /// <summary>
        /// Visitation is NOT being handled. Scopes should NOT be opened and control should fall the the next available handler.
        /// </summary>
        Unhandled = 0,

        /// <summary>
        /// Visitation is being handled. Use the default behaviour of nesting in to containers and visiting list elements.
        /// </summary>
        Handled = 1,

        /// <summary>
        /// Visitation is being handled. Override the default pattern of nesting in to containers or visiting list elements.
        /// </summary>
        Override = 2
    }
}
