using System;
using System.Linq;
using System.Net.Http;
#if UserAuth
using System.Security;
#endif
using System.Text;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Extensions;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.Common;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Release;
using VndbSharp.Models.VisualNovel;

namespace VndbConsole
{
	public class Program
	{
		public static void Main(String[] args)
			=> new Program().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

		public async Task MainAsync(String[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;

			// Redundant, but it's an example!
			VndbUtils.WithHttpMessageHandler(() => Task.FromResult((HttpMessageHandler) new HttpClientHandler()));

			this._client = new Vndb(true)
				.WithClientDetails("VndbSharpExamples", "0.1")
				.WithFlagsCheck(true, this.InvalidFlagsCallback);
			
//			await this.GetDatabaseStats();
//			await this.GetVisualNovelAsync();
//			await this.GetReleaseAsync();
//			await this.GetProducerAsync();
//			await this.GetCharacterAsync();
//		    await this.GetStaffAsync();
//			await this.GetUserAsync();
//			await this.GetVoteListAsync();
//			await this.GetVisualNovelListAsync();
//			await this.GetWishlistAsync();

//			await this.GetFilterExampleAsync();
//			await this.GetInvalidFlagsExampleAsync();
//			await this.GetErrorExampleAsync();

			this._client.Logout(); // Same as this._client.Dispose();

#if UserAuth
			var userPass = this.GetUsernameAndPassword();
			// Are all usernames forced lower? I could have sworn i registered with captials >:|
			this._client = new Vndb(userPass.Item1.ToLower(), userPass.Item2)
				.WithClientDetails("VndbSharpExamples", "0.1")
				.WithFlagsCheck(true, this.InvalidFlagsCallback);

			Console.WriteLine();

//			await this.SetVoteListAsync();
//			await this.SetVisualNovelListAsync();
//			await this.SetWishlistAsync();

			this._client.Logout(); // Not the same as this._client.Dispose();, it also immediately unsets the password.
#endif

			Boolean doRaw;
			while (!Boolean.TryParse(this.GetUserInput("Try Raw Input (True / False): "), out doRaw)) ;

			if (doRaw)
			{
				Console.WriteLine("Type \"exit\" to stop");

				String input;
				while ((input = this.GetUserInput("> ")) != "exit")
					Console.WriteLine(await this._client.DoRawAsync(input));
				Console.WriteLine();
			}
			
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		public async Task GetDatabaseStats()
		{
			Console.WriteLine("Get Database Stats Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetDatabaseStatsAsync();");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: dbstats");
			Console.WriteLine();

			var stats = await this._client.GetDatabaseStatsAsync();

			Console.WriteLine("Vndb has..." + Environment.NewLine +
							  $"{stats.Users} Users, who have made {stats.Threads} forum threads, with {stats.Posts} posts" + Environment.NewLine +
							  $"{stats.VisualNovels} Visual Novels, with {stats.Releases} releases and {stats.Tags} tags made by {stats.Producers} producers," + Environment.NewLine +
//							  $"{stats.Producers} Producers," + Environment.NewLine +
							  $"and {stats.Characters} Characters, with {stats.Traits} traits.");

			Console.WriteLine();
		}

		public async Task GetVisualNovelAsync()
		{
			Console.WriteLine("Get Visual Novel Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17), VndbFlags.FullVisualNovel);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get vn basic,details,anime,relations,tags,stats,screens (id=17)");
			Console.WriteLine();

			var visualNovels = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17), VndbFlags.FullVisualNovel);
			// Check for an error (Indicated by a null return)
			if (visualNovels == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (visualNovels.Count == 0)
			{
				Console.WriteLine("No Visual Novels Found!");
				return; // End method
			}

			// VndbResponse<T> implements IEnumerable<T>, so you can foreach it!
			foreach (var visualNovel in visualNovels)
			{
				Console.WriteLine($"{visualNovel?.Name} is roughly " +
								  $"{visualNovel?.Length?.Description() ?? "Unknown"} long, " +
								  $"has {visualNovel?.Tags.Count} tags " +
								  $"and {visualNovel?.Screenshots.Count} screenshots");
			}

			if (visualNovels.HasMore)
				Console.WriteLine("And more visual novels were found!");

			Console.WriteLine();
		}

		public async Task GetReleaseAsync()
		{
			Console.WriteLine("Get Release Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(17), VndbFlags.FullRelease);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get release basic,details,vn,producers (vn=17)");
			Console.WriteLine();

			var releases = await this._client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(17), VndbFlags.FullRelease);

			if (releases == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (releases.Count == 0)
			{
				Console.WriteLine("No Releases Found!");
				return; // End method
			}

			var englishRelease = releases.FirstOrDefault(r => r.Languages[0] == "en" && r.Type == ReleaseType.Complete);

			// Yeah i dunno. Here's a primitive example!
			Console.WriteLine($"Ever17 had a {englishRelease.MinimumAge}+ English Release on {englishRelease.Released} " +
							  $"that was {englishRelease.Type}, on {englishRelease.Platforms[0]}.");

			if (releases.HasMore) // This doesn't really work since we only show 1
				Console.WriteLine("And other more releases!");

			Console.WriteLine();
		}

		public async Task GetProducerAsync()
		{
			Console.WriteLine("Get Producer Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetReleaseAsync(VndbFilters.Id.Equals(9), VndbFlags.FullRelease);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get producer basic,details,relations (id=17)");
			Console.WriteLine();

			// 9 = Hiramaki International, Publisher for Ever17 in English.
			var producers = await this._client.GetProducerAsync(VndbFilters.Id.Equals(9), VndbFlags.FullProducer);

			if (producers == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (producers.Count == 0)
			{
				Console.WriteLine("No Producers Found!");
				return; // End method
			}

			var producer = producers.FirstOrDefault(); // Expecting only 1

			// Yeah i dunno. Here's a primitive example!
			Console.WriteLine($"{producer.Name} can be found at {producer.Links.Homepage} and primarily released in {producer.Language}.");

			if (producers.HasMore) // This doesn't really work since we only show 1
				Console.WriteLine("More producers were found as well!");

			Console.WriteLine();
		}

		public async Task GetCharacterAsync()
		{
			Console.WriteLine("Get Producer Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(17), VndbFlags.FullCharacter);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get character basic,details,meas,traits,vns (vn=17)");
			Console.WriteLine();

			var characters = await this._client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(17), VndbFlags.FullCharacter);

			if (characters == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (characters.Count == 0)
			{
				Console.WriteLine("No Characters Found!");
				return; // End method
			}

			// Yeah i dunno. Here's a primitive example!
			Console.WriteLine($"Ever17 has {characters.Count} characters.");
			foreach (var character in characters)
			{
				Console.WriteLine($"A {character.Gender} named {character.Name} (" +
					$"{character.OriginalName}), with Blood Type {(character.BloodType.HasValue ? character.BloodType.ToString() : "Unknown")}.");
			}

			if (characters.HasMore)
				Console.WriteLine("And more!");

			Console.WriteLine();
		}

	    public async Task GetStaffAsync()
	    {
			Console.WriteLine("Get Staff Command");
	        // Lib = This Library (VndbSharp)
	        Console.WriteLine("Lib Usage: await this._client.GetStaffAsync(VndbFilters.Id.Equals(2), VndbFlags.FullStaff);");
	        // Api = Vndb Tcp Api
	        Console.WriteLine("Api Usage: get staff basic,details,aliases,vns,voiced (id=2)");
	        Console.WriteLine();

	        var staff = await this._client.GetStaffAsync(VndbFilters.Id.Equals(2), VndbFlags.FullStaff);

	        if (staff == null)
	        {
	            this.HandleError(this._client.GetLastError());
	            return; // End method
	        }

	        // Check to see if we got any results
	        if (staff.Count == 0)
	        {
	            Console.WriteLine("No Staff Found!");
	            return; // End method
	        }

	        // Yeah i dunno. Here's a primitive example!
	        Console.WriteLine($"There are {staff.Count} staff members.");
	        foreach (var member in staff)
	        {
	            Console.WriteLine($"A {member.Gender} named {member.Name} (" +
	                              $"{member.OriginalName}), with Language {member.Language}.");
	        }

	        if (staff.HasMore)
	            Console.WriteLine("And more!");

	        Console.WriteLine();
		}

		public async Task GetUserAsync()
		{
			Console.WriteLine("Get User Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetUserAsync(VndbFilters.Id.Equals(2), VndbFlags.FullUser);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get user basic (id=2)");
			Console.WriteLine();

			var users = await this._client.GetUserAsync(VndbFilters.Id.Equals(2), VndbFlags.FullUser);

			if (users == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (users.Count == 0)
			{
				Console.WriteLine("No Users Found!");
				return; // End method
			}

			Console.WriteLine($"Found user {users.First().Username}");

			if (users.HasMore)
				Console.WriteLine("And more users were found!");

			Console.WriteLine();
		}

		public async Task GetVoteListAsync()
		{
			Console.WriteLine("Get VoteList Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetVoteListAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullVotelist);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get votelist basic (uid=2)");
			Console.WriteLine();

			var votes = await this._client.GetVoteListAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullVotelist);

			if (votes == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (votes.Count == 0)
			{
				Console.WriteLine("No Votes Found!");
				return; // End method
			}

			var ids = votes.Select(vl => vl.VisualNovelId);
			var vns = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(ids.ToArray()));

			if (vns == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			Console.WriteLine("Yorhel has voted on...");
			foreach (var vote in votes)
			{
				var vn = vns.FirstOrDefault(v => v.Id == vote.VisualNovelId);
				var vnName = vn == default(VisualNovel) ? "A Deleted VN" : vn.Name;
				Console.WriteLine($"{vnName} with a score of {vote.Vote / 10}/10, since {vote.AddedOn}");
			}

			if (votes.HasMore)
				Console.WriteLine("And more!");

			Console.WriteLine();
		}

		public async Task GetVisualNovelListAsync()
		{
			Console.WriteLine("Get VisualNovelList Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetVoteListAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullVisualNovelList);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get vnlist basic (uid=2)");
			Console.WriteLine();

			var vnList = await this._client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullVisualNovelList);

			if (vnList == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (vnList.Count == 0)
			{
				Console.WriteLine("No Votes Found!");
				return; // End method
			}

			var ids = vnList.Select(vnl => vnl.VisualNovelId);
			var vns = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(ids.ToArray()));

			if (vns == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}


			Console.WriteLine("Yorhel has played...");
			foreach (var vn in vnList)
			{
				var vnInfo = vns.FirstOrDefault(v => v.Id == vn.VisualNovelId);
				var vnName = vnInfo == default(VisualNovel) ? "A Deleted VN" : vnInfo.Name;
				Console.WriteLine($"{vnName} and currently has it set to {vn.Status}, since {vn.AddedOn}");
			}

			if (vnList.HasMore)
				Console.WriteLine("And more!");

			Console.WriteLine();
		}

		public async Task GetWishlistAsync()
		{
			Console.WriteLine("Get Wishlist Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetWishlistAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullWishlist);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get wishlist basic (uid=2)");
			Console.WriteLine();

			var wishlist = await this._client.GetWishlistAsync(VndbFilters.UserId.Equals(2), VndbFlags.FullWishlist);

			if (wishlist == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (wishlist.Count == 0)
			{
				Console.WriteLine("No Votes Found!");
				return; // End method
			}

			var ids = wishlist.Select(wl => wl.VisualNovelId);
			var vns = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(ids.ToArray()));

			if (vns == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}


			Console.WriteLine("Yorhel wants to play...");
			foreach (var vn in wishlist)
			{
				var vnInfo = vns.FirstOrDefault(v => v.Id == vn.VisualNovelId);
				var vnName = vnInfo == default(VisualNovel) ? "A Deleted VN" : vnInfo.Name;
				Console.WriteLine($"{vnName} and currently has it set to {vn.Priority} priority, since {vn.AddedOn}");
			}

			if (wishlist.HasMore)
				Console.WriteLine("And more!");

			Console.WriteLine();
		}

		// TODO: WIP
		public async Task GetFilterExampleAsync()
		{
			Console.WriteLine("Get Filter Example");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17, 18) " +
							  "| VndbFilters.Search.Fuzzy(\"aokana\"), VndbFlags.FullVisualNovel);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get vn basic (id=[17,18] or search~\"aokana\")");
			Console.WriteLine();
			
			var vns = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17, 18) | VndbFilters.Search.Fuzzy("aokana"), VndbFlags.FullVisualNovel);
			// Check for an error (Indicated by a null return)
			if (vns == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // End method
			}

			// Check to see if we got any results
			if (vns.Count == 0)
			{
				Console.WriteLine("No Visual Novels Found!");
				return; // End method
			}

			Console.WriteLine($"Found {vns.Count}{(vns.HasMore ? "+" : "")} Visual Novels");
			// VndbResponse<T> implements IEnumerable<T>, so you can foreach it!
			foreach (var visualNovel in vns)
			{
				Console.WriteLine($"{visualNovel?.Name} is roughly " +
								  $"{visualNovel?.Length?.Description() ?? "Unknown"} long, " +
								  $"has {visualNovel?.Tags.Count} tags " +
								  $"and {visualNovel?.Screenshots.Count} screenshots");
			}

			Console.WriteLine();
		}
		
		public async Task GetInvalidFlagsExampleAsync()
		{
			Console.WriteLine("Get Invalid Flags Example");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17), VndbFlags.FullCharacter);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: get vn basic,details,meas,traits,vns (id=17)");
			Console.WriteLine();

			// meas,traits,vns are not valid flags for 'get vn'
			var vns = await this._client.GetVisualNovelAsync(VndbFilters.Id.Equals(17), VndbFlags.FullCharacter);

			if (vns == null)
			{
				this.HandleError(this._client.GetLastError());
				return; // Exit Method
			}

			Console.WriteLine("Voodoo has occurred.");
		}

		public async Task GetErrorExampleAsync()
		{
			Console.WriteLine("Get Error Example");
			// Lib = This Library (VndbSharp)
			Console.WriteLine("Lib Usage: await this._client.SetVoteListAsync(0, 101);");
			// Api = Vndb Tcp Api
			Console.WriteLine("Api Usage: set votelist 0 {\"vote\": 101}");
			Console.WriteLine();

			// Invalid visual novel id, and score is too high.
			if (!await this._client.SetVoteListAsync(0, 101))
			{
				this.HandleError(this._client.GetLastError());
				return; // Exit Method
			}

			Console.WriteLine("Voodoo has occurred.");
		}

		public async Task SetVoteListAsync()
		{
			UInt32 id;
			while (!UInt32.TryParse(this.GetUserInput("Visual Novel Id: "), out id)) ;
			Byte score;
			while (!Byte.TryParse(this.GetUserInput("Score (Out of 10): "), out score) || score > 10) ;
			score *= 10;

			Console.WriteLine();
			Console.WriteLine("Set VoteList Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine($"Lib Usage: await this._client.SetVoteListAsync({id}, {score});");
			// Api = Vndb Tcp Api
			Console.WriteLine($"Api Usage: set votelist {id} {{\"vote\": {score}}}");
			Console.WriteLine();

			if (!await this._client.SetVoteListAsync(id, score))
			{
				this.HandleError(this._client.GetLastError());
				return; // Exit Method
			}

			Console.WriteLine("Score Updated!");
			Console.WriteLine();
		}

		public async Task SetVisualNovelListAsync()
		{
			UInt32 id;
			while (!UInt32.TryParse(this.GetUserInput("Visual Novel Id: "), out id)) ;
			Console.WriteLine($"0 - Unknown, 1 - Playing, 2 - Finished, 3 - Stalled, 4 - Dropped");
			Byte status;
			while (!Byte.TryParse(this.GetUserInput("Status (0-4): "), out status) || status > 4) ;
			var notes = this.GetUserInput("Notes: ");

			Console.WriteLine();
			Console.WriteLine("Set Visual Novel List Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine($"Lib Usage: await this._client.SetVisualNovelListAsync({id}, (Status) {status}, {notes.Quote()});");
			// Api = Vndb Tcp Api
			Console.WriteLine($"Api Usage: set vnlist {id} {{\"status\": {status}, \"notes\": {notes.Quote()}}}");
			Console.WriteLine();

			if (!await this._client.SetVisualNovelListAsync(id, (Status) status, notes))
			{
				this.HandleError(this._client.GetLastError());
				return; // Exit Method
			}

			Console.WriteLine("Visual Novel Added / Updated");
			Console.WriteLine();
		}

		public async Task SetWishlistAsync()
		{
			UInt32 id;
			while (!UInt32.TryParse(this.GetUserInput("Visual Novel Id: "), out id)) ;
			Console.WriteLine($"0 - High, 1 - Medium, 2 - Low, 3 - Blacklist");
			Byte priority;
			while (!Byte.TryParse(this.GetUserInput("Priority (0-3): "), out priority) || priority > 3) ;

			Console.WriteLine();
			Console.WriteLine("Set Wishlist Command");
			// Lib = This Library (VndbSharp)
			Console.WriteLine($"Lib Usage: await this._client.SetWishlistAsync({id}, (Priority) {priority});");
			// Api = Vndb Tcp Api
			Console.WriteLine($"Api Usage: set vnlist {id} {{\"priority\": {priority}}}");
			Console.WriteLine();

			if (!await this._client.SetWishlistAsync(id, (Priority) priority))
			{
				this.HandleError(this._client.GetLastError());
				return; // Exit Method
			}

			Console.WriteLine("Visual Novel Added to Wishlist");
			Console.WriteLine();
		}

		private String GetUserInput(String prefix)
		{
			var input = String.Empty;
			Console.Write(prefix);
			while (true)
			{
				ConsoleKeyInfo i = Console.ReadKey(true);
				if (i.Key == ConsoleKey.Enter)
				{
					Console.Write($"\r{prefix}");
					break;
				}
				else if (i.Key == ConsoleKey.Backspace)
				{
					if (input.Length > 0)
					{
						input = input.Substring(0, input.Length - 1);
						Console.Write("\b \b");
					}
				}
				else
				{
					input += i.KeyChar;
					Console.Write(i.KeyChar);
				}
			}

			Console.WriteLine();
			return input;
		}

#if UserAuth
		private Tuple<String, SecureString> GetUsernameAndPassword()
		{
			var username = this.GetUserInput("Vndb Username: ");
			var password = new SecureString();
			var prefix = "Vndb Password: ";

			Console.Write(prefix);
			while (true)
			{
				ConsoleKeyInfo i = Console.ReadKey(true);
				if (i.Key == ConsoleKey.Enter)
				{
					Console.Write($"\r{prefix}");
					break;
				}
				else if (i.Key == ConsoleKey.Backspace)
				{
					if (password.Length > 0)
					{
						password.RemoveAt(password.Length - 1);
						Console.Write("\b \b");
					}
				}
				else
				{
					password.AppendChar(i.KeyChar);
					Console.Write("*");
				}
			}

			Console.WriteLine();
			return new Tuple<String, SecureString>(username, password);
		}
#endif

		private void HandleError(IVndbError error)
		{
			if (error is MissingError missing)
			{
				Console.WriteLine($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
			}
			else if (error is BadArgumentError badArg)
			{
				Console.WriteLine($"A BadArgument Error occured, the field \"{badArg.Field}\" is invalid.");
			}
			else if (error is ThrottledError throttled)
			{
				var minSeconds = (throttled.MinimumWait - DateTime.Now).TotalSeconds; // Not sure if this is correct
				var fullSeconds = (throttled.FullWait - DateTime.Now).TotalSeconds; // Not sure if this is correct
				Console.WriteLine(
					$"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
					$"and preferably \"{fullSeconds}\" before issuing commands.");
			}
			else if (error is GetInfoError getInfo)
			{
				Console.WriteLine($"A GetInfo Error occured, the flag \"{getInfo.Flag}\" is not valid on the issued command.");
			}
			else if (error is InvalidFilterError invalidFilter)
			{
				Console.WriteLine(
					$"A InvalidFilter Error occured, the filter combination of \"{invalidFilter.Field}\", " +
					$"\"{invalidFilter.Operator}\", \"{invalidFilter.Value}\" is not a valid combination.");
			}
			else
			{
				Console.WriteLine($"A {error.Type} Error occured.");
			}
			Console.WriteLine($"Message: {error.Message}");
		}

		private void InvalidFlagsCallback(String method, VndbFlags providedFlags, VndbFlags invalidFlags)
		{
			Console.WriteLine($"Attempted to use method \"{method}\" with flags {providedFlags}, but {invalidFlags} are not permitted on that method.");
		}

		private Vndb _client;
	}
}
