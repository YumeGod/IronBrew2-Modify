using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Obfuscator;
using IronBrew2.Obfuscator.Control_Flow;
using IronBrew2.Obfuscator.Encryption;
using IronBrew2.Obfuscator.VM_Generation;

namespace IronBrew2
{
	public static class IB2
	{
		public static Random Random = new Random();
		private static Encoding _fuckingLua = Encoding.GetEncoding(28591);

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		public static string Obfuscate(string fileName, string input, ObfuscationSettings settings)
		{
			try
			{
				string path = "./temp/" + fileName + "/";

				long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

				string OS = Environment.OSVersion.Platform == PlatformID.Unix ? "/usr/bin/" : "";
				
				string testLua = Path.Combine(path, "luac.out");

				if (!File.Exists(input))
					return "ERROR";

				Console.WriteLine("Checking file...");
				
				Process proc = new Process
				       {
					       StartInfo =
					       {
						       FileName  = @"./luac.exe",
						       Arguments = "-o \"" + testLua + "\" \"" + input + "\"",
						       UseShellExecute = false,
						       RedirectStandardError = true,
						       RedirectStandardOutput = true
					       }
				       };
                Console.WriteLine($"{OS}luac");
				string err = "";
				
				proc.OutputDataReceived += (sender, args) => { err += args.Data; };
				proc.ErrorDataReceived += (sender, args) => { err += args.Data; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();
				
				File.Delete(testLua);

				string t0 = Path.Combine(path, "1.lua");
				
				Console.WriteLine("Stripping comments...");

				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName = @"./luajit.exe",
						       Arguments =
							       "../Lua/Minifier/luasrcdiet.lua --noopt-whitespace --noopt-emptylines --noopt-numbers --noopt-locals --noopt-strings --opt-comments \"" +
							       input                                                       +
							       "\" -o \""                                                  + t0 + "\"",
						       UseShellExecute        = false,
						       RedirectStandardError  = true,
						       RedirectStandardOutput = true
					       }
				       };

				proc.OutputDataReceived += (sender, args) => { err += args.Data; };
				proc.ErrorDataReceived  += (sender, args) => { err += args.Data; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();

				string t1 = Path.Combine(path, "2.lua");
				
				Console.WriteLine("Compiling...");

				File.WriteAllText(t1, new ConstantEncryption(settings, File.ReadAllText(t0, _fuckingLua)).EncryptStrings());
				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName  = $"{OS}luac",
						       Arguments = "-o \"" + testLua + "\" \"" + t1 + "\"",
						       UseShellExecute = false,
						       RedirectStandardError = true,
						       RedirectStandardOutput = true
					       }
				       };

				proc.OutputDataReceived += (sender, args) => { err += args.Data; };
				proc.ErrorDataReceived += (sender, args) => { err += args.Data; };

				proc.Start();
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
				proc.WaitForExit();
				
				Console.WriteLine("Obfuscating...");

				Deserializer des    = new Deserializer(File.ReadAllBytes(testLua));
				Chunk lChunk = des.DecodeFile();

				if (settings.ControlFlow)
				{
					CFContext cf = new CFContext(lChunk);
					cf.DoChunks();
				}

				Console.WriteLine("Serializing...");
				
				//shuffle stuff
				//lChunk.Constants.Shuffle();
				//lChunk.Functions.Shuffle();

				ObfuscationContext context = new ObfuscationContext(lChunk);

				string t2 = Path.Combine(path, "3.lua");
				string c = new Generator(context).GenerateVM(settings);

				//string byteLocal = c.Substring(null, "\n");
				//string rest = c.Substring("\n");

				File.WriteAllText(t2, c, _fuckingLua);

				string t3 = Path.Combine(path, "4.lua");
				
				Console.WriteLine("Minifying...");
				
				proc = new Process
				       {
					       StartInfo =
					       {
						       FileName = $"{OS}luajit",
						       Arguments =
							       "./Lua/Minifier/luasrcdiet.lua --maximum --opt-entropy --opt-emptylines --opt-eols --opt-numbers --opt-whitespace --opt-locals --noopt-strings \"" +
							       t2                                                                                                                                                +
							       "\" -o \"" + 
							        t3 + 
							       "\""
								,
					       }
				       };

				proc.Start();
				proc.WaitForExit();
				
				Console.WriteLine("Watermark...");

				long length = new System.IO.FileInfo(t3).Length;
				long finishTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				long timeTaken = finishTime - startTime;

				string output = @"--[[
This header was automatically generated by YumeCloud Obfuscator

Version: 1.0 Release
Time of Submit: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
Time of Obfuscate: " + timeTaken + @"ms
File Size: " + length + @" bytes
Base Offset: 0x00000539
Obfuscation Mapping: V1(0x00000539)
Obfuscation ID: " + RandomString(16) + @"
Compile ID: " + RandomString(16) + @"
Memory Dump: null
Server: YumeCloud_OVH_MAIN_LAX

Thanks for using YumeCloud Obfuscator
]]
" + File.ReadAllText(t3, _fuckingLua).Replace("\n", " ");
				return output;
			}
			catch (Exception e)
			{
				return e.ToString();
			}
		}
	}
}