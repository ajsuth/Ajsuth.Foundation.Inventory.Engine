// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PopulateSellableItemInventorySetsViewActionsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the populate sellable item inventory sets view actions block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.EntityViews.EntityView,
    ///         Sitecore.Commerce.EntityViews.EntityView, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(Engine.InventoryConstants.Pipelines.Blocks.PopulateSellableItemInventorySetsViewActions)]
    public class PopulateSellableItemInventorySetsViewActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
		/// Executes the pipeline block.
		/// </summary>
		/// <param name="entityView">The entity view.</param>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            var viewsPolicy = context.GetPolicy<Policies.KnownInventoryViewsPolicy>();
            var enablementPolicy = context.GetPolicy<Policies.InventoryFeatureEnablementPolicy>();
            if (!enablementPolicy.InventoryFromProductView
                || string.IsNullOrEmpty(entityView?.Name)
                || !entityView.Name.Equals(viewsPolicy.SellableItemInventorySets, StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(entityView.Action))
            {
                return Task.FromResult(entityView);
            }

            var entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            var sellableItem = entityViewArgument?.Entity as SellableItem;
            if (sellableItem == null)
            {
                return Task.FromResult(entityView);
            }
            
            var knownActionsPolicy = context.GetPolicy<Policies.KnownInventoryActionsPolicy>();
            var actionPolicy = entityView.GetPolicy<ActionsPolicy>();

            var isVariation = !string.IsNullOrWhiteSpace(entityViewArgument.ItemId);
            var isProductMaster =
                sellableItem.HasComponent<ItemVariationsComponent>()
                    ? sellableItem.GetComponent<ItemVariationsComponent>().Variations.Any()
                    : false;

            actionPolicy.Actions.Add(
                new EntityActionView(new List<Policy>
                    {
                        new MultiStepActionPolicy
                        {
                            FirstStep = new EntityActionView
                            {
                                Name = knownActionsPolicy.SelectInventorySet,
                                DisplayName = "Select Inventory Set",
                                Description = "Select inventory set",
                                RequiresConfirmation = false,
                                IsEnabled = true,
                                EntityView = viewsPolicy.SelectInventorySet
                            }
                        }
                    })
                {
                    Name = knownActionsPolicy.AssociateSellableItemToSelectInventorySet,
                    DisplayName = "Associate Sellable Item to an inventory set",
                    Description = "Associates a sellable to an inventory set",
                    IsEnabled = isVariation || !isProductMaster,
                    EntityView = string.Empty,
                    Icon = "link"
                });

            return Task.FromResult(entityView);
        }
    }
}
