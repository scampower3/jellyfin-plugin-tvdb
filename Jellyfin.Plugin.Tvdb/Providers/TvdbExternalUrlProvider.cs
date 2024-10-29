using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Tvdb.Providers
{
    /// <summary>
    /// Provides external urls for TheTVDB.
    /// </summary>
    public class TvdbExternalUrlProvider : IExternalUrlProvider
    {
        private readonly FrozenSet<string> _supportedOrders = new[] { "official", "regional", "alternate", "altdvd", "dvd", "absolute" }.ToFrozenSet();

        /// <inheritdoc/>
        public string Name => TvdbPlugin.ProviderName;

        /// <inheritdoc/>
        public IEnumerable<string> GetExternalUrls(BaseItem item)
        {
            var externalId = item.GetProviderId(TvdbPlugin.ProviderId);
            var slugId = item.GetProviderId(TvdbPlugin.SlugProviderId);

            switch (item)
            {
                case Series:
                    if (string.IsNullOrEmpty(slugId) && !string.IsNullOrEmpty(externalId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"?tab=series&id={externalId}";
                    }
                    else if (!string.IsNullOrEmpty(slugId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"series/{slugId}";
                    }

                    break;
                case Season season:
                    season.Series.ProviderIds.TryGetValue(TvdbPlugin.SlugProviderId, out var seriesSlugId);
                    var displayOrder = season.Series.DisplayOrder;
                    if (string.IsNullOrEmpty(displayOrder))
                    {
                        displayOrder = "official";
                    }

                    if (_supportedOrders.Contains(displayOrder) && !string.IsNullOrEmpty(seriesSlugId) && !string.IsNullOrEmpty(externalId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"series/{seriesSlugId}/seasons/{displayOrder}/{item.IndexNumber}";
                    }
                    else if (string.Equals(displayOrder, "official", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(externalId))
                    {
                        // This url format only works for official order
                        yield return TvdbUtils.TvdbBaseUrl + $"dereferrer/season/{externalId}";
                    }

                    break;
                case Episode episode:
                    episode.Series.ProviderIds.TryGetValue(TvdbPlugin.SlugProviderId, out seriesSlugId);
                    if (!string.IsNullOrEmpty(seriesSlugId) && !string.IsNullOrEmpty(externalId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"series/{seriesSlugId}/episodes/{externalId}";
                    }
                    else if (!string.IsNullOrEmpty(externalId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"?tab=episode&id={externalId}";
                    }

                    break;
                case Movie:
                    if (!string.IsNullOrEmpty(slugId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"movies/{slugId}";
                    }

                    break;
                case Person:
                    if (!string.IsNullOrEmpty(externalId))
                    {
                        yield return TvdbUtils.TvdbBaseUrl + $"people/{externalId}";
                    }

                    break;
            }
        }
    }
}