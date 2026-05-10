#nullable enable

using System;
using Android.Content;
using Android.Runtime;
using AndroidX.Core.App;

namespace AndroidX.Activity.Result
{

public readonly struct ActivityResultUnit
{
    public static ActivityResultUnit Value => default;
}

internal sealed class ActivityResultCallbackAdapter<TResult> : Java.Lang.Object, IActivityResultCallback
{
    readonly Action<TResult?> callback;

    public ActivityResultCallbackAdapter(Action<TResult?> callback)
    {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public void OnActivityResult(Java.Lang.Object? result)
        => callback(ConvertResult(result));

    static TResult? ConvertResult(Java.Lang.Object? result)
    {
        if (result is null)
            return default;

        if (result is TResult typed)
            return typed;

        if (typeof(TResult) == typeof(bool) && result is Java.Lang.Boolean javaBoolean)
            return (TResult?)(object)javaBoolean.BooleanValue();

        if (typeof(IJavaObject).IsAssignableFrom(typeof(TResult)))
            return result is TResult javaTyped ? javaTyped : default;

        throw new InvalidCastException($"Cannot cast '{result.GetType().FullName}' to '{typeof(TResult).FullName}'.");
    }
}

public sealed class ActivityResultLauncher<TInput>
{
    internal AndroidX.Activity.Result.ActivityResultLauncher Native { get; }

    internal ActivityResultLauncher(AndroidX.Activity.Result.ActivityResultLauncher native)
        => Native = native ?? throw new ArgumentNullException(nameof(native));

    public void Unregister()
        => Native.Unregister();
}

public sealed class ActivityResultCallerLauncher<TInput>
{
    readonly ActivityResultLauncher<TInput> launcher;
    readonly TInput input;

    internal ActivityResultCallerLauncher(ActivityResultLauncher<TInput> launcher, TInput input)
    {
        this.launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        this.input = input;
    }

    public void Launch()
        => Launch(null);

    public void Launch(ActivityOptionsCompat? options)
    {
        if (typeof(TInput) == typeof(ActivityResultUnit))
        {
            launcher.Native.Launch(null, options);
            return;
        }

        if (input is Java.Lang.Object javaInput)
        {
            launcher.Native.Launch(javaInput, options);
            return;
        }

        if (input is string stringInput)
        {
            launcher.Native.Launch(stringInput, options);
            return;
        }

        if (input is string[] stringArrayInput)
        {
            launcher.Native.Launch(stringArrayInput, options);
            return;
        }

        throw new InvalidCastException($"Cannot marshal '{typeof(TInput).FullName}' to Java.Lang.Object.");
    }

    public void Unregister()
        => launcher.Unregister();
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
        => launcher.Native.Launch(input, options);

    public static void Launch(
        this ActivityResultLauncher<string[]> launcher,
        string[] input,
        ActivityOptionsCompat? options = null)
        => launcher.Native.Launch(input, options);

    public static void Launch(
        this ActivityResultLauncher<ActivityResultUnit> launcher,
        ActivityOptionsCompat? options = null)
        => launcher.Native.Launch(null, options);
}

public static class ActivityResultCallerTypedExtensions
{
    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        Contract.ActivityResultContract<TInput, TResult> contract,
        Action<TResult?> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new ActivityResultCallbackAdapter<TResult>(callback);
        var launcher = caller.RegisterForActivityResult(contract.Native, callbackAdapter);
        return new ActivityResultLauncher<TInput>(launcher);
    }

    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        Contract.ActivityResultContract<TInput, TResult> contract,
        ActivityResultRegistry registry,
        Action<TResult?> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new ActivityResultCallbackAdapter<TResult>(callback);
        var launcher = caller.RegisterForActivityResult(contract.Native, registry, callbackAdapter);
        return new ActivityResultLauncher<TInput>(launcher);
    }
}

}

namespace AndroidX.Activity.Result.Contract
{

public sealed class ActivityResultContract<TInput, TResult>
{
    internal AndroidX.Activity.Result.Contract.ActivityResultContract Native { get; }

    public ActivityResultContract(AndroidX.Activity.Result.Contract.ActivityResultContract native)
        => Native = native ?? throw new ArgumentNullException(nameof(native));
}

public partial class ActivityResultContracts
{
    public static class Typed
    {
        public static ActivityResultContract<Android.Net.Uri, bool> CaptureVideo()
            => new(new ActivityResultContracts.CaptureVideo());

        public static ActivityResultContract<string, Android.Net.Uri> CreateDocument()
            => new(new ActivityResultContracts.CreateDocument());

        public static ActivityResultContract<string, Android.Net.Uri> GetContent()
            => new(new ActivityResultContracts.GetContent());

        public static ActivityResultContract<string, Java.Util.IList> GetMultipleContents()
            => new(new ActivityResultContracts.GetMultipleContents());

        public static ActivityResultContract<string[], Android.Net.Uri> OpenDocument()
            => new(new ActivityResultContracts.OpenDocument());

        public static ActivityResultContract<Android.Net.Uri, Android.Net.Uri> OpenDocumentTree()
            => new(new ActivityResultContracts.OpenDocumentTree());

        public static ActivityResultContract<string[], Java.Util.IList> OpenMultipleDocuments()
            => new(new ActivityResultContracts.OpenMultipleDocuments());

        public static ActivityResultContract<AndroidX.Activity.Result.ActivityResultUnit, Android.Net.Uri> PickContact()
            => new(new ActivityResultContracts.PickContact());

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Java.Util.IList> PickMultipleVisualMedia()
            => new(new ActivityResultContracts.PickMultipleVisualMedia());

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Android.Net.Uri> PickVisualMedia()
            => new(new ActivityResultContracts.PickVisualMedia());

        public static ActivityResultContract<string[], Java.Util.IMap> RequestMultiplePermissions()
            => new(new ActivityResultContracts.RequestMultiplePermissions());

        public static ActivityResultContract<string, bool> RequestPermission()
            => new(new ActivityResultContracts.RequestPermission());

        public static ActivityResultContract<Intent, AndroidX.Activity.Result.ActivityResult> StartActivityForResult()
            => new(new ActivityResultContracts.StartActivityForResult());

        public static ActivityResultContract<AndroidX.Activity.Result.IntentSenderRequest, AndroidX.Activity.Result.ActivityResult> StartIntentSenderForResult()
            => new(new ActivityResultContracts.StartIntentSenderForResult());

        public static ActivityResultContract<Android.Net.Uri, bool> TakePicture()
            => new(new ActivityResultContracts.TakePicture());

        public static ActivityResultContract<AndroidX.Activity.Result.ActivityResultUnit, Android.Graphics.Bitmap> TakePicturePreview()
            => new(new ActivityResultContracts.TakePicturePreview());

        public static ActivityResultContract<Android.Net.Uri, Android.Graphics.Bitmap> TakeVideo()
            => new(new ActivityResultContracts.TakeVideo());
    }
}
}
