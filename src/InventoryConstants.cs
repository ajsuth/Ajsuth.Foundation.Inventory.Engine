// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryConstants.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine
{
    /// <summary>
    /// The inventory constants.
    /// </summary>
    public static class InventoryConstants
    {
        /// <summary>
        /// The names of the pipelines.
        /// </summary>
        public static class Pipelines
        {
            /// <summary>
            /// The names of the pipeline blocks.
            /// </summary>
            public static class Blocks
            {
                /// <summary>
                /// The do action select inventory set to associate sellable item block name.
                /// </summary>
                public const string DoActionSelectInventorySetToAssociateSellableItem = "Inventory.Block.DoActionSelectInventorySetToAssociateSellableItem";
                
                /// <summary>
                /// The get select inventory set view block name.
                /// </summary>
                public const string GetSelectInventorySetView = "Inventory.Block.GetSelectInventorySetView";
                
                /// <summary>
                /// The populate sellable item inventory sets view actions block name.
                /// </summary>
                public const string PopulateSellableItemInventorySetsViewActions = "Inventory.Block.PopulateSellableItemInventorySetsViewActions";
            }
        }
    }
}