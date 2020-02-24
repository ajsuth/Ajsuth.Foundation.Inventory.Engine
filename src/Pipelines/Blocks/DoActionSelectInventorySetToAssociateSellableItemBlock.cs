// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoActionSelectInventorySetToAssociateSellableItemBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine.Pipelines.Blocks
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Inventory;
    using Sitecore.Framework.Pipelines;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the do action select inventory set to associate sellable item block.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.EntityViews.EntityView,
    ///         Sitecore.Commerce.EntityViews.EntityView, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName(Engine.InventoryConstants.Pipelines.Blocks.DoActionSelectInventorySetToAssociateSellableItem)]
    public class DoActionSelectInventorySetToAssociateSellableItemBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        protected readonly CommerceCommander Commander;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoActionSelectInventorySetToAssociateSellableItemBlock"/> class.
        /// </summary>
        /// <param name="commander">The commander.</param>
        public DoActionSelectInventorySetToAssociateSellableItemBlock(CommerceCommander commander)
        {
            Commander = commander;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="entityView">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="EntityView"/>.</returns>
        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            var views = context.GetPolicy<Policies.KnownInventoryViewsPolicy>();
            var knownActionsPolicy = context.GetPolicy<Policies.KnownInventoryActionsPolicy>();
            var enablementPolicy = context.GetPolicy<Policies.InventoryFeatureEnablementPolicy>();
            if (!enablementPolicy.InventoryFromProductView
                || string.IsNullOrEmpty(entityView?.Action)
                || (string.IsNullOrEmpty(entityView.ItemId) && string.IsNullOrEmpty(entityView.EntityId))
                || string.IsNullOrEmpty(entityView.Name)
                || !entityView.Name.Equals(views.SelectInventorySet, StringComparison.OrdinalIgnoreCase)
                || !entityView.Action.Equals(knownActionsPolicy.SelectInventorySet, StringComparison.OrdinalIgnoreCase))
            {
                return entityView;
            }

            var sellableItemId =
                entityView.EntityId.StartsWith(CommerceEntity.IdPrefix<SellableItem>(), StringComparison.OrdinalIgnoreCase)
                    ? entityView.EntityId
                    : entityView.ItemId;

            var inventorySetProperty = entityView.GetProperty("Inventory Set");
            inventorySetProperty.UiType = string.Empty;
            inventorySetProperty.IsReadOnly = true;

            var inventorySetId = inventorySetProperty.Value;
            
            // Reset the entity view properties for the inventory set context
            entityView.EntityId = inventorySetId.ToEntityId<InventorySet>();
            entityView.ItemId = sellableItemId;
            entityView.EntityVersion = 1;
            entityView.SetPropertyValue("Version", 1);
            var version = entityView.GetProperty("Version");
            version.IsHidden = true;

            var variationId = string.Empty;
            if (sellableItemId.Contains("|"))
            {
                var parts = sellableItemId.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    sellableItemId = parts[0];
                    variationId = parts[1];
                }
            }

            var sellableItem = 
                await Commander.Pipeline<FindEntityPipeline>().Run(
                        new FindEntityArgument(typeof(SellableItem), sellableItemId),
                        context)
                    .ConfigureAwait(false) as SellableItem;

            if (sellableItem == null)
            {
                return entityView;
            }

            var inventoryInformation =
                await Commander.Command<GetInventoryInformationCommand>().Process(
                    context.CommerceContext,
                    inventorySetId,
                    sellableItem?.Id,
                    variationId,
                    true).ConfigureAwait(false);

            await AddViewProperties(entityView, sellableItem, variationId, inventoryInformation, context).ConfigureAwait(false);
            
            context.CommerceContext.AddModel(
                new MultiStepActionModel(
                    inventoryInformation != null
                        ? knownActionsPolicy.EditSellableItemInventory
                        : knownActionsPolicy.AssociateSellableItemToInventorySet));
            
            return entityView;
        }

        /// <summary>
        /// Adds the view properties.
        /// </summary>
        /// <param name="entityView">The entity view.</param>
        /// <param name="sellableItem">The sellable item.</param>
        /// <param name="variationId">The variation identifier.</param>
        /// <param name="inventoryInformation">The <see cref="InventoryInformation"/> of the sellable item for the inventory set.</param>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task AddViewProperties(
            EntityView entityView,
            SellableItem sellableItem,
            string variationId,
            InventoryInformation inventoryInformation,
            CommercePipelineExecutionContext context)
        {
            entityView.Properties.Add(new ViewProperty
            {
                Name = "SellableItem",
                RawValue = string.IsNullOrEmpty(variationId) ? sellableItem?.Id : $"{sellableItem.Id}|{variationId}",
                IsReadOnly = true
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = "Quantity",
                RawValue = inventoryInformation?.Quantity ?? 0
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = "InvoiceUnitPrice",
                RawValue = inventoryInformation?.InvoiceUnitPrice?.Amount ?? 0,
                OriginalType = typeof(decimal).FullName,
                IsRequired = false
            });

            var currencySet =
                await Commander.Command<GetCurrencySetCommand>().Process(
                    context.CommerceContext,
                    context.GetPolicy<GlobalCurrencyPolicy>().DefaultCurrencySet).ConfigureAwait(false);

            var defaultCurrency =
                context.GetPolicy<GlobalEnvironmentPolicy>().DefaultCurrency;

            entityView.Properties.Add(new ViewProperty
            {
                Name = "InvoiceUnitPriceCurrency",
                RawValue = inventoryInformation?.InvoiceUnitPrice?.CurrencyCode ?? defaultCurrency,
                IsRequired = false,
                IsReadOnly = false,
                Policies = new List<Policy>
                {
                    new AvailableSelectionsPolicy
                    {
                        List = currencySet.HasComponent<CurrenciesComponent>()
                            ? currencySet.GetComponent<CurrenciesComponent>().Currencies
                                .Select(c => new Selection {DisplayName = c.Code, Name = c.Code, IsDefault = c.Code.Equals(defaultCurrency, StringComparison.OrdinalIgnoreCase)}).ToList()
                            : new List<Selection>()
                    }
                }
            });

            // Preorder fields
            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(PreorderableComponent.Preorderable),
                RawValue = inventoryInformation.GetValueOrDefault<PreorderableComponent, bool>(x => x.Preorderable, false),
                IsRequired = false
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(PreorderableComponent.PreorderAvailabilityDate),
                RawValue = inventoryInformation.GetValueOrDefault<PreorderableComponent, DateTimeOffset?>(x => x.PreorderAvailabilityDate, string.Empty),
                OriginalType = typeof(DateTimeOffset).ToString(),
                IsRequired = false
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(PreorderableComponent.PreorderLimit),
                RawValue = inventoryInformation.GetValueOrDefault<PreorderableComponent, int?>(x => x.PreorderLimit, null),
                OriginalType = typeof(int).ToString(),
                IsRequired = false
            });

            // Backorder fields
            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(BackorderableComponent.Backorderable),
                RawValue = inventoryInformation.GetValueOrDefault<BackorderableComponent, bool>(x => x.Backorderable, false),
                IsRequired = false
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(BackorderableComponent.BackorderAvailabilityDate),
                RawValue = inventoryInformation.GetValueOrDefault<BackorderableComponent, DateTimeOffset?>(x => x.BackorderAvailabilityDate, string.Empty),
                OriginalType = typeof(DateTimeOffset).ToString(),
                IsRequired = false
            });

            entityView.Properties.Add(new ViewProperty
            {
                Name = nameof(BackorderableComponent.BackorderLimit),
                RawValue = inventoryInformation.GetValueOrDefault<BackorderableComponent, int?>(x => x.BackorderLimit, null),
                OriginalType = typeof(int).ToString(),
                IsRequired = false
            });
        }
    }
}
