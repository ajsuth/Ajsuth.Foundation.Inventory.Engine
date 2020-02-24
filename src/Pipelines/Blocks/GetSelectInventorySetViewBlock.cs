// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetSelectInventorySetViewBlock.cs" company="Sitecore Corporation">
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
    using Sitecore.Commerce.Plugin.Inventory;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines the get select inventory set view block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.EntityViews.EntityView,
    ///         Sitecore.Commerce.EntityViews.EntityView, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(Engine.InventoryConstants.Pipelines.Blocks.GetSelectInventorySetView)]
	public class GetSelectInventorySetViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
	{
        protected readonly CommerceCommander CommerceCommander;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSelectInventorySetViewBlock"/> class.
        /// </summary>
        /// <param name="commerceCommander">The commerce commander.</param>
        public GetSelectInventorySetViewBlock(CommerceCommander commerceCommander)
        {
            CommerceCommander = commerceCommander;
        }

        /// <summary>
        /// Executes the pipeline block.
        /// </summary>
        /// <param name="arg">The entity view.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
		{
            Condition.Requires(entityView).IsNotNull($"{Name}: The argument cannot be null");

            var viewsPolicy = context.GetPolicy<Policies.KnownInventoryViewsPolicy>();
            var entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();
            var enablementPolicy = context.GetPolicy<Policies.InventoryFeatureEnablementPolicy>();
            if (!enablementPolicy.InventoryFromProductView
                || string.IsNullOrEmpty(entityViewArgument?.ViewName)
                || !entityViewArgument.ViewName.Equals(viewsPolicy.SelectInventorySet, StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(entityView).ConfigureAwait(false);
            }

            var inventorySets =
                await CommerceCommander.Pipeline<FindEntitiesInListPipeline>().Run(
                    new FindEntitiesInListArgument(
                        typeof(InventorySet),
                        $"{CommerceEntity.ListName<InventorySet>()}",
                        0,
                        int.MaxValue),
                    context).ConfigureAwait(false);

            var availableSelectionsPolicy = new AvailableSelectionsPolicy(
                inventorySets.List.Items.Select(s =>
                    new Selection { DisplayName = s.DisplayName, Name = s.Name }).ToList()
                    ?? new List<Selection>());

            var viewProperty = new ViewProperty()
            {
                Name = "Inventory Set",
                UiType = "SelectList",
                Policies = new List<Policy>() { availableSelectionsPolicy },
                RawValue =
                    availableSelectionsPolicy.List.Where(s => s.IsDefault).FirstOrDefault()?.Name
                        ?? availableSelectionsPolicy.List?.FirstOrDefault().Name
            };
            
            entityView.Properties.Add(viewProperty);

            return entityView;
		}
	}
}