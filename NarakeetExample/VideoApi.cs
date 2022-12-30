using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System;
using System.Net.Http;
using System.IO.Compression;
using Newtonsoft.Json;
using System.IO;

namespace NarakeetExample 
{
	public class UploadToken
	{
		public string? Url { get; set; }
		public string? ContentType { get; set; }
		public string? RepositoryType { get; set; }
		public string? Repository { get; set; }
	}
	public class BuildTask
	{
		public string? StatusUrl {get; set;}
		public string? TaskId {get; set;}
	}
	public class BuildTaskStatus
	{
		public string? Message {get; set;}
		public int? Percent {get; set;}
		public bool Succeeded {get; set;}
		public bool Finished {get; set;}
		public string? Result {get; set;}
	}

	public class VideoAPI
	{    
		public string ApiKey { get; set; }
		public string ApiUrl { get; set; }
		public int PollingIntervalSeconds { get; set; }

		public VideoAPI(string apiKey, string apiUrl = "https://api.narakeet.com", int pollingIntervalSeconds = 5)
		{
			ApiKey = apiKey;
			ApiUrl = apiUrl;
			PollingIntervalSeconds = pollingIntervalSeconds;
		}
		public async Task<UploadToken> RequestUploadTokenAsync()
		{
			string url = ApiUrl + "/video/upload-request/zip";
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
				var response = await client.GetAsync(url);
				var json = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<UploadToken>(json);
			}
		}
		public string ZipDirectoryIntoTempFile(string directory)
		{
			string tempPath = Path.GetTempPath();
			string tempFileName = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".zip");
			ZipFile.CreateFromDirectory(directory, tempFileName);
			return tempFileName;
		}
		public async Task UploadZipFileAsync(UploadToken uploadToken, string zipArchive)
		{
			using (var client = new HttpClient())
			{
				using (var content = new StreamContent(File.OpenRead(zipArchive)))
				{
					content.Headers.Add("Content-Type", uploadToken.ContentType);
					var response = await client.PutAsync(uploadToken.Url, content);
					response.EnsureSuccessStatusCode();
				}
			}
		}
		public async Task<BuildTask> RequestBuildTaskAsync(UploadToken uploadToken, string sourceFileInZip)
		{
			var request = new
			{
				repositoryType = uploadToken.RepositoryType,
				repository = uploadToken.Repository,
				source = sourceFileInZip
			};

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

				var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
				var response = await client.PostAsync($"{ApiUrl}/video/build", content);
				response.EnsureSuccessStatusCode();

				var responseContent = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<BuildTask>(responseContent);
			}
		}
		public async Task<BuildTaskStatus> PollUntilFinishedAsync(BuildTask buildTask, Action<BuildTaskStatus> progressCallback = null)
		{
			while (true)
			{
				using (var client = new HttpClient())
				{
					var response = await client.GetAsync(buildTask.StatusUrl);
					response.EnsureSuccessStatusCode();
					var responseContent = await response.Content.ReadAsStringAsync();
					var buildTaskStatus = JsonConvert.DeserializeObject<BuildTaskStatus>(responseContent);
					if (buildTaskStatus.Finished)
					{
						return buildTaskStatus;
					}
					progressCallback?.Invoke(buildTaskStatus);
					await Task.Delay(PollingIntervalSeconds * 1000);
				}
			}

		}
		public async Task<string> DownloadToTempFileAsync(string url)
		{
			using (var client = new HttpClient())
			{
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();

				string tempPath = Path.GetTempPath();
				string tempFileName = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".mp4");

				using (var fileStream = File.Create(tempFileName))
				{
					await response.Content.CopyToAsync(fileStream);
				}

				return tempFileName;
			}
		}

	}
}

