#nullable enable

using System;
using Android.Content;
using AndroidX.Core.App;
using Java.Interop;

namespace AndroidX.Activity.Result
{

public readonly struct ActivityResultUnit
{
    public static ActivityResultUnit Value => default;
}

internal sealed class ActivityResultCallbackAdapter<TResult> : Java.Lang.Object, IActivityResultCallback
{
    readonly Action<TResult?> callback;
    readonly Func<Java.Lang.Object?, TResult?> outputConverter;

    public ActivityResultCallbackAdapter(Action<TResult?> callback, Func<Java.Lang.Object?, TResult?> outputConverter)
    {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        this.outputConverter = outputConverter ?? throw new ArgumentNullException(nameof(outputConverter));
    }

    public void OnActivityResult(Java.Lang.Object? result)
        => callback(outputConverter(result));
}

public sealed class ActivityResultLauncher<TInput>
{
    internal AndroidX.Activity.Result.ActivityResultLauncher Native { get; }

    internal ActivityResultLauncher(AndroidX.Activity.Result.ActivityResultLauncher native)
        => Native = native ?? throw new ArgumentNullException(nameof(native));

    public void Unregister()
        => Native.Unregister();
}

public static class ActivityResultLauncherTypedExtensions
{
    public static void Launch<TInput>(
        this ActivityResultLauncher<TInput> launcher,
        TInput input,
        ActivityOptionsCompat? options = null)
        where TInput : Java.Lang.Object
        => launcher.Native.Launch(input, options);

    public static void Launch(
        this ActivityResultLauncher<string> launcher,
        string input,
        ActivityOptionsCompat? options = null)
        => launcher.Native.Launch(input is null ? null : new Java.Lang.String(input), options);

    public static void Launch(
        this ActivityResultLauncher<string[]> launcher,
        string[] input,
        ActivityOptionsCompat? options = null)
        => launcher.Native.Launch(input is null ? null : new JavaArray<string>(input), options);

    public static void Launch(
        this ActivityResultLauncher<ActivityResultUnit> launcher,
        ActivityOptionsCompat? options = null)
        => launcher.Native.Launch(null, options);
}

}

namespace AndroidX.Activity.Result.Contract
{

public sealed class ActivityResultContract<TInput, TResult>
{
    readonly AndroidX.Activity.Result.Contract.ActivityResultContract native;
    readonly Func<TInput?, Java.Lang.Object?> inputConverter;
    readonly Func<Java.Lang.Object?, TResult?> outputConverter;

    public ActivityResultContract(AndroidX.Activity.Result.Contract.ActivityResultContract native)
        : this(native, DefaultInputConverter, DefaultOutputConverter)
    {
    }

    internal ActivityResultContract(
        AndroidX.Activity.Result.Contract.ActivityResultContract native,
        Func<TInput?, Java.Lang.Object?> inputConverter,
        Func<Java.Lang.Object?, TResult?> outputConverter)
    {
        this.native = native ?? throw new ArgumentNullException(nameof(native));
        this.inputConverter = inputConverter ?? throw new ArgumentNullException(nameof(inputConverter));
        this.outputConverter = outputConverter ?? throw new ArgumentNullException(nameof(outputConverter));
    }

    public Intent CreateIntent(Context context, TInput? input)
        => native.CreateIntent(context, inputConverter(input));

    public TResult? ParseResult(int resultCode, Intent? intent)
        => outputConverter(native.ParseResult(resultCode, intent));

    public AndroidX.Activity.Result.Contract.ActivityResultContract.SynchronousResult? GetSynchronousResult(Context context, TInput? input)
        => native.GetSynchronousResult(context, inputConverter(input));

    public AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult(AndroidX.Activity.Result.IActivityResultCaller caller, Action<TResult?> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, outputConverter);
        var launcher = caller.RegisterForActivityResult(native, callbackAdapter);
        return new AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher);
    }

    public AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult(AndroidX.Activity.Result.IActivityResultCaller caller, AndroidX.Activity.Result.ActivityResultRegistry registry, Action<TResult?> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, outputConverter);
        var launcher = caller.RegisterForActivityResult(native, registry, callbackAdapter);
        return new AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher);
    }

    public AndroidX.Activity.Result.ActivityResultLauncher<TInput> Register(AndroidX.Activity.Result.ActivityResultRegistry registry, string key, Action<TResult?> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, outputConverter);
        var launcher = registry.Register(key, native, callbackAdapter);
        return new AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher);
    }

    public AndroidX.Activity.Result.ActivityResultLauncher<TInput> Register(AndroidX.Activity.Result.ActivityResultRegistry registry, string key, AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner, Action<TResult?> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (lifecycleOwner is null)
            throw new ArgumentNullException(nameof(lifecycleOwner));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, outputConverter);
        var launcher = registry.Register(key, lifecycleOwner, native, callbackAdapter);
        return new AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher);
    }

    static Java.Lang.Object? DefaultInputConverter(TInput? input)
        => input switch
        {
            null => null,
            Java.Lang.Object javaObject => javaObject,
            _ => throw new InvalidCastException($"Cannot marshal '{typeof(TInput).FullName}' to Java.Lang.Object."),
        };

    static TResult? DefaultOutputConverter(Java.Lang.Object? output)
        => output switch
        {
            null => default,
            TResult typed => typed,
            _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(TResult).FullName}'."),
        };
}

public partial class ActivityResultContracts
{
    public static class Typed
    {
        public static ActivityResultContract<Android.Net.Uri, bool> CaptureVideo()
            => new(new ActivityResultContracts.CaptureVideo(), UriInputConverter, BooleanOutputConverter);

        public static ActivityResultContract<string, Android.Net.Uri> CreateDocument()
            => new(new ActivityResultContracts.CreateDocument(), StringInputConverter, UriOutputConverter);

        public static ActivityResultContract<string, Android.Net.Uri> GetContent()
            => new(new ActivityResultContracts.GetContent(), StringInputConverter, UriOutputConverter);

        public static ActivityResultContract<string, Java.Util.IList> GetMultipleContents()
            => new(new ActivityResultContracts.GetMultipleContents(), StringInputConverter, ListOutputConverter);

        public static ActivityResultContract<string[], Android.Net.Uri> OpenDocument()
            => new(new ActivityResultContracts.OpenDocument(), StringArrayInputConverter, UriOutputConverter);

        public static ActivityResultContract<Android.Net.Uri, Android.Net.Uri> OpenDocumentTree()
            => new(new ActivityResultContracts.OpenDocumentTree());

        public static ActivityResultContract<string[], Java.Util.IList> OpenMultipleDocuments()
            => new(new ActivityResultContracts.OpenMultipleDocuments(), StringArrayInputConverter, ListOutputConverter);

        public static ActivityResultContract<AndroidX.Activity.Result.ActivityResultUnit, Android.Net.Uri> PickContact()
            => new(new ActivityResultContracts.PickContact(), UnitInputConverter, UriOutputConverter);

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Java.Util.IList> PickMultipleVisualMedia()
            => new(new ActivityResultContracts.PickMultipleVisualMedia());

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Android.Net.Uri> PickVisualMedia()
            => new(new ActivityResultContracts.PickVisualMedia());

        public static ActivityResultContract<string[], Java.Util.IMap> RequestMultiplePermissions()
            => new(new ActivityResultContracts.RequestMultiplePermissions(), StringArrayInputConverter, MapOutputConverter);

        public static ActivityResultContract<string, bool> RequestPermission()
            => new(new ActivityResultContracts.RequestPermission(), StringInputConverter, BooleanOutputConverter);

        public static ActivityResultContract<Intent, AndroidX.Activity.Result.ActivityResult> StartActivityForResult()
            => new(new ActivityResultContracts.StartActivityForResult(), IntentInputConverter, ActivityResultOutputConverter);

        public static ActivityResultContract<AndroidX.Activity.Result.IntentSenderRequest, AndroidX.Activity.Result.ActivityResult> StartIntentSenderForResult()
            => new(new ActivityResultContracts.StartIntentSenderForResult(), IntentSenderRequestInputConverter, ActivityResultOutputConverter);

        public static ActivityResultContract<Android.Net.Uri, bool> TakePicture()
            => new(new ActivityResultContracts.TakePicture(), UriInputConverter, BooleanOutputConverter);

        public static ActivityResultContract<AndroidX.Activity.Result.ActivityResultUnit, Android.Graphics.Bitmap> TakePicturePreview()
            => new(new ActivityResultContracts.TakePicturePreview(), UnitInputConverter, BitmapOutputConverter);

        public static ActivityResultContract<Android.Net.Uri, Android.Graphics.Bitmap> TakeVideo()
            => new(new ActivityResultContracts.TakeVideo());

        static Java.Lang.Object? UriInputConverter(Android.Net.Uri? input)
            => input;

        static Java.Lang.Object? IntentInputConverter(Intent? input)
            => input;

        static Java.Lang.Object? IntentSenderRequestInputConverter(AndroidX.Activity.Result.IntentSenderRequest? input)
            => input;

        static Java.Lang.Object? StringInputConverter(string? input)
            => input is null ? null : new Java.Lang.String(input);

        static Java.Lang.Object? StringArrayInputConverter(string[]? input)
            => input is null ? null : new JavaArray<string>(input);

        static Java.Lang.Object? UnitInputConverter(AndroidX.Activity.Result.ActivityResultUnit unit)
            => null;

        static Android.Net.Uri? UriOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => null,
                Android.Net.Uri uri => uri,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(Android.Net.Uri).FullName}'."),
            };

        static Java.Util.IList? ListOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => null,
                Java.Util.IList list => list,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(Java.Util.IList).FullName}'."),
            };

        static Java.Util.IMap? MapOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => null,
                Java.Util.IMap map => map,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(Java.Util.IMap).FullName}'."),
            };

        static AndroidX.Activity.Result.ActivityResult? ActivityResultOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => null,
                AndroidX.Activity.Result.ActivityResult activityResult => activityResult,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(AndroidX.Activity.Result.ActivityResult).FullName}'."),
            };

        static Android.Graphics.Bitmap? BitmapOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => null,
                Android.Graphics.Bitmap bitmap => bitmap,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(Android.Graphics.Bitmap).FullName}'."),
            };

        static bool? BooleanOutputConverter(Java.Lang.Object? output)
            => output switch
            {
                null => default,
                Java.Lang.Boolean javaBoolean => javaBoolean.BooleanValue(),
                bool managedBoolean => managedBoolean,
                _ => throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(bool).FullName}'."),
            };
    }
}
}
