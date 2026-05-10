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
            if (typeof(TInput) == typeof(Java.Lang.Void))
            {
                launcher.Native.Launch((Java.Lang.Void?)null, options);
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

            throw new InvalidCastException($"Unsupported input type for activity result launch: '{typeof(TInput).FullName}'.");
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
