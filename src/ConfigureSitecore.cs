// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Foundation.Inventory.Engine
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
	using Sitecore.Commerce.EntityViews;
	using Sitecore.Commerce.Plugin.Inventory;
	using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            // Configure pipelines
            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IGetEntityViewPipeline>(pipeline => pipeline
                    .Add<Pipelines.Blocks.GetSelectInventorySetViewBlock>().After<PopulateEntityVersionBlock>()
                )

                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(pipeline => pipeline
                    .Add<Pipelines.Blocks.PopulateSellableItemInventorySetsViewActionsBlock>().Before<PopulateInventorySkusViewActionsBlock>()
                )

                .ConfigurePipeline<IDoActionPipeline>(pipeline => pipeline
                    .Add<Pipelines.Blocks.DoActionSelectInventorySetToAssociateSellableItemBlock>().After<ValidateEntityVersionBlock>()
                )
            );

            services.RegisterAllCommands(assembly);
        }
    }
}