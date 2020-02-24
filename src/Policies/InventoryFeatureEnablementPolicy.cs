// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryFeatureEnablementPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine.Policies
{
    using Sitecore.Commerce.Core;
    /// <inheritdoc />
    /// <summary>
    /// Defines the inventory feature enablement policy
    /// </summary>
    /// <seealso cref="Policy" />
    public class InventoryFeatureEnablementPolicy : Policy
    {
        public bool InventoryFromProductView { get; set; }
    }
}
