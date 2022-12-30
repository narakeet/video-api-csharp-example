using System;
using System.IO;
namespace NarakeetExample
{
	public class Program
	{    

		public static string mainSourceFile = "source.txt";
		public static string videoDirectory = "video";

		static async Task Main()
		{
			String apiKey =  System.Environment.GetEnvironmentVariable("NARAKEET_API_KEY");
			if (string.IsNullOrEmpty(apiKey)) {
				throw new ArgumentException("provide an API key as an environment variable NARAKEET_API_KEY");
			}
			VideoAPI api = new VideoAPI(apiKey);

			// upload the files to Narakeet 
			String videoZip = api.ZipDirectoryIntoTempFile(videoDirectory);
			UploadToken uploadToken = await api.RequestUploadTokenAsync();
			await api.UploadZipFileAsync(uploadToken, videoZip);
			File.Delete(videoZip);

			// request a build task and wait until it completes
			BuildTask buildTask = await api.RequestBuildTaskAsync(uploadToken, mainSourceFile);
			var taskResult = await api.PollUntilFinishedAsync(
					buildTask, 
					// do something smarter here with progress
					progress => Console.WriteLine($"{progress.Message} ({progress.Percent}%)")
					);

			// save to local file
			if (taskResult.Succeeded)
			{
				var tempFileName = await api.DownloadToTempFileAsync(taskResult.Result);
				Console.WriteLine($"Downloaded to {tempFileName}");
			}
			else
			{
				throw new Exception(taskResult.Message);
			}
		}
	}
}
