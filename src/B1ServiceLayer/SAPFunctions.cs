namespace B1ServiceLayer;

public static class SAPFunctions
{
    /// <summary>
    /// Verify if specified value is part of target SAP model property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The values should be provided at the exact order.
    ///     </para>
    ///     
    ///     <para>
    ///         This method does not work outside of SAPQuery and will throw <see cref="NotImplementedException"/>.
    ///     </para>
    /// </remarks>
    /// <param name="value"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool SubstringOf(string value, string property)
        => throw new NotImplementedException();
}
