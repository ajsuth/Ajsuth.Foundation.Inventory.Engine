// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnownInventoryActionsPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine.Policies
{
    /// <summary>
    /// Defines the known inventory actions.
    /// </summary>
    public class KnownInventoryActionsPolicy : Sitecore.Commerce.Plugin.Inventory.KnownInventoryActionsPolicy
    {
        /// <summary>
        /// Gets or sets the associate sellable item to select inventory set action name.
        /// </summary>
        public string AssociateSellableItemToSelectInventorySet { get; set; } = nameof(AssociateSellableItemToSelectInventorySet);

        /// <summary>
        /// Gets or sets the select inventory set action name.
        /// </summary>
        public string SelectInventorySet { get; set; } = nameof(SelectInventorySet);
    }
}
