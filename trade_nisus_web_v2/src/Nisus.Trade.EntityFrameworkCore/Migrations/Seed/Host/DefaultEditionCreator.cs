using System.Linq;
using Abp.Application.Features;
using Microsoft.EntityFrameworkCore;
using Nisus.Trade.Editions;
using Nisus.Trade.EntityFrameworkCore;
using Nisus.Trade.Features;

namespace Nisus.Trade.Migrations.Seed.Host
{
    public class DefaultEditionCreator
    {
        private readonly TradeDbContext _context;

        public DefaultEditionCreator(TradeDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateEditions();
        }

        private void CreateEditions()
        {
            var defaultEdition = _context.Editions.IgnoreQueryFilters().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
            if (defaultEdition == null)
            {
                defaultEdition = new SubscribableEdition { Name = EditionManager.DefaultEditionName, DisplayName = EditionManager.DefaultEditionName };
                _context.Editions.Add(defaultEdition);
                _context.SaveChanges();

                /* Add desired features to the standard edition, if wanted... */
            }

#if FEATURE_SIGNALR
            if (defaultEdition.Id > 0)
            {
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.ChatFeature, true);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.TenantToTenantChatFeature, true);
                CreateFeatureIfNotExists(defaultEdition.Id, AppFeatures.TenantToHostChatFeature, true);
            }
#endif
        }

        private void CreateFeatureIfNotExists(int editionId, string featureName, bool isEnabled)
        {
            var defaultEditionChatFeature = _context.EditionFeatureSettings.IgnoreQueryFilters()
                                                        .FirstOrDefault(ef => ef.EditionId == editionId && ef.Name == featureName);

            if (defaultEditionChatFeature == null)
            {
                _context.EditionFeatureSettings.Add(new EditionFeatureSetting
                {
                    Name = featureName,
                    Value = isEnabled.ToString(),
                    EditionId = editionId
                });
            }
        }
    }
}