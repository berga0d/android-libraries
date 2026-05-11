#nullable enable

using System;
using Android.Content;
using Android.Runtime;
using AndroidX.Core.App;

namespace AndroidX.Activity.Result
{
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

    public void Launch(TInput input, ActivityOptionsCompat? options = null)
    {
        Java.Lang.Object? javaInput = input switch
        {
            null => null,
            Java.Lang.Object value => value,
            string value => value,
            string[] value => value,
            _ => throw new InvalidOperationException(
                $"Unsupported activity result input type '{typeof(TInput)}'.")
        };

        Native.Launch(javaInput, options);
    }

    public AndroidX.Activity.Result.Contract.ActivityResultContract<TInput> Contract
        => new(Native.Contract);

    public void Unregister()
        => Native.Unregister();
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

public static class ActivityResultRegistryTypedExtensions
{
    public static ActivityResultLauncher<TInput> Register<TInput, TResult>(
        this ActivityResultRegistry registry,
        string key,
        Contract.ActivityResultContract<TInput, TResult> contract,
        Action<TResult?> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new ActivityResultCallbackAdapter<TResult>(callback);
        var launcher = registry.Register(key, contract.Native, callbackAdapter);
        return new ActivityResultLauncher<TInput>(launcher);
    }

    public static ActivityResultLauncher<TInput> Register<TInput, TResult>(
        this ActivityResultRegistry registry,
        string key,
        AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner,
        Contract.ActivityResultContract<TInput, TResult> contract,
        Action<TResult?> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (lifecycleOwner is null)
            throw new ArgumentNullException(nameof(lifecycleOwner));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var callbackAdapter = new ActivityResultCallbackAdapter<TResult>(callback);
        var launcher = registry.Register(key, lifecycleOwner, contract.Native, callbackAdapter);
        return new ActivityResultLauncher<TInput>(launcher);
    }
}

}
namespace AndroidX.Activity.Result.Contract
{

public sealed class ActivityResultContract<TInput>
{
    internal AndroidX.Activity.Result.Contract.ActivityResultContract Native { get; }

    internal ActivityResultContract(AndroidX.Activity.Result.Contract.ActivityResultContract native)
        => Native = native ?? throw new ArgumentNullException(nameof(native));

    public Intent? CreateIntent(Context context, TInput input)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        Java.Lang.Object? javaInput = input switch
        {
            null => null,
            Java.Lang.Object value => value,
            string value => value,
            string[] value => value,
            _ => throw new InvalidOperationException(
                $"Unsupported activity result input type '{typeof(TInput)}'.")
        };

        return Native.CreateIntent(context, javaInput);
    }
}

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

        [Obsolete]
        public static ActivityResultContract<string, Android.Net.Uri> CreateDocument()
            => new(new ActivityResultContracts.CreateDocument());

        public static ActivityResultContract<string, Android.Net.Uri> CreateDocument(string mimeType)
            => new(new ActivityResultContracts.CreateDocument(mimeType));

        public static ActivityResultContract<string, Android.Net.Uri> GetContent()
            => new(new ActivityResultContracts.GetContent());

        public static ActivityResultContract<string, Java.Util.IList> GetMultipleContents()
            => new(new ActivityResultContracts.GetMultipleContents());

        public static ActivityResultContract<string[], Android.Net.Uri> OpenDocument()
            => new(new ActivityResultContracts.OpenDocument());

        public static ActivityResultContract<Android.Net.Uri?, Android.Net.Uri> OpenDocumentTree()
            => new(new ActivityResultContracts.OpenDocumentTree());

        public static ActivityResultContract<string[], Java.Util.IList> OpenMultipleDocuments()
            => new(new ActivityResultContracts.OpenMultipleDocuments());

        public static ActivityResultContract<Java.Lang.Void, Android.Net.Uri> PickContact()
            => new(new ActivityResultContracts.PickContact());

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Java.Util.IList> PickMultipleVisualMedia()
            => new(new ActivityResultContracts.PickMultipleVisualMedia());

        public static ActivityResultContract<AndroidX.Activity.Result.PickVisualMediaRequest, Java.Util.IList> PickMultipleVisualMedia(int maxItems)
            => new(new ActivityResultContracts.PickMultipleVisualMedia(maxItems));

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

        public static ActivityResultContract<Java.Lang.Void, Android.Graphics.Bitmap> TakePicturePreview()
            => new(new ActivityResultContracts.TakePicturePreview());

        [Obsolete]
        public static ActivityResultContract<Android.Net.Uri, Android.Graphics.Bitmap> TakeVideo()
            => new(new ActivityResultContracts.TakeVideo());
    }
}
}
