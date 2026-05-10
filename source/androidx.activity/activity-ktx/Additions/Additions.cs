#nullable enable

using System;
using AndroidX.Core.App;

namespace AndroidX.Activity.Result
{
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
            Java.Lang.Object? javaInput = input switch
            {
                null => null,
                Java.Lang.Object value => value,
                string value => value,
                string[] value => value,
                _ => throw new InvalidCastException($"Unsupported input type for activity result launch: '{typeof(TInput).FullName}'.")
            };

            launcher.Native.Launch(javaInput, options);
        }

        public void Unregister()
            => launcher.Unregister();
    }

    public static class ActivityResultCallerKtxTypedExtensions
    {
        public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            TInput input,
            Action<TResult?> callback)
            => new(AndroidX.Activity.Result.ActivityResultCallerTypedExtensions.RegisterForActivityResult(caller, contract, callback), input);

        public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            ActivityResultRegistry registry,
            TInput input,
            Action<TResult?> callback)
            => new(AndroidX.Activity.Result.ActivityResultCallerTypedExtensions.RegisterForActivityResult(caller, contract, registry, callback), input);
    }
}
