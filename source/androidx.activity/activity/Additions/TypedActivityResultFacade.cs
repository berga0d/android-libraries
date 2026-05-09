#nullable enable

using System;
using Android.Content;
using AndroidX.Core.App;
using Java.Interop;

namespace AndroidX.Activity.Result
{
    sealed class ActivityResultCallbackAdapter<TResult> : Java.Lang.Object, global::AndroidX.Activity.Result.IActivityResultCallback
    {
        readonly Action<TResult?> callback;
        readonly Func<Java.Lang.Object?, TResult?> outputCast;

        public ActivityResultCallbackAdapter(Action<TResult?> callback, Func<Java.Lang.Object?, TResult?> outputCast)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.outputCast = outputCast ?? throw new ArgumentNullException(nameof(outputCast));
        }

        public void OnActivityResult(Java.Lang.Object? result)
            => callback(outputCast(result));
    }

    public sealed class ActivityResultLauncher<TInput>
    {
        readonly global::AndroidX.Activity.Result.ActivityResultLauncher rawLauncher;
        readonly Func<TInput?, Java.Lang.Object?> inputCast;

        internal ActivityResultLauncher(global::AndroidX.Activity.Result.ActivityResultLauncher rawLauncher, Func<TInput?, Java.Lang.Object?> inputCast)
        {
            this.rawLauncher = rawLauncher ?? throw new ArgumentNullException(nameof(rawLauncher));
            this.inputCast = inputCast ?? throw new ArgumentNullException(nameof(inputCast));
        }

        public void Launch(TInput? input)
            => rawLauncher.Launch(inputCast(input));

        public void Launch(TInput? input, ActivityOptionsCompat? options)
            => rawLauncher.Launch(inputCast(input), options);

        public void Unregister()
            => rawLauncher.Unregister();
    }
}

namespace AndroidX.Activity.Result.Contract
{
    public sealed class ActivityResultContract<TInput, TResult>
    {
        readonly ActivityResultContract rawContract;

        public ActivityResultContract(ActivityResultContract rawContract)
        {
            this.rawContract = rawContract ?? throw new ArgumentNullException(nameof(rawContract));
        }

        internal Java.Lang.Object? MarshalInput(TInput? input)
        {
            if (input is null)
                return null;

            if (input is Java.Lang.Object javaObject)
                return javaObject;

            throw new InvalidCastException($"Cannot marshal '{typeof(TInput).FullName}' to Java.Lang.Object. Use Java-bound types for typed activity-result wrappers.");
        }

        internal TResult? MarshalOutput(Java.Lang.Object? output)
        {
            if (output is null)
                return default;

            if (output is TResult typed)
                return typed;

            throw new InvalidCastException($"Cannot cast '{output.GetType().FullName}' to '{typeof(TResult).FullName}'.");
        }

        public Intent CreateIntent(Context context, TInput? input)
            => rawContract.CreateIntent(context, MarshalInput(input));

        public TResult? ParseResult(int resultCode, Intent? intent)
            => MarshalOutput(rawContract.ParseResult(resultCode, intent));

        public ActivityResultContract.SynchronousResult? GetSynchronousResult(Context context, TInput? input)
            => rawContract.GetSynchronousResult(context, MarshalInput(input));

        public global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult(global::AndroidX.Activity.Result.IActivityResultCaller caller, Action<TResult?> callback)
        {
            if (caller is null)
                throw new ArgumentNullException(nameof(caller));
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var rawCallback = new global::AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, MarshalOutput);
            var launcher = caller.RegisterForActivityResult(rawContract, rawCallback);
            return new global::AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher, MarshalInput);
        }

        public global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult(global::AndroidX.Activity.Result.IActivityResultCaller caller, global::AndroidX.Activity.Result.ActivityResultRegistry registry, Action<TResult?> callback)
        {
            if (caller is null)
                throw new ArgumentNullException(nameof(caller));
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var rawCallback = new global::AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, MarshalOutput);
            var launcher = caller.RegisterForActivityResult(rawContract, registry, rawCallback);
            return new global::AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher, MarshalInput);
        }

        public global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> Register(global::AndroidX.Activity.Result.ActivityResultRegistry registry, string key, Action<TResult?> callback)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var rawCallback = new global::AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, MarshalOutput);
            var launcher = registry.Register(key, rawContract, rawCallback);
            return new global::AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher, MarshalInput);
        }

        public global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> Register(global::AndroidX.Activity.Result.ActivityResultRegistry registry, string key, global::AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner, Action<TResult?> callback)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));
            if (lifecycleOwner is null)
                throw new ArgumentNullException(nameof(lifecycleOwner));
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var rawCallback = new global::AndroidX.Activity.Result.ActivityResultCallbackAdapter<TResult>(callback, MarshalOutput);
            var launcher = registry.Register(key, lifecycleOwner, rawContract, rawCallback);
            return new global::AndroidX.Activity.Result.ActivityResultLauncher<TInput>(launcher, MarshalInput);
        }
    }

    public partial class ActivityResultContracts
    {
        public static class Typed
        {
            public static ActivityResultContract<Android.Net.Uri, Java.Lang.Boolean> CaptureVideo()
                => new(new ActivityResultContracts.CaptureVideo());

            public static ActivityResultContract<Java.Lang.String, Android.Net.Uri> CreateDocument()
                => new(new ActivityResultContracts.CreateDocument());

            public static ActivityResultContract<Java.Lang.String, Android.Net.Uri> GetContent()
                => new(new ActivityResultContracts.GetContent());

            public static ActivityResultContract<Java.Lang.String, Java.Util.IList> GetMultipleContents()
                => new(new ActivityResultContracts.GetMultipleContents());

            public static ActivityResultContract<JavaArray<Java.Lang.String>, Android.Net.Uri> OpenDocument()
                => new(new ActivityResultContracts.OpenDocument());

            public static ActivityResultContract<Android.Net.Uri, Android.Net.Uri> OpenDocumentTree()
                => new(new ActivityResultContracts.OpenDocumentTree());

            public static ActivityResultContract<JavaArray<Java.Lang.String>, Java.Util.IList> OpenMultipleDocuments()
                => new(new ActivityResultContracts.OpenMultipleDocuments());

            public static ActivityResultContract<Java.Lang.Void, Android.Net.Uri> PickContact()
                => new(new ActivityResultContracts.PickContact());

            public static ActivityResultContract<global::AndroidX.Activity.Result.PickVisualMediaRequest, Java.Util.IList> PickMultipleVisualMedia()
                => new(new ActivityResultContracts.PickMultipleVisualMedia());

            public static ActivityResultContract<global::AndroidX.Activity.Result.PickVisualMediaRequest, Android.Net.Uri> PickVisualMedia()
                => new(new ActivityResultContracts.PickVisualMedia());

            public static ActivityResultContract<JavaArray<Java.Lang.String>, Java.Util.IMap> RequestMultiplePermissions()
                => new(new ActivityResultContracts.RequestMultiplePermissions());

            public static ActivityResultContract<Java.Lang.String, Java.Lang.Boolean> RequestPermission()
                => new(new ActivityResultContracts.RequestPermission());

            public static ActivityResultContract<Intent, global::AndroidX.Activity.Result.ActivityResult> StartActivityForResult()
                => new(new ActivityResultContracts.StartActivityForResult());

            public static ActivityResultContract<global::AndroidX.Activity.Result.IntentSenderRequest, global::AndroidX.Activity.Result.ActivityResult> StartIntentSenderForResult()
                => new(new ActivityResultContracts.StartIntentSenderForResult());

            public static ActivityResultContract<Android.Net.Uri, Java.Lang.Boolean> TakePicture()
                => new(new ActivityResultContracts.TakePicture());

            public static ActivityResultContract<Java.Lang.Void, Android.Graphics.Bitmap> TakePicturePreview()
                => new(new ActivityResultContracts.TakePicturePreview());

            public static ActivityResultContract<Android.Net.Uri, Android.Graphics.Bitmap> TakeVideo()
                => new(new ActivityResultContracts.TakeVideo());
        }
    }
}
