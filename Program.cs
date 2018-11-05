using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using SpotifyAPI.Web.Auth;
using System.Configuration;
using static RedditSharp.AuthProvider;

namespace redditbot
{
	class Program
	{
		private static SpotifyWebAPI _spotify;
		private static string _clientId = "";
		private static string _secretId = "";

		static void Main(string[] args)
		{
		
			var reddit = new Reddit();
			_clientId = ConfigurationManager.AppSettings["APIKey"];
			_secretId = ConfigurationManager.AppSettings["ClientSecret"];

			AuthorizationCodeAuth auth =
				new AuthorizationCodeAuth(_clientId, _secretId, "http://localhost:4002", "http://localhost:4002",
					SpotifyAPI.Web.Enums.Scope.PlaylistReadPrivate | SpotifyAPI.Web.Enums.Scope.PlaylistReadCollaborative);
			auth.AuthReceived += AuthOnAuthReceived;
			auth.Start();
			auth.OpenBrowser();

			List<string> freshSongs = new List<string>();
			var subreddit = reddit.GetSubreddit("/r/hiphopheads");

		

			foreach (var post in subreddit.GetTop((FromTime)Enum.Parse(typeof(FromTime), "Month")).Take(100))
			{
				string title = post.Title;
				string tag = title.Substring(0, 7);
				if (tag.ToLower() == "[fresh]")
				{
					freshSongs.Add(title.ToString());
				} 
				Console.WriteLine(post.Title);
			}
			string song = freshSongs.ElementAt(0).Substring(8);
			SearchItem item = _spotify.SearchItems(song, SpotifyAPI.Web.Enums.SearchType.Track, 1, 0, "na");

		/*
			for(int i = 0; i < freshSongs.Count(); i++)
			{
				 
			}
			_spotify.SearchItems()
		*/
			Console.WriteLine("");
		}

		private static async void AuthOnAuthReceived(object sender, AuthorizationCode payload)
		{
			AuthorizationCodeAuth auth = (AuthorizationCodeAuth)sender;
			auth.Stop();

			Token token = await auth.ExchangeCode(payload.Code);
			_spotify = new SpotifyWebAPI
			{
				AccessToken = token.AccessToken,
				TokenType = token.TokenType
			};
			SearchSongs(_spotify);
		}

		private static async void SearchSongs(SpotifyWebAPI api)
		{
			api = _spotify;
			PrivateProfile profile = await api.GetPrivateProfileAsync();
			string name = string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName;
			Console.WriteLine("Hello there " + name);
			Console.WriteLine("Your playlists:");
			Paging<SimplePlaylist> playlists = await api.GetUserPlaylistsAsync(profile.Id);
			do
			{
				playlists.Items.ForEach(playlist =>
				{
					Console.WriteLine(playlist.Name);
				});
				playlists = await api.GetNextPageAsync(playlists);
			} while (playlists.HasNextPage());
		//	SearchItem item = await api.SearchItemsAsync("Anderson .Paak - Tints", SpotifyAPI.Web.Enums.SearchType.Track, 1, 0, "na");

		}


	}
}
