using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Producer;
using VndbSharp.Models.Release;
using VndbSharp.Models.Staff;
using VndbSharp.Models.VisualNovel;
using VnManager.Helpers.Vndb;

namespace VnManager.MetadataProviders.Vndb
{
    public class GetVndbData
    {
        public async Task GetData(uint id)
        {
			uint vnid = id;
			try
			{
				using (var client = new VndbSharp.Vndb(true))
				{
					RequestOptions ro = new RequestOptions { Count = 25 };

					var visualNovel = await GetVisualNovel(client, vnid);
					var releases = await GetReleases(client, vnid, ro);
					uint[] producerIds = releases.SelectMany(x => x.Producers.Select(y => y.Id)).Distinct().ToArray();
					var producers = await GetProducers(client, producerIds, ro);
					var characters = await GetCharacters(client, vnid, ro);

					var staffIds = visualNovel.Staff.Select(x => x.StaffId).Distinct().ToArray();
					var staff = await GetStaff(client, staffIds, ro);
				}
			}
			catch (Exception ex)
			{

				throw;
			}
        }

		private async Task<VisualNovel> GetVisualNovel(VndbSharp.Vndb client, uint vnid)
		{
			VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid), VndbFlags.FullVisualNovel);
			if (visualNovels == null)
			{
				HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
				return null;
			}
			else return visualNovels.First();
		}

		private async Task<List<Release>> GetReleases(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
		{
			bool hasMore = true;
			int pageCount = 1;
			int releasesCount = 0;
			List<Release> releaseList = new List<Release>();
			while (hasMore)
			{
				ro.Page = pageCount;
				VndbResponse<Release> releases = await client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullRelease, ro);
				if (releases == null)
				{
					HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
					break;
				}
				hasMore = releases.HasMore;
				releaseList.AddRange(releases.Items);
				releasesCount = releasesCount + releases.Count;
				pageCount++;
			}
			return releaseList;
		}

		private async Task<List<Character>> GetCharacters(VndbSharp.Vndb client, uint vnid, RequestOptions ro)
		{
			bool hasMore = true;
			int pageCount = 1;
			int characterCount = 0;
			List<Character> characterList = new List<Character>();
			while (hasMore)
			{
				ro.Page = pageCount;
				VndbResponse<Character> characters = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(vnid), VndbFlags.FullCharacter, ro);
				if (characters != null)
				{
					hasMore = characters.HasMore;
					characterList.AddRange(characters.Items);
					characterCount = characterCount + characters.Count;
					pageCount++;
				}
				if (characters != null) continue;
				HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
				return;
			}
			return characterList;
		}

		private async Task<List<Producer>> GetProducers(VndbSharp.Vndb client, uint[] producerIdList, RequestOptions ro)
		{
			try
			{
				bool hasMore = true;
				int pageCount = 1;
				int producerCount = 0;
				List<Producer> producerList = new List<Producer>();
				while (hasMore)
				{
					ro.Page = pageCount;
					var producers = await client.GetProducerAsync(VndbFilters.Id.Equals(producerIdList), VndbFlags.FullProducer, ro);
					if (producers == null)
					{
						HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
						break;
					}
					hasMore = producers.HasMore;
					producerList.AddRange(producers.Items);
					producerCount = producerCount + producers.Count;
					pageCount++;
				}
				return producerList;
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		private async Task<List<Staff>> GetStaff(VndbSharp.Vndb client, uint[] staffId, RequestOptions ro)
		{
			bool hasMore = true;
			int pageCount = 1;
			int staffCount = 0;
			List<Staff> staffList = new List<Staff>();
			while (hasMore)
			{
				ro.Page = pageCount;				
				VndbResponse<Staff> staff = await client.GetStaffAsync(VndbFilters.Id.Equals(staffId), VndbFlags.FullStaff, ro);
				if (staff == null)
				{
					HandleVndbErrors.HandleErrors(client.GetLastError(), 0);
					break;
				}
				hasMore = staff.HasMore;
				staffList.AddRange(staff.Items);
				staffCount = staffCount + staff.Count;
				pageCount++;
			}
			return staffList;
		}
		
    }
}
