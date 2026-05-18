using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.App;

namespace TypedActivityResultFacadeApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Exported = true)]
public class MainActivity : AndroidX.Activity.ComponentActivity
{
	const string Tag = "TypedFacadeRuntime";

	DispatchingActivityResultRegistry? registry;
	FakeActivityResultCaller? caller;
	readonly List<RegisteredLauncher> launchers = [];
	bool didRun;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		registry = new DispatchingActivityResultRegistry();
		caller = new FakeActivityResultCaller(registry);

		RegisterPublicApiCases();

		SetContentView(new TextView(this) { Text = "Typed Activity Result facade API coverage app" });
	}

	protected override void OnPostResume()
	{
		base.OnPostResume();

		if (didRun)
			return;

		didRun = true;
		RunRegisteredLaunchers();
		Log.Info(Tag, $"Public API checks passed ({launchers.Count} launchers)");
	}

	void RegisterPublicApiCases()
	{
#pragma warning disable CS0618
#pragma warning disable CS0612
		var captureVideo = ActivityResultContracts.Typed.CaptureVideo();
		var createDocumentLegacy = ActivityResultContracts.Typed.CreateDocument();
		var createDocument = ActivityResultContracts.Typed.CreateDocument("text/plain");
		var getContent = ActivityResultContracts.Typed.GetContent();
		var getMultipleContents = ActivityResultContracts.Typed.GetMultipleContents();
		var openDocument = ActivityResultContracts.Typed.OpenDocument();
		var openDocumentTree = ActivityResultContracts.Typed.OpenDocumentTree();
		var openMultipleDocuments = ActivityResultContracts.Typed.OpenMultipleDocuments();
		var pickContact = ActivityResultContracts.Typed.PickContact();
		var pickMultipleVisualMedia = ActivityResultContracts.Typed.PickMultipleVisualMedia();
		var pickMultipleVisualMediaLimited = ActivityResultContracts.Typed.PickMultipleVisualMedia(3);
		var pickVisualMedia = ActivityResultContracts.Typed.PickVisualMedia();
		var requestMultiplePermissions = ActivityResultContracts.Typed.RequestMultiplePermissions();
		var requestPermission = ActivityResultContracts.Typed.RequestPermission();
		var startActivityForResult = ActivityResultContracts.Typed.StartActivityForResult();
		var startIntentSenderForResult = ActivityResultContracts.Typed.StartIntentSenderForResult();
		var takePicture = ActivityResultContracts.Typed.TakePicture();
		var takePicturePreview = ActivityResultContracts.Typed.TakePicturePreview();
		var takeVideo = ActivityResultContracts.Typed.TakeVideo();
#pragma warning restore CS0612
#pragma warning restore CS0618

		Require(caller is not null && registry is not null, "Test harness was not initialized.");
		var testCaller = caller!;
		var testRegistry = registry!;

		AddCallerCase(
			"request-permission",
			testCaller.RegisterForActivityResult(requestPermission, result => Require(result == true, "RequestPermission should return bool true.")),
			"android.permission.CAMERA");

		AddCallerCase(
			"request-multiple-permissions",
			testCaller.RegisterForActivityResult(requestMultiplePermissions, result =>
			{
				Require(result is not null, "RequestMultiplePermissions should produce a dictionary.");
				var permissions = result!;
				Require(permissions["camera"], "RequestMultiplePermissions should preserve true values.");
				Require(permissions.ContainsKey("microphone"), "RequestMultiplePermissions should wrap the source map.");
				Require(!permissions["microphone"], "RequestMultiplePermissions should preserve false values.");
			}),
			new[] { "camera", "microphone" });

		AddCallerCase(
			"capture-video",
			testCaller.RegisterForActivityResult(captureVideo, result => Require(result == true, "CaptureVideo should return bool true.")),
			Uri("content://typed/capture-video"));

		AddCallerCase(
			"take-picture",
			testCaller.RegisterForActivityResult(takePicture, result => Require(result == true, "TakePicture should return bool true.")),
			Uri("content://typed/take-picture"));

		AddCallerCase(
			"take-picture-preview",
			testCaller.RegisterForActivityResult(takePicturePreview, result => Require(result is not null, "TakePicturePreview should return a bitmap.")),
			null!);

		AddCallerCase(
			"take-video",
			testCaller.RegisterForActivityResult(takeVideo, result => Require(result is not null, "TakeVideo should return a bitmap.")),
			Uri("content://typed/take-video"));

		AddCallerWithRegistryCase(
			"create-document-legacy",
			testCaller.RegisterForActivityResult(createDocumentLegacy, testRegistry, result => RequireUri(result, "content://typed/legacy.txt")),
			"legacy.txt");

		AddCallerWithRegistryCase(
			"create-document",
			testCaller.RegisterForActivityResult(createDocument, testRegistry, result => RequireUri(result, "content://typed/typed.txt")),
			"typed.txt");

		AddCallerWithRegistryCase(
			"get-content",
			testCaller.RegisterForActivityResult(getContent, testRegistry, result => RequireUri(result, "content://typed/get-content")),
			"image/*");

		AddCallerWithRegistryCase(
			"open-document",
			testCaller.RegisterForActivityResult(openDocument, testRegistry, result => RequireUri(result, "content://typed/open-document")),
			new[] { "image/*", "video/*" });

		AddCallerWithRegistryCase(
			"open-document-tree",
			testCaller.RegisterForActivityResult(openDocumentTree, testRegistry, result => RequireUri(result, "content://typed/open-document-tree")),
			null!);

		AddRegistryCase(
			"pick-contact",
			testRegistry.Register("pick-contact", pickContact, result => RequireUri(result, "content://typed/pick-contact")),
			null!);

		AddRegistryCase(
			"start-activity-for-result",
			testRegistry.Register("start-activity", startActivityForResult, result =>
			{
				Require(result is not null, "StartActivityForResult should return ActivityResult.");
				var activityResult = result!;
				Require(activityResult.ResultCode == (int)Result.Ok, "StartActivityForResult should preserve the result code.");
			}),
			new Intent(Intent.ActionView));

		AddRegistryCase(
			"start-intent-sender-for-result",
			testRegistry.Register("start-intent-sender", startIntentSenderForResult, result =>
			{
				Require(result is not null, "StartIntentSenderForResult should return ActivityResult.");
				var activityResult = result!;
				Require(activityResult.ResultCode == (int)Result.Ok, "StartIntentSenderForResult should preserve the result code.");
			}),
			CreateIntentSenderRequest());

		AddRegistryCase(
			"pick-visual-media",
			testRegistry.Register("pick-visual-media", pickVisualMedia, result => RequireUri(result, "content://typed/pick-visual-media")),
			CreatePickVisualMediaRequest());

		AddRegistryWithLifecycleCase(
			"get-multiple-contents",
			testRegistry.Register("get-multiple-contents", this, getMultipleContents, result =>
			{
				Require(result is not null, "GetMultipleContents should return a list.");
				var uris = result!;
				Require(uris.Count == 2, "GetMultipleContents should observe list mutations after dispatch.");
				RequireUri(uris[0], "content://typed/get-multiple-contents/1");
				RequireUri(uris[1], "content://typed/get-multiple-contents/2");
			}),
			"image/*");

		AddRegistryWithLifecycleCase(
			"open-multiple-documents",
			testRegistry.Register("open-multiple-documents", this, openMultipleDocuments, result =>
			{
				Require(result is not null, "OpenMultipleDocuments should return a list.");
				var uris = result!;
				Require(uris.Count == 2, "OpenMultipleDocuments should observe list mutations after dispatch.");
			}),
			new[] { "image/*", "video/*" });

		AddRegistryWithLifecycleCase(
			"pick-multiple-visual-media",
			testRegistry.Register("pick-multiple-visual-media", this, pickMultipleVisualMedia, result =>
			{
				Require(result is not null, "PickMultipleVisualMedia should return a list.");
				var uris = result!;
				Require(uris.Count == 2, "PickMultipleVisualMedia should observe list mutations after dispatch.");
			}),
			CreatePickVisualMediaRequest());

		AddRegistryWithLifecycleCase(
			"pick-multiple-visual-media-limited",
			testRegistry.Register("pick-multiple-visual-media-limited", this, pickMultipleVisualMediaLimited, result =>
			{
				Require(result is not null, "PickMultipleVisualMedia(maxItems) should return a list.");
				var uris = result!;
				Require(uris.Count == 2, "PickMultipleVisualMedia(maxItems) should observe list mutations after dispatch.");
			}),
			CreatePickVisualMediaRequest());

		AddKtxCase(
			"ktx-request-permission",
			testCaller.RegisterForActivityResult(
				requestPermission,
				"android.permission.CAMERA",
				result => Require(result == true, "KTX RequestPermission should return bool true.")),
			options: null);

		AddKtxCase(
			"ktx-get-content",
			testCaller.RegisterForActivityResult(
				getContent,
				testRegistry,
				"image/*",
				result => RequireUri(result, "content://typed/get-content")),
			ActivityOptionsCompat.MakeBasic());
	}

	void RunRegisteredLaunchers()
	{
		foreach (var launcher in launchers)
		{
			launcher.Launch();
			launcher.Unregister();
		}
	}

	void AddCallerCase<TInput>(string name, ActivityResultLauncher<TInput> launcher, TInput input)
		=> launchers.Add(new RegisteredLauncher(name, () => launcher.Launch(input), launcher.Unregister));

	void AddCallerWithRegistryCase<TInput>(string name, ActivityResultLauncher<TInput> launcher, TInput input)
		=> launchers.Add(new RegisteredLauncher(name, () => launcher.Launch(input), launcher.Unregister));

	void AddRegistryCase<TInput>(string name, ActivityResultLauncher<TInput> launcher, TInput input)
		=> launchers.Add(new RegisteredLauncher(name, () => launcher.Launch(input), launcher.Unregister));

	void AddRegistryWithLifecycleCase<TInput>(string name, ActivityResultLauncher<TInput> launcher, TInput input)
		=> launchers.Add(new RegisteredLauncher(name, () => launcher.Launch(input), launcher.Unregister));

	void AddKtxCase<TInput>(string name, ActivityResultCallerLauncher<TInput> launcher, ActivityOptionsCompat? options)
		=> launchers.Add(new RegisteredLauncher(name, () =>
		{
			if (options is null)
				launcher.Launch();
			else
				launcher.Launch(options);
		}, launcher.Unregister));

	static PickVisualMediaRequest CreatePickVisualMediaRequest()
		=> new PickVisualMediaRequest.Builder().SetMaxItems(2).Build();

	IntentSenderRequest CreateIntentSenderRequest()
	{
		var pendingIntent = PendingIntent.GetActivity(
			this,
			0,
			new Intent(this, typeof(MainActivity)),
			PendingIntentFlags.Immutable)!;

		return new IntentSenderRequest.Builder(pendingIntent).Build();
	}

	internal static Android.Net.Uri Uri(string value)
		=> Android.Net.Uri.Parse(value)!;

	internal static void RequireUri(Android.Net.Uri? actual, string expected)
	{
		Require(actual is not null, $"Expected URI '{expected}' but got null.");
		Require(actual!.ToString() == expected, $"Expected URI '{expected}' but got '{actual}'.");
	}

	internal static void Require(bool condition, string message)
	{
		if (!condition)
			throw new InvalidOperationException(message);
	}

	readonly record struct RegisteredLauncher(string Name, Action Launch, Action Unregister);
}

sealed class FakeActivityResultCaller : Java.Lang.Object, IActivityResultCaller
{
	int counter;
	readonly DispatchingActivityResultRegistry defaultRegistry;

	public FakeActivityResultCaller(DispatchingActivityResultRegistry sharedRegistry)
	{
		defaultRegistry = sharedRegistry;
	}

	public ActivityResultLauncher RegisterForActivityResult(
		AndroidX.Activity.Result.Contract.ActivityResultContract contract,
		IActivityResultCallback callback)
		=> defaultRegistry.Register($"caller-{++counter}", contract, callback);

	public ActivityResultLauncher RegisterForActivityResult(
		AndroidX.Activity.Result.Contract.ActivityResultContract contract,
		ActivityResultRegistry registry,
		IActivityResultCallback callback)
		=> registry.Register($"caller-registry-{++counter}", contract, callback);
}

sealed class DispatchingActivityResultRegistry : ActivityResultRegistry
{
	public override void OnLaunch(
		int requestCode,
		AndroidX.Activity.Result.Contract.ActivityResultContract contract,
		Java.Lang.Object? input,
		ActivityOptionsCompat? options)
	{
		var result = CreateResult(contract, input);
		var dispatched = DispatchResult(requestCode, result);
		MainActivity.Require(dispatched, $"DispatchResult returned false for '{contract.GetType().FullName}'.");
	}

	static Java.Lang.Object CreateResult(
		AndroidX.Activity.Result.Contract.ActivityResultContract contract,
		Java.Lang.Object? input)
	{
		switch (contract)
		{
			case ActivityResultContracts.RequestPermission:
			case ActivityResultContracts.CaptureVideo:
			case ActivityResultContracts.TakePicture:
#pragma warning disable CA1422
				return new Java.Lang.Boolean(true);
#pragma warning restore CA1422

			case ActivityResultContracts.RequestMultiplePermissions:
				var permissions = new Android.Runtime.JavaDictionary<string, bool> {
					["camera"] = true,
				};
				permissions["microphone"] = false;
				return permissions;

			case ActivityResultContracts.GetMultipleContents:
				return CreateUriList("content://typed/get-multiple-contents");

			case ActivityResultContracts.OpenMultipleDocuments:
				return CreateUriList("content://typed/open-multiple-documents");

			case ActivityResultContracts.PickMultipleVisualMedia:
				return CreateUriList("content://typed/pick-multiple-visual-media");

			case ActivityResultContracts.CreateDocument:
				var documentName = input?.ToString() ?? "document";
				return MainActivity.Uri($"content://typed/{documentName}");

			case ActivityResultContracts.GetContent:
				return MainActivity.Uri("content://typed/get-content");

			case ActivityResultContracts.OpenDocument:
				return MainActivity.Uri("content://typed/open-document");

			case ActivityResultContracts.OpenDocumentTree:
				return MainActivity.Uri("content://typed/open-document-tree");

			case ActivityResultContracts.PickContact:
				return MainActivity.Uri("content://typed/pick-contact");

			case ActivityResultContracts.PickVisualMedia:
				return MainActivity.Uri("content://typed/pick-visual-media");

			case ActivityResultContracts.StartActivityForResult:
			case ActivityResultContracts.StartIntentSenderForResult:
				return new ActivityResult((int)Result.Ok, new Intent("typed-action"));

			case ActivityResultContracts.TakePicturePreview:
			case ActivityResultContracts.TakeVideo:
				return Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888!)!;

			default:
				throw new InvalidOperationException($"Unhandled contract type '{contract.GetType().FullName}'.");
		}
	}

	static Android.Runtime.JavaList<Android.Net.Uri> CreateUriList(string baseUri)
	{
		var uris = new Android.Runtime.JavaList<Android.Net.Uri> {
			MainActivity.Uri($"{baseUri}/1"),
		};

		uris.Add(MainActivity.Uri($"{baseUri}/2"));
		return uris;
	}
}
