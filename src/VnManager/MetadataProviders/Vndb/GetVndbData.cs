using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Producer;
using VndbSharp.Models.Release;
using VndbSharp.Models.Staff;
using VndbSharp.Models.VisualNovel;
using VnManager.Helpers.Vndb;

namespace VnManager.MetadataProviders.Vndb
{
    public class GetVndbData
    {
        private Stopwatch stopwatch = new Stopwatch();
		private TimeSpan maxTime = TimeSpan.FromMinutes(3);
        private bool didErrorOccur = false;
		public async Task GetData(uint id)
        {
			uint vnid = id;
			try
			{
				using (var client = new VndbSharp.Vndb(true))
				{
					RequestOptions ro = new RequestOptions { Count = 25 };
					stopwatch.Start();
					var visualNovel = await GetVisualNovel(client, vnid);
					var releases = await GetReleases(client, vnid, ro);
					uint[] producerIds = releases.SelectMany(x => x.Producers.Select(y => y.Id)).Distinct().ToArray();
					var producers = await GetProducers(client, producerIds, ro);
					var characters = await GetCharacters(client, vnid, ro);

					uint[] staffIds = visualNovel.Staff.Select(x => x.StaffId).Distinct().ToArray();
					var staff = await GetStaff(client, staffIds, ro);
					stopwatch.Stop();
					stopwatch.Reset();

					
					if(didErrorOccur)
					{
						App.Logger.Error("Failed to get all of the Vndb Info from the API, one of the items was null");
						//stop the progressbar here, and force it to show an error icon
					}
					else
					{
						//run code to add info to database
                        SaveVnDataToDb foo = new SaveVnDataToDb();
						foo.SaveVnInfo(visualNovel);
						foo.SaveVnCharacters(characters, vnid);
						foo.SaveVnReleases(releases);
						foo.SaveProducers(producers);
						foo.SaveStaff(staff, (int)vnid);
                    }

				}
			}
			catch (Exception ex)
			{
				App.Logger.Error(ex, "An error occured when trying to get the vndb data from the API");
				throw;
			}
        }

		internal async Task<VisualNovel> GetVisualNovel(VndbSharp.Vndb client, uint vnid)
		{
			stopwatch.Restart();
			while (true)
			{
                if (stopwatch.Elapsed > maxTime) return null;
				VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid), VndbFlags.FullVisualNovel);


				switch (visualNovels)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                        return null;
                    default:
                        return visualNovels.First();
                }
				
			}
		}

		internal async Task<List<Release>> GetReleases(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
		{
			stopwatch.Restart();
			bool hasMore = true;
			int pageCount = 1;
			int releasesCount = 0;
			List<Release> releaseList = new List<Release>();
			bool shouldContinue = true;
			while (hasMore && shouldContinue)
			{
				shouldContinue = true;
				ro.Page = pageCount;
				VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullRelease, ro);

				switch (releases)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = releases.HasMore;
                        releaseList.AddRange(releases.Items);
                        releasesCount = releasesCount + releases.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime) return null;
                        break;
                    }
                }
				
			}
			return releaseList;

		}

		internal async Task<List<Character>> GetCharacters(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
		{
			stopwatch.Restart();
			bool hasMore = true;
			int pageCount = 1;
			int characterCount = 0;
			List<Character> characterList = new List<Character>();
			bool shouldContinue = true;
			while (hasMore && shouldContinue)
			{
				shouldContinue = true;
				ro.Page = pageCount;
				VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullCharacter, ro);

				switch (characters)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = characters.HasMore;
                        characterList.AddRange(characters.Items);
                        characterCount = characterCount + characters.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime) return null;
                        break;
                    }
                }
				
			}
			return characterList;
		}

		internal async Task<List<Producer>> GetProducers(VndbSharp.Vndb client, uint[] producerIdList, RequestOptions ro)
		{
			stopwatch.Restart();
			bool hasMore = true;
			int pageCount = 1;
			int producerCount = 0;
			List<Producer> producerList = new List<Producer>();
			bool shouldContinue = true;
			while (hasMore && shouldContinue)
			{
				shouldContinue = true;
				ro.Page = pageCount;
				var producers = await client.GetProducerAsync(VndbFilters.Id.Equals(producerIdList), VndbFlags.FullProducer, ro);

				if (producers == null && client.GetLastError().Type == ErrorType.Throttled)
				{
					await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
				}
				else if (producers == null)
				{
					HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                    didErrorOccur = true;
                    return null;
				}
				else
				{
					shouldContinue = false;
                    hasMore = producers.HasMore;
                    producerList.AddRange(producers.Items);
                    producerCount = producerCount + producers.Count;
                    pageCount++;
                    if (stopwatch.Elapsed > maxTime) return null;
				}
				
			}
			return producerList;
		}

		internal async Task<List<Staff>> GetStaff(VndbSharp.Vndb client, uint[] staffId, RequestOptions ro)
		{
			stopwatch.Restart();
			bool hasMore = true;
			int pageCount = 1;
			int staffCount = 0;
			List<Staff> staffList = new List<Staff>();
			bool shouldContinue = true;
			while (hasMore && shouldContinue)
			{
				shouldContinue = true;
				ro.Page = pageCount;
				VndbResponse<Staff> staff = await client.GetStaffAsync(VndbFilters.Id.Equals(staffId), VndbFlags.FullStaff, ro);

				switch (staff)
                {
                    case null when client.GetLastError().Type == ErrorType.Throttled:
                        await HandleVndbErrors.ThrottledWait((ThrottledError)client.GetLastError(), 0);
                        break;
                    case null:
                        HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
                        didErrorOccur = true;
                        return null;
                    default:
                    {
                        shouldContinue = false;
                        hasMore = staff.HasMore;
                        staffList.AddRange(staff.Items);
                        staffCount = staffCount + staff.Count;
                        pageCount++;
                        if (stopwatch.Elapsed > maxTime) return null;
                        break;
                    }
                }
				
			}
			return staffList;

		}
		
    }
}
