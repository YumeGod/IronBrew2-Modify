using System;
using System.IO;
using System.Linq;
using System.Text;
using IronBrew2;
using IronBrew2.Obfuscator;

namespace IronBrew2_CLI
{
	class Program
	{
		public static Random Random = new Random();

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		static void Main(string[] args)
        {
            try
            {
				string fileName = RandomString(6);
				Directory.CreateDirectory("./temp/" + fileName);

				string result = IB2.Obfuscate(fileName, args[0], new ObfuscationSettings());

				string output_path = "./output/obfuscated_" + args[0];


				if (File.Exists(output_path))
                {
					File.Delete(output_path);
                }

				File.WriteAllText(output_path, result);

				Directory.Delete("./temp/" + fileName, true);

				Console.WriteLine("Done!");
			}catch(Exception e)
            {
				Console.WriteLine(e.Message);
            }
		}
	}
}