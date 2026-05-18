using System.Text;
using System.Xml;
using CliWrap;
using CliWrap.Buffered;
using NUnit.Framework;

namespace AllPackagesTests;

[TestFixture]
public class TypedActivityResultFacadeAppTests
{
	static string base_dir = "";
	static string test_dir = Path.Combine ("output", "tests", "allpackages");
	static string configuration = "Release";
	static string platform_version = "29";
	static string net_version = "net9.0";
	static string local_package_version = "1.13.0";

	static TypedActivityResultFacadeAppTests ()
	{
		while (!File.Exists (Path.Combine (base_dir, "config.json")))
			base_dir = Path.Combine ("..", base_dir);

		base_dir = Path.GetFullPath (base_dir);

		Directory.CreateDirectory (Path.Combine (base_dir, test_dir));

		var directory_props = Path.Combine (base_dir, test_dir, "Directory.Build.props");
		var props_content = """
		<Project>
		  <PropertyGroup>
		    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
		  </PropertyGroup>
		</Project>
		""";

		if (!File.Exists (directory_props))
			File.WriteAllText (directory_props, props_content);

		var nuget_config_src = Path.Combine (base_dir, "tests", "common", "NuGet.config");
		var nuget_config_dst = Path.Combine (base_dir, test_dir, "NuGet.config");

		if (!File.Exists (nuget_config_dst)) {
			var contents = File.ReadAllText (nuget_config_src);
			contents = contents.Replace ("../output", "../..");
			File.WriteAllText (nuget_config_dst, contents);
		}
	}

	[Test]
	[Category ("Android")]
	public async Task TestTypedActivityResultFacadeApp ()
	{
		var case_dir = Path.Combine (base_dir, test_dir, "android", "TypedActivityResultFacadeApp");

		if (Directory.Exists (case_dir))
			Directory.Delete (case_dir, true);

		Directory.CreateDirectory (case_dir);

		await RunAndAssertSuccess ("new android", case_dir);

		var proj_file = Directory.GetFiles (case_dir, "*.csproj").FirstOrDefault ();

		if (proj_file is null) {
			Assert.Fail ("Could not find the project file.");
			return;
		}

		RewriteProjectFile (proj_file);
		WriteMainActivity (Path.Combine (case_dir, "MainActivity.cs"));
		await PackActivityPackage ();
		await RunAndAssertSuccess ($"add package Xamarin.AndroidX.Activity --version {local_package_version} --no-restore", case_dir);

		await RunAndAssertSuccess ($"build -c {configuration} -bl", case_dir, true);

		try {
			Directory.Delete (case_dir, true);
		} catch {
			// Ignore
		}
	}

	static void RewriteProjectFile (string filename)
	{
		ReplaceInFile (filename, "<TargetFramework>net10.0-android</TargetFramework>", $"<TargetFramework>{net_version}-android</TargetFramework>");
		ReplaceInFile (filename, "<SupportedOSPlatformVersion>24</SupportedOSPlatformVersion>", $"<SupportedOSPlatformVersion>{platform_version}</SupportedOSPlatformVersion>");
		ReplaceInFile (filename, "<TrimMode>full</TrimMode>", "<TrimMode>partial</TrimMode>");

		var xml = new XmlDocument ();
		xml.Load (filename);
		var root = xml.DocumentElement!;

		var property_group = root ["PropertyGroup"]!;

		var no_warn = xml.CreateElement ("NoWarn");
		no_warn.InnerText = "CS0612;CS0618";
		property_group.AppendChild (no_warn);

		var heap_size = xml.CreateElement ("JavaMaximumHeapSize");
		heap_size.InnerText = "4G";
		property_group.AppendChild (heap_size);

		xml.Save (filename);
	}

	static void WriteMainActivity (string filename)
	{
		var template = Path.Combine (base_dir, "tests", "allpackages", "TypedActivityResultFacadeMainActivityTemplate.cs");
		File.WriteAllText (filename, File.ReadAllText (template));
	}

	static Task PackActivityPackage ()
	{
		var project = Path.Combine (base_dir, "generated", "androidx.activity.activity", "androidx.activity.activity.csproj");
		var output = Path.Combine (base_dir, "output");
		return RunAndAssertSuccess (
			$"pack \"{project}\" -c {configuration} -o \"{output}\" -noAutoResponse",
			base_dir,
			true);
	}

	static void ReplaceInFile (string filename, string oldValue, string newValue)
	{
		var contents = File.ReadAllText (filename);
		contents = contents.Replace (oldValue, newValue);
		File.WriteAllText (filename, contents);
	}

	static async Task RunAndAssertSuccess (string arguments, string workingDir, bool isMSBuild = false)
	{
		var result = await Cli.Wrap ("dotnet")
			.WithArguments (arguments)
			.WithWorkingDirectory (workingDir)
			.WithValidation (CommandResultValidation.None)
			.ExecuteBufferedAsync ();

		if (result.ExitCode == 0)
			return;

		var sb = new StringBuilder ();

		sb.AppendLine ($"Command '{arguments}' failed with exit code {result.ExitCode}.");

		if (!isMSBuild) {
			sb.AppendLine ("Output:");
			sb.AppendLine (result.StandardOutput);
			sb.AppendLine ();
			sb.AppendLine ("Error:");
			sb.AppendLine (result.StandardError);

			Assert.Fail (sb.ToString ());
		}

		var errors = new List<string> ();
		var warnings = new List<string> ();

		using (var sr = new StringReader (result.StandardOutput)) {
			string? line;

			while ((line = sr.ReadLine ()) != null) {
				if (line == "Build succeeded." || line == "Build FAILED.")
					break;

				if (CanonicalError.Parse (line) is CanonicalError.Parts parts) {
					var message = $"{parts.code}: {parts.text}";

					if (parts.category == CanonicalError.Parts.Category.Warning)
						warnings.Add (message);
					else
						errors.Add (message);
				}
			}
		}

		if (errors.Count > 0) {
			sb.AppendLine ("Errors:");
			errors.ForEach (e => sb.AppendLine (e));
		}

		if (warnings.Count > 0) {
			sb.AppendLine ("Warnings:");
			warnings.ForEach (w => sb.AppendLine (w));
		}

		Assert.Fail (sb.ToString ());
	}
}
