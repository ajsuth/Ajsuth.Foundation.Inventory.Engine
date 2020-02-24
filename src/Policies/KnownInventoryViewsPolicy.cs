// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnownInventoryViewsPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine.Policies
{
    /// <summary>
    /// Defines the known inventory Views.
    /// </summary>
    public class KnownInventoryViewsPolicy : Sitecore.Commerce.Plugin.Inventory.KnownInventoryViewsPolicy
    {
        /// <summary>
        /// Gets or sets the select inventory set view name.
        /// </summary>
        public string SelectInventorySet { get; set; } = nameof(SelectInventorySet);
    }
}