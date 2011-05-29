using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SeasonProvider _seasonsProvider;
        private readonly IRepository _repository;
        private readonly TvDbProvider _tvDbProvider;

        public EpisodeProvider(IRepository repository, SeasonProvider seasonProviderProvider, TvDbProvider tvDbProviderProvider)
        {
            _repository = repository;
            _tvDbProvider = tvDbProviderProvider;
            _seasonsProvider = seasonProviderProvider;
        }

        public EpisodeProvider()
        {
        }

        public virtual int AddEpisode(Episode episode)
        {
            return (Int32)_repository.Add(episode);
        }

        public virtual Episode GetEpisode(long id)
        {
            return _repository.Single<Episode>(id);
        }

        public virtual Episode GetEpisode(int seriesId, int seasonNumber, int episodeNumber)
        {
            return
                _repository.Single<Episode>(
                    c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber && c.EpisodeNumber == episodeNumber);
        }

        public virtual Episode GetEpisode(int seriesId, DateTime date)
        {
            return
                _repository.Single<Episode>(
                    c => c.SeriesId == seriesId && c.AirDate == date.Date);
        }

        public virtual IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            return _repository.Find<Episode>(e => e.SeriesId == seriesId);
        }

        public virtual IList<Episode> GetEpisodeBySeason(long seasonId)
        {
            return _repository.Find<Episode>(e => e.SeasonId == seasonId);
        }


        public virtual IList<Episode> EpisodesWithoutFiles(bool includeSpecials)
        {
            if (includeSpecials)
                return _repository.All<Episode>().Where(e => e.EpisodeFileId == 0 && e.AirDate <= DateTime.Today).ToList();

            return _repository.All<Episode>().Where(e => e.EpisodeFileId == 0 && e.AirDate <= DateTime.Today && e.SeasonNumber > 0).ToList();
        }


        public virtual void RefreshEpisodeInfo(Series series)
        {
            Logger.Info("Starting episode info refresh for series:{0}", series.SeriesId);
            int successCount = 0;
            int failCount = 0;
            var tvDbSeriesInfo = _tvDbProvider.GetSeries(series.SeriesId, true);

            var updateList = new List<Episode>();
            var newList = new List<Episode>();

            Logger.Debug("Updating season info for series:{0}", tvDbSeriesInfo.SeriesName);
            tvDbSeriesInfo.Episodes.Select(e => new { e.SeasonId, e.SeasonNumber })
                .Distinct().ToList()
                .ForEach(s => _seasonsProvider.EnsureSeason(series.SeriesId, s.SeasonId, s.SeasonNumber));

            foreach (var episode in tvDbSeriesInfo.Episodes)
            {
                try
                {
                    //DateTime throws an error in SQLServer per message below:
                    //SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.
                    //So lets hack it so it works for SQLServer (as well as SQLite), perhaps we can find a better solution
                    //Todo: Fix this hack
                    if (episode.FirstAired < new DateTime(1753, 1, 1))
                        episode.FirstAired = new DateTime(1753, 1, 1);

                    Logger.Trace("Updating info for [{0}] - S{1}E{2}", tvDbSeriesInfo.SeriesName, episode.SeasonNumber, episode.EpisodeNumber);

                    var episodeToUpdate = GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber);

                    if (episodeToUpdate == null)
                    {
                        episodeToUpdate = new Episode();
                        newList.Add(episodeToUpdate);
                    }
                    else
                    {
                        updateList.Add(episodeToUpdate);
                    }

                    episodeToUpdate.SeriesId = series.SeriesId;
                    episodeToUpdate.TvDbEpisodeId = episode.Id;
                    episodeToUpdate.AirDate = episode.FirstAired.Date;
                    episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
                    episodeToUpdate.SeasonId = episode.SeasonId;
                    episodeToUpdate.SeasonNumber = episode.SeasonNumber;
                    episodeToUpdate.Title = episode.EpisodeName;
                    episodeToUpdate.Overview = episode.Overview;

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(
                        String.Format("An error has occurred while updating episode info for series {0}", series.SeriesId), e);
                    failCount++;
                }
            }

            _repository.AddMany(newList);
            _repository.UpdateMany(updateList);

            Logger.Debug("Finished episode refresh for series:{0}. Successful:{1} - Failed:{2} ",
                         tvDbSeriesInfo.SeriesName, successCount, failCount);
        }

        public virtual void DeleteEpisode(int episodeId)
        {
            _repository.Delete<Episode>(episodeId);
        }

        public virtual void UpdateEpisode(Episode episode)
        {
            _repository.Update(episode);
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      